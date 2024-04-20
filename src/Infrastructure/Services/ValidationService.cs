using System.ComponentModel.DataAnnotations;
using Application.Common.Interfaces.Services;
using System.Text.RegularExpressions;

namespace Infrastructure.Services;

public class ValidationService : IValidationService
{
    public void ValidateEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ValidationException("Email не должен быть пустым.");

        if (!email.Contains("@") || !email.Contains("."))
            throw new ValidationException("Email должен содержать символ '@' и домен.");
    }

    public void ValidatePasswordAsync(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ValidationException("Пароль не должен быть пустым.");

        if (password.Length < 8)
            throw new ValidationException("Пароль должен содержать не менее 8 символов.");

        if (!password.Any(char.IsLetter))
            throw new ValidationException("Пароль должен содержать хотя бы одну букву.");

        if (!password.Any(c => char.IsLetter(c) && char.IsUpper(c)))
            throw new ValidationException("Пароль должен содержать хотя бы одну заглавную букву.");

        if (!password.Any(char.IsDigit))
            throw new ValidationException("Пароль должен содержать хотя бы одну цифру.");
    }

    public void ValidatePhoneNumberAsync(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ValidationException("Номер телефона не должен быть пустым.");

        if (phoneNumber.Length != 10 || !phoneNumber.All(char.IsDigit))
        {
            throw new ValidationException("Номер телефона должен содержать ровно 10 цифр.");
        }
    }

    public void ValidateUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ValidationException("Имя пользователя не должно быть пустым.");

        if (username.Length < 3 || username.Length > 16)
            throw new ValidationException("Имя пользователя должно быть от 3 до 16 символов в длину.");

        if (!username.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-'))
            throw new ValidationException("Имя пользователя может содержать только буквы, цифры, символы '_', и '-'.");
    }

    public void ValidateNamesAsync(string name, string surname, string patronymic)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(surname) ||
            string.IsNullOrWhiteSpace(patronymic))
            throw new ValidationException("Имя, фамилия и отчество не должны быть пустыми.");

        if (name.Any(char.IsDigit) || surname.Any(char.IsDigit) || patronymic.Any(char.IsDigit))
            throw new ValidationException("Имя, фамилия и отчество не должны содержать цифры.");

        if (name.Any(c => !char.IsLetter(c) && c != ' ') || 
            surname.Any(c => !char.IsLetter(c) && c != ' ') ||
            patronymic.Any(c => !char.IsLetter(c) && c != ' '))
            throw new ValidationException("Имя, фамилия и отчество могут содержать только буквы и пробелы.");
    }
}