using System.ComponentModel.DataAnnotations;

namespace Application.Contract.Account.Commands;

public sealed class UpdateProfileCommand : IRequest<Unit>
{
    [Required, MinLength(2), MaxLength(50)]
    [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\-'\s]+$", ErrorMessage = "Имя может содержать только буквы и дефис.")]
    public required string Name { get; set; }

    [Required, MinLength(2), MaxLength(50)]
    [RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ\-'\s]+$", ErrorMessage = "Фамилия может содержать только буквы и дефис.")]
    public required string Surname { get; set; }
}