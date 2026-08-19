using AccuFlow.Application.Common.Interfaces;
using AccuFlow.Application.Features.DocumentAttachments.Commands;
using AccuFlow.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace AccuFlow.Application.Features.DocumentAttachments.Handlers
{
    public class UploadAttachmentCommandHandler : IRequestHandler<UploadAttachmentCommand, DocumentAttachmentEntity>
    {
        private readonly IRepository<DocumentAttachmentEntity> _attachmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public UploadAttachmentCommandHandler(
            IRepository<DocumentAttachmentEntity> attachmentRepository,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _attachmentRepository = attachmentRepository;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _environment = environment;
        }

        public async Task<DocumentAttachmentEntity> Handle(UploadAttachmentCommand request, CancellationToken cancellationToken)
        {
            var uploadRequest = request.Request;
            var file = request.File;
            var userId = request.UserId;

            if (file == null || file.Length == 0) throw new Exception("File is required");

            var maxFileSizeMb = _configuration.GetValue<int?>("Storage:MaxFileSizeMb") ?? 10;
            if (file.Length > maxFileSizeMb * 1024L * 1024L) throw new Exception($"File size must be less than {maxFileSizeMb} MB");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = _configuration.GetSection("Storage:AllowedExtensions").Get<string[]>() ?? Array.Empty<string>();
            if (!allowedExtensions.Contains(extension)) throw new Exception("File extension is not allowed");

            var uploadRoot = GetUploadRoot();
            var documentFolder = Path.Combine(uploadRoot, uploadRequest.DocumentType, uploadRequest.DocumentId.ToString());
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
                DocumentType = uploadRequest.DocumentType,
                DocumentId = uploadRequest.DocumentId,
                FileName = Path.GetFileName(file.FileName),
                StoredFileName = storedFileName,
                FilePath = relativePath,
                ContentType = file.ContentType,
                FileSize = file.Length,
                Description = uploadRequest.Description,
                CreatedBy = userId.ToString()
            };

            _attachmentRepository.Add(attachment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return attachment;
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