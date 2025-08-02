namespace Client.Infrastructure.Services.Validation;

public interface IValidationService
{
    public string ValidateEmail(string username);
    public void ValidatePassword(string email);
    public void ValidatePhoneNumber(string phoneNumber);
    public void ValidateNames(string name, string surname);
    string? ValidateFirstName(string name);
    string? ValidateLastName(string surname, bool allowEmpty = false);
}