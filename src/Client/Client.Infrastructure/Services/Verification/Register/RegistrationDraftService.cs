using Application.Contract.User.Commands;
using Blazored.SessionStorage;

namespace Client.Infrastructure.Services.Verification.Register;

public sealed class RegistrationDraftService(ISessionStorageService session) : IRegistrationDraftService
{
    private const string Key = "reg_public_v1";
    private string? _pwd, _confirm;

    public async Task SavePublicAsync(RegisterUserCommand publicPart)
    {
        // гарантируем, что пароль в sessionStorage не попадёт
        var dto = new RegisterUserCommand
        {
            Name = publicPart.Name,
            Surname = publicPart.Surname,
            PhoneNumber = publicPart.PhoneNumber,
            Email = publicPart.Email,
            Password = "",
            ConfirmPassword = ""
        };

        await session.SetItemAsync(Key, dto);
        // или так без async: return session.SetItemAsync(Key, dto).AsTask();
    }

    public async Task<RegisterUserCommand?> GetPublicAsync()
    {
        if (await session.ContainKeyAsync(Key))
            return await session.GetItemAsync<RegisterUserCommand>(Key);

        return null;
    }

    public async Task ClearPublicAsync()
        => await session.RemoveItemAsync(Key);

    public void SetPassword(string password, string confirm)
    {
        _pwd = password;
        _confirm = confirm;
    }

    public (string? Password, string? Confirm) GetPassword()
        => (_pwd, _confirm);

    public void ClearPassword()
    {
        _pwd = null;
        _confirm = null;
    }

    public async Task ClearAllAsync()
    {
        await ClearPublicAsync(); // убрать черновик из sessionStorage
        ClearPassword(); // стереть пароль из памяти
    }
}