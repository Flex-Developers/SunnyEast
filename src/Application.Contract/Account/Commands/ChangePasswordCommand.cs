using System.ComponentModel.DataAnnotations;

namespace Application.Contract.Account.Commands;

public sealed class ChangePasswordCommand : IRequest<Unit>
{
    [Required]
    public required string CurrentPassword { get; set; }

    [Required, MinLength(8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
        ErrorMessage = "Новый пароль должен содержать минимум 8 символов, цифру, строчную и заглавную буквы.")]
    public required string NewPassword { get; set; }
}