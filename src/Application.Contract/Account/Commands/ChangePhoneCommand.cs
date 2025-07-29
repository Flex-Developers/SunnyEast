using System.ComponentModel.DataAnnotations;

namespace Application.Contract.Account.Commands;

public sealed class ChangePhoneCommand : IRequest<Unit>
{
    // Внутри сохраняем как +7-XXX-XXX-XX-XX
    [Required]
    [RegularExpression(@"^\+7-\d{3}-\d{3}-\d{2}-\d{2}$", ErrorMessage = "Телефон должен быть в формате +7-XXX-XXX-XX-XX")]
    public required string NewPhone { get; set; }
}