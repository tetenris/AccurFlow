# SECURITY FEATURES

## 1. Authentication

### Password Security
**Implementation**: Koneksi.vb
- **Algorithm**: PBKDF2 (Password-Based Key Derivation Function 2)
- **Iterations**: 10,000
- **Hash Size**: 256-bit (32 bytes)
- **Salt**: Random, unique per user
- **Storage**: Separate Hash and Salt columns

### Password Hashing Process:
```vb
Function HashPassword(password As String, saltSize As Integer) 
    As (Hash As String, Salt As String)
    ' Generate random salt
    Dim saltBytes(saltSize - 1) As Byte
    Using rng As New RNGCryptoServiceProvider()
        rng.GetBytes(saltBytes)
    End Using
    Dim salt As String = Convert.ToBase64String(saltBytes)
    
    ' Hash password + salt
    Dim pbkdf2 As New Rfc2898DeriveBytes(password, saltBytes, 10000)
    Dim hash As Byte() = pbkdf2.GetBytes(32)
    
    Return (Convert.ToBase64String(hash), salt)
End Function
```

### Password Verification:
```vb
Function VerifyPassword(password As String, 
                       storedHash As String, 
                       storedSalt As String) As Boolean
    Dim saltBytes As Byte() = Convert.FromBase64String(storedSalt)
    Dim pbkdf2 As New Rfc2898DeriveBytes(password, saltBytes, 10000)
    Dim hash As Byte() = pbkdf2.GetBytes(32)
    
    Return Convert.ToBase64String(hash) = storedHash
End Function
```


## 2. Authorization

### Role-Based Access Control (RBAC)
**Roles Defined in ConstConfig.vb:**
- **SuperAdministrator** - Full system access
- **Administrator** - Admin access
- **Admin** - Standard admin
- **Report** - Report viewing only

### User Table Structure:
```
Pengguna (User)
- UserId (PK)
- Nama (Username)
- NamaPanjang (Full Name)
- Password (Hashed)
- Salt (for password)
- Status (Role)
```

### Session Management:
- Current user stored in: MENUUTAMA.Label6.Text
- Used for audit trail (CreatedBy, ModifiedBy, DeletedBy)

## 3. Data Security

### Soft Delete Pattern
All tables use soft delete instead of hard delete:
- DeletedBy: Username who deleted
- DeletedDate: Timestamp of deletion
- Queries filter: `WHERE DeletedBy IS NULL`

### Audit Trail
Every transaction tracks:
- **CreatedBy**: User who created record
- **CreatedDate**: Creation timestamp
- **ModifiedBy**: User who last modified
- **ModifiedDate**: Last modification timestamp
- **DeletedBy**: User who deleted (soft delete)
- **DeletedDate**: Deletion timestamp


## 4. SQL Injection Prevention

### Current Implementation Issues:
⚠️ **WARNING**: Application uses string concatenation for SQL queries
```vb
' VULNERABLE CODE EXAMPLE:
STR = "select * from Profile where nama = '" & txtNama.Text & "'"
```

### Recommended Fix:
Use parameterized queries:
```vb
' SECURE CODE:
CMD = New SqlCommand("SELECT * FROM Profile WHERE nama = @nama", CONN)
CMD.Parameters.AddWithValue("@nama", txtNama.Text)
```

## 5. Connection Security

### Database Connection
- **Server**: 103.82.242.240 (Remote)
- **Authentication**: SQL Server Authentication
- **Encrypted**: ❌ No (should use encrypted connection)
- **Connection String**: Stored in app.config

### FTP Configuration
- **Server**: ftp://103.82.242.240
- **Credentials**: Hardcoded in ConstConfig.vb
  - User: "cent"
  - Password: "Alhamdulillah"
- ⚠️ **Security Risk**: Credentials should be encrypted/secured

## 6. Security Recommendations for Web Migration

1. **Use Parameterized Queries** - Prevent SQL injection
2. **Implement JWT/OAuth** - Modern authentication
3. **HTTPS Only** - Encrypt all traffic
4. **Environment Variables** - Store sensitive config
5. **Rate Limiting** - Prevent brute force
6. **Input Validation** - Server-side validation
7. **CORS Policy** - Control API access
8. **Audit Logging** - Enhanced audit trail
9. **Password Policy** - Enforce strong passwords
10. **Session Timeout** - Auto logout inactive users

