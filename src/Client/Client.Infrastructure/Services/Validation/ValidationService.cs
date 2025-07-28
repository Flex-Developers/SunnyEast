using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Client.Infrastructure.Services.Validation;

public class ValidationService : IValidationService
{
    public string ValidateEmail(string email)
    {
        if (!string.IsNullOrWhiteSpace(email) && (!email.Contains("@") || !email.Contains(".")))
            return "Почта должна содержать символ '@' и домен.";

        return null;
    }

    public void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ValidationException("Пароль не должен быть пустым.");

        if (password.Length < 8)
            throw new ValidationException("Пароль должен содержать не менее 8 символов.");

        if (!password.Any(char.IsLetter))
            throw new ValidationException("Пароль должен содержать хотя бы одну букву.");

        if (!password.Any(c => char.IsLetter(c) && char.IsUpper(c)))
            throw new ValidationException("Пароль должен содержать хотя бы одну заглавную букву.");

        if (!password.Any(c => char.IsLetter(c) && char.IsLower(c)))
            throw new ValidationException("Пароль должен содержать хотя бы одну строчную букву.");

        if (!password.Any(char.IsDigit))
            throw new ValidationException("Пароль должен содержать хотя бы одну цифру.");
    }

    public void ValidatePhoneNumber(string phoneNumber)
    {
        if (phoneNumber.Length != 10 || !phoneNumber.All(char.IsDigit))
            throw new ValidationException("Некорректный номер телефона.");
    }

    public void ValidateNames(string name, string surname)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Имя не может быть пустым.");

        if (name.Any(char.IsDigit) || surname.Any(char.IsDigit))
            throw new ValidationException("Имя или фамилия не должны содержать цифры.");

        if (name.Any(c => !char.IsLetter(c) && c != ' ') || surname.Any(c => !char.IsLetter(c) && c != ' '))
            throw new ValidationException("Имя и фамилия могут содержать только буквы и пробелы.");
    }

    public string? ValidateFirstName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Имя обязательно.";

        if (name.Length < 2 || name.Length > 50)
            return "Длина имени должна быть от 2 до 50 символов.";

        if (!Regex.IsMatch(name, @"^[a-zA-Zа-яА-ЯёЁ]+$"))
            return "Имя может содержать только буквы.";

        return null;
    }

    public string? ValidateLastName(string surname, bool allowEmpty = false)
    {
        if (string.IsNullOrWhiteSpace(surname))
            return allowEmpty ? null : "Фамилия обязательна.";

        if (surname.Length < 2 || surname.Length > 50)
            return "Длина фамилии должна быть от 2 до 50 символов.";

        if (!Regex.IsMatch(surname, @"^[a-zA-Zа-яА-ЯёЁ]+$"))
            return "Фамилия может содержать только буквы.";

        return null;

    }
}