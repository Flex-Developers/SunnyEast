namespace Application.Contract.Database.Responses;

public sealed record DatabaseBackupResponse(byte[] Content, string FileName, string ContentType = "application/gzip");