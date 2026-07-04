namespace Application.Contract.File.Responses;

public record FileResult(byte[] Data, string ContentType, string FileName);