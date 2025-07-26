using Application.Contract.Staff.Responses;

namespace Application.Contract.Staff.Queries;

public sealed class GetStaffByUserIdQuery : IRequest<StaffResponse?>
{
    public Guid UserId { get; set; }
}