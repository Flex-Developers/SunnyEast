namespace Application.Contract.Database.Commands;

public sealed record RestoreDatabaseCommand(Stream FileStream, string FileName) : IRequest<Unit>;