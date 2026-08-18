using AccuFlow.Infrastructure.Persistence;
using AccuFlow.Domain.Entities;
using AccuFlow.Models.AgingReport;
using AccuFlow.Models.Approval;
using AccuFlow.Models.BaseModel;
using AccuFlow.Models.DocumentAttachment;
using AccuFlow.Models.Inventory;
using AccuFlow.Models.ReceivablePayable;
using AccuFlow.Models.Tax;
using Microsoft.EntityFrameworkCore;

namespace AccuFlow.Services
{
    public interface IApprovalService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTableApprovalRequest request);
        Task Submit(SubmitApprovalRequest request, Guid userId);
        Task Approve(ApprovalActionRequest request, Guid userId);
        Task Reject(ApprovalActionRequest request, Guid userId);
    }

    public class ApprovalService : BaseService, IApprovalService
    {
        public ApprovalService(AppDbContext dbContext) : base(dbContext) { }

        public async Task<BaseDatatableResponse> Datatable(DataTableApprovalRequest request)
        {
            var query = _dbContext.ApprovalRequests.Include(x => x.RequestedByUser).Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(request.DocumentType)) query = query.Where(x => x.DocumentType == request.DocumentType);
            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.RequestedAt).Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new ApprovalRequestViewModel
                {
                    ApprovalRequestId = x.ApprovalRequestId,
                    DocumentType = x.DocumentType,
                    DocumentId = x.DocumentId,
                    Status = x.Status,
                    RequestedByName = x.RequestedByUser.FullName,
                    RequestedAt = x.RequestedAt,
                    Notes = x.Notes
                }).ToListAsync();
            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task Submit(SubmitApprovalRequest request, Guid userId)
        {
            _dbContext.ApprovalRequests.Add(new ApprovalRequestEntity
            {
                ApprovalRequestId = Guid.NewGuid(),
                DocumentType = request.DocumentType,
                DocumentId = request.DocumentId,
                Status = "Pending",
                RequestedBy = userId,
                RequestedAt = DateTime.UtcNow,
                CurrentApproverId = request.CurrentApproverId,
                Notes = request.Notes,
                CreatedBy = userId.ToString()
            });
            await _dbContext.SaveChangesAsync();
        }

        public Task Approve(ApprovalActionRequest request, Guid userId) => ChangeStatus(request, userId, "Approved");
        public Task Reject(ApprovalActionRequest request, Guid userId) => ChangeStatus(request, userId, "Rejected");

        private async Task ChangeStatus(ApprovalActionRequest request, Guid userId, string status)
        {
            var approval = await _dbContext.ApprovalRequests.FirstOrDefaultAsync(x => x.ApprovalRequestId == request.ApprovalRequestId && !x.IsDeleted);
            if (approval == null) throw new Exception("Approval request not found");
            approval.Status = status;
            approval.UpdatedBy = userId.ToString();
            _dbContext.ApprovalHistories.Add(new ApprovalHistoryEntity
            {
                ApprovalHistoryId = Guid.NewGuid(),
                ApprovalRequestId = approval.ApprovalRequestId,
                ApproverId = userId,
                Action = status,
                ActionAt = DateTime.UtcNow,
                Notes = request.Notes,
                CreatedBy = userId.ToString()
            });
            await _dbContext.SaveChangesAsync();
        }
    }

    public interface IDocumentAttachmentService : IBaseService
    {
        Task<BaseDatatableResponse> Datatable(DataTableDocumentAttachmentRequest request);
        Task<List<DocumentAttachmentViewModel>> GetByDocument(string documentType, Guid documentId);
        Task<DocumentAttachmentEntity> Upload(UploadDocumentAttachmentRequest request, IFormFile file, Guid userId);
        Task<DocumentAttachmentEntity> GetFile(Guid id);
        Task Delete(Guid id, Guid userId);
    }

    public class DocumentAttachmentService : BaseService, IDocumentAttachmentService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public DocumentAttachmentService(AppDbContext dbContext, IConfiguration configuration, IWebHostEnvironment environment) : base(dbContext)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public async Task<BaseDatatableResponse> Datatable(DataTableDocumentAttachmentRequest request)
        {
            var query = _dbContext.DocumentAttachments.Where(x => !x.IsDeleted);
            if (!string.IsNullOrWhiteSpace(request.DocumentType)) query = query.Where(x => x.DocumentType == request.DocumentType);
            if (request.DocumentId.HasValue) query = query.Where(x => x.DocumentId == request.DocumentId);
            var total = await query.CountAsync();
            var data = await query.OrderByDescending(x => x.CreatedAt).Skip((request.Page - 1) * request.Size).Take(request.Size)
                .Select(x => new DocumentAttachmentViewModel
                {
                    DocumentAttachmentId = x.DocumentAttachmentId,
                    DocumentType = x.DocumentType,
                    DocumentId = x.DocumentId,
                    FileName = x.FileName,
                    ContentType = x.ContentType,
                    FileSize = x.FileSize,
                    Description = x.Description,
                    CreatedAt = x.CreatedAt
                }).ToListAsync();
            return new BaseDatatableResponse { Draw = request.Draw, RecordsTotal = total, RecordsFiltered = total, Data = data };
        }

        public async Task<List<DocumentAttachmentViewModel>> GetByDocument(string documentType, Guid documentId)
        {
            return await _dbContext.DocumentAttachments
                .Where(x => x.DocumentType == documentType && x.DocumentId == documentId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new DocumentAttachmentViewModel
                {
                    DocumentAttachmentId = x.DocumentAttachmentId,
                    DocumentType = x.DocumentType,
                    DocumentId = x.DocumentId,
                    FileName = x.FileName,
                    ContentType = x.ContentType,
                    FileSize = x.FileSize,
                    Description = x.Description,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<DocumentAttachmentEntity> Upload(UploadDocumentAttachmentRequest request, IFormFile file, Guid userId)
        {
            if (file == null || file.Length == 0) throw new Exception("File is required");

            var maxFileSizeMb = _configuration.GetValue<int?>("Storage:MaxFileSizeMb") ?? 10;
            if (file.Length > maxFileSizeMb * 1024L * 1024L) throw new Exception($"File size must be less than {maxFileSizeMb} MB");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = _configuration.GetSection("Storage:AllowedExtensions").Get<string[]>() ?? Array.Empty<string>();
            if (!allowedExtensions.Contains(extension)) throw new Exception("File extension is not allowed");

            var uploadRoot = GetUploadRoot();
            var documentFolder = Path.Combine(uploadRoot, request.DocumentType, request.DocumentId.ToString());
            Directory.CreateDirectory(documentFolder);

            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(documentFolder, storedFileName);

            await using (var stream = new FileStream(filePath, FileMode.CreateNew))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = Path.GetRelativePath(_environment.ContentRootPath, filePath).Replace('\\', '/');
            var attachment = new DocumentAttachmentEntity
            {
                DocumentAttachmentId = Guid.NewGuid(),
                DocumentType = request.DocumentType,
                DocumentId = request.DocumentId,
                FileName = Path.GetFileName(file.FileName),
                StoredFileName = storedFileName,
                FilePath = relativePath,
                ContentType = file.ContentType,
                FileSize = file.Length,
                Description = request.Description,
                CreatedBy = userId.ToString()
            };

            _dbContext.DocumentAttachments.Add(attachment);
            await _dbContext.SaveChangesAsync();
            return attachment;
        }

        public async Task<DocumentAttachmentEntity> GetFile(Guid id)
        {
            var attachment = await _dbContext.DocumentAttachments.FirstOrDefaultAsync(x => x.DocumentAttachmentId == id && !x.IsDeleted);
            if (attachment == null) throw new Exception("Attachment not found");
            return attachment;
        }

        public async Task Delete(Guid id, Guid userId)
        {
            var attachment = await _dbContext.DocumentAttachments.FirstOrDefaultAsync(x => x.DocumentAttachmentId == id && !x.IsDeleted);
            if (attachment == null) throw new Exception("Attachment not found");

            var filePath = Path.Combine(_environment.ContentRootPath, attachment.FilePath);
            if (File.Exists(filePath)) File.Delete(filePath);

            attachment.IsDeleted = true;
            attachment.DeletedBy = userId.ToString();
            attachment.DeletedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        private string GetUploadRoot()
        {
            var configuredPath = _configuration.GetValue<string>("Storage:UploadsPath") ?? "wwwroot/uploads";
            return Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(_environment.ContentRootPath, configuredPath);
        }
    }
}


