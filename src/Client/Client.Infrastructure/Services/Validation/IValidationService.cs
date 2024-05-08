namespace Client.Infrastructure.Services.Validation;

public interface IValidationService
{
    public void ValidateEmail(string username);
    public void ValidatePassword(string email);
    public void ValidatePhoneNumber(string phoneNumber);
    public void ValidateUsername(string username);
    public void ValidateNames(string name, string surname, string patronymic);
}