namespace Application.Common.Interfaces.Services;

public interface IValidationService
{
    public void ValidateEmailAsync(string username);
    public void ValidatePasswordAsync(string email);
    public void ValidatePhoneNumberAsync(string phoneNumber);
    public void ValidateUsernameAsync(string username);
    public void ValidateNamesAsync(string name, string surname, string patronymic);
}