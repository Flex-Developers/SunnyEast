namespace Application.Common.Interfaces.Services;

public interface IDateTimeService
{
    DateTime UtcNow  { get; }
    DateTime Moscow  { get; }
}