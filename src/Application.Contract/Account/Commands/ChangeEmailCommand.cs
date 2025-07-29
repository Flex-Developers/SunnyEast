using System.ComponentModel.DataAnnotations;

namespace Application.Contract.Account.Commands;

public sealed class ChangeEmailCommand : IRequest<Unit>
{
    [Required, EmailAddress]
    public required string NewEmail { get; set; }
}