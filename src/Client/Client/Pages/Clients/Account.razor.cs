using System.Text.RegularExpressions;
using Application.Contract.Account.Commands;
using Application.Contract.Account.Responses;
using Application.Contract.Identity;
using Application.Contract.Verification.Commands;
using Client.Infrastructure.Auth;
using Client.Infrastructure.Services.Verification;
using Microsoft.JSInterop;
using MudBlazor;

namespace Client.Pages.Clients;

public partial class Account
{
    private bool _loading = true;
    private bool _isMobile = false;
    private int _tab;

    private bool _showCurrentPwd;
    private bool _showNewPwd;
    private bool _showConfirmPwd;

    private string _pwdConfirm = "";
    private string? _pwdConfirmError;

    private MyAccountResponse? _account;

    // Модели
    private UpdateProfileCommand _profile = new() { Name = "", Surname = "" };
    private ChangeEmailCommand _email = new() { NewEmail = "" };
    private ChangePasswordCommand _pwd = new() { CurrentPassword = "", NewPassword = "" };
    private string _phone = ""; // маска: 000-000-00-00

    // Режимы редактирования
    private bool _editProfile, _editEmail, _editPhone;

    // Формы
    private MudForm _profileForm = null!;
    private MudForm _emailForm = null!;
    private MudForm _phoneForm = null!;
    private MudForm _pwdForm = null!;

    private bool _logoutSelfLoading;
    private bool _deletingAccount;

    // Состояния
    private bool _savingProfile, _savingEmail, _savingPhone, _savingPwd;

    // Подсказки/прогресс по отправке кодов
    private bool _promptEmailConfirm, _promptPhoneConfirm;
    private bool _sendingEmailCode, _sendingPhoneCode;
    private bool _isSuperAdmin;
    
    protected override async Task OnInitializedAsync()
    {
        var myAccountResponse = await AccountService.GetAsync();
        if (myAccountResponse is null)
        {
            Nav.NavigateTo($"/login?returnUrl={Uri.EscapeDataString("/account")}");
            return;
        }

        _account = myAccountResponse;
        
        var auth = await AuthState.GetAuthenticationStateAsync();
        _isSuperAdmin = auth.User.IsInRole(ApplicationRoles.SuperAdmin);

        // Уже заполненные значения в TextField-ах
        _profile.Name = myAccountResponse.Name;
        _profile.Surname = myAccountResponse.Surname;
        _email.NewEmail = myAccountResponse.Email ?? string.Empty;
        _phone = (myAccountResponse.Phone ?? "").Replace("+7-", ""); // показываем без +7-

        _loading = false;
    }

    // выйти ТОЛЬКО на этом устройстве (клиентский логаут)
    private async Task LogoutThisDevice()
    {
        _logoutSelfLoading = true;
        try
        {
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };
            await DialogService.ShowAsync<Components.Dialogs.Logout>("Выход", options);
        }
        finally
        {
            _logoutSelfLoading = false;
        }
    }

    // подтвердить и удалить аккаунт
    private async Task ConfirmDeleteAccount()
    {
        var ok = await DialogService.ShowMessageBox(
            "Удалить аккаунт?",
            "Это действие необратимо. Будут удалены ваши данные и завершены все сессии.",
            yesText: "Удалить",
            cancelText: "Отмена");

        if (ok == true)
        {
            _deletingAccount = true;
            try
            {
                var deleted = await AccountService.DeleteAccountAsync();
                if (deleted)
                {
                    await AuthState.MarkUserAsLoggedOut();
                    Snackbar.Add("Аккаунт удалён.", Severity.Success);
                    Nav.NavigateTo("/");
                }
                else
                {
                    Snackbar.Add("Не удалось удалить аккаунт.", Severity.Error);
                }
            }
            finally
            {
                _deletingAccount = false;
            }
        }
    }

    private async Task CopyToClipboard(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        await JS.InvokeVoidAsync("navigator.clipboard.writeText", value);
        Snackbar.Add("Скопировано.", Severity.Success);
    }

    // --- Валидации для MudForm ---
    private Func<object, string, Task<IEnumerable<string>>> ProfileValidate => async (model, prop) =>
    {
        var m = (UpdateProfileCommand)model;
        var errs = new List<string>();

        if (prop == nameof(UpdateProfileCommand.Name))
        {
            var e = ValidationService.ValidateFirstName(m.Name ?? "");
            if (!string.IsNullOrWhiteSpace(e)) errs.Add(e);
        }
        else if (prop == nameof(UpdateProfileCommand.Surname))
        {
            var e = ValidationService.ValidateLastName(m.Surname ?? "", allowEmpty: false);
            if (!string.IsNullOrWhiteSpace(e)) errs.Add(e);
        }

        return errs;
    };

    private Func<object, string, Task<IEnumerable<string>>> EmailValidate => async (model, prop) =>
    {
        var m = (ChangeEmailCommand)model;
        var err = ValidationService.ValidateEmail(m.NewEmail ?? "");
        return string.IsNullOrWhiteSpace(err) ? Array.Empty<string>() : new[] { err };
    };

    private Func<object, string, Task<IEnumerable<string>>> PhoneValidate => async (model, prop) =>
    {
        var digits = Regex.Replace(_phone ?? "", @"\D", "");
        try
        {
            ValidationService.ValidatePhoneNumber(digits);
            return [];
        }
        catch (System.ComponentModel.DataAnnotations.ValidationException ex)
        {
            return [ex.Message];
        }
    };

    private Func<object, string, Task<IEnumerable<string>>> PasswordValidate => async (model, prop) =>
    {
        var m = (ChangePasswordCommand)model;
        var errs = new List<string>();

        if (prop == nameof(ChangePasswordCommand.CurrentPassword))
        {
            if (string.IsNullOrWhiteSpace(m.CurrentPassword))
                errs.Add("Текущий пароль обязателен.");
            return errs;
        }

        try
        {
            ValidationService.ValidatePassword(m.NewPassword ?? "");
        }
        catch (System.ComponentModel.DataAnnotations.ValidationException ex)
        {
            errs.Add(ex.Message);
        }

        return errs;
    };

    // --- Профиль ---
    private async Task SaveProfile()
    {
        _savingProfile = true;
        try
        {
            await _profileForm.Validate();
            if (!_profileForm.IsValid)
            {
                Snackbar.Add("Проверьте корректность имени/фамилии.", Severity.Warning);
                return;
            }

            var ok = await AccountService.UpdateProfileAsync(_profile);
            if (ok)
            {
                await AccountService.RefreshTokenAsync();
                _account = await AccountService.GetAsync();
                _editProfile = false;
                Snackbar.Add("Сохранено.", Severity.Success);
            }
            else Snackbar.Add("Ошибка при сохранении.", Severity.Error);
        }
        finally
        {
            _savingProfile = false;
        }
    }

    private void CancelProfileEdit()
    {
        if (_account is null) return;
        _profile.Name = _account.Name;
        _profile.Surname = _account.Surname;
        _editProfile = false;
    }

    // --- Пароль ---
    private async Task SavePassword()
    {
        _savingPwd = true;
        _pwdConfirmError = null;
        try
        {
            await _pwdForm.Validate();
            if (!_pwdForm.IsValid)
            {
                Snackbar.Add("Проверьте пароль.", Severity.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_pwdConfirm) || _pwd.NewPassword != _pwdConfirm)
            {
                _pwdConfirmError = "Пароли не совпадают.";
                Snackbar.Add("Подтверждение пароля не совпадает.", Severity.Warning);
                return;
            }

            var ok = await AccountService.ChangePasswordAsync(_pwd);
            if (ok)
            {
                Snackbar.Add("Пароль изменён. Другие сессии завершены.", Severity.Success);
                _pwd = new ChangePasswordCommand { CurrentPassword = "", NewPassword = "" };
                _pwdConfirm = "";
                _pwdConfirmError = null;
            }
            else
            {
                Snackbar.Add("Неверный текущий пароль или ошибка.", Severity.Error);
            }
        }
        finally
        {
            _savingPwd = false;
        }
    }

    // --- Email: сохранить = показать подсказку, а не сразу слать код ---
    private async Task SaveEmail()
    {
        var newMail = (_email.NewEmail ?? "").Trim();
        var oldMail = (_account?.Email ?? "").Trim();

        if (string.Equals(newMail, oldMail, StringComparison.OrdinalIgnoreCase))
        {
            Snackbar.Add("Этот e-mail уже привязан к вашему аккаунту.", Severity.Info);
            _editEmail = false;
            return;
        }
        
        _savingEmail = true;
        try
        {
            await _emailForm.Validate();
            if (!_emailForm.IsValid)
            {
                Snackbar.Add("Введите корректный e-mail.", Severity.Warning);
                return;
            }

            _promptEmailConfirm = true;
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
        finally
        {
            _savingEmail = false;
        }
    }

    private async Task StartSendEmailCodeAsync()
    {
        var newMail = (_email.NewEmail ?? "").Trim();
        var oldMail = (_account?.Email ?? "").Trim();
        if (string.Equals(newMail, oldMail, StringComparison.OrdinalIgnoreCase))
        {
            Snackbar.Add("E-mail не изменился.", Severity.Info);
            return;
        }

        
        if (string.IsNullOrWhiteSpace(_email.NewEmail)) 
            return;

        _sendingEmailCode = true;
        try
        {
            var start = await AccountService.StartLinkEmailAsync(_email.NewEmail);

            var ch = start.Selected.ToString(); // "email"
            var to = Uri.EscapeDataString(start.MaskedEmail ?? _email.NewEmail);

            Nav.NavigateTo(
                $"/verify?purpose=link&channel={ch}&to={to}&title=Подтверждение%20e-mail&len={start.CodeLength}&sid={start.SessionId}&returnUrl=%2Faccount");
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
        finally
        {
            _sendingEmailCode = false;
        }
    }

    private void CancelEmailPrompt()
    {
        _promptEmailConfirm = false;
        _email.NewEmail = _account?.Email ?? "";
        _editEmail = false;
    }

    private void CancelEmailEdit()
    {
        _promptEmailConfirm = false;
        _email.NewEmail = _account?.Email ?? "";
        _editEmail = false;
    }

    // --- Телефон: сохранить = показать подсказку ---
    private async Task SavePhone()
    {
        var fullPhone = $"+7-{_phone}";
        if (string.Equals(fullPhone, _account?.Phone ?? "", StringComparison.Ordinal))
        {
            Snackbar.Add("Этот номер уже привязан к вашему аккаунту.", Severity.Info);
            _editPhone = false;
            return;
        }

        _savingPhone = true;
        try
        {
            await _phoneForm.Validate();
            if (!_phoneForm.IsValid)
            {
                Snackbar.Add("Телефон должен быть в формате +7-XXX-XXX-XX-XX.", Severity.Warning);
                return;
            }

            _promptPhoneConfirm = true;
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
        finally
        {
            _savingPhone = false;
        }
    }

    private async Task StartSendPhoneCodeAsync()
    {
        var fullPhone = $"+7-{_phone}";
        if (string.Equals(fullPhone, _account?.Phone ?? "", StringComparison.Ordinal))
        {
            Snackbar.Add("Номер телефона не изменился.", Severity.Info);
            return;
        }

        _sendingPhoneCode = true;
        try
        {
            var start = await AccountService.StartLinkPhoneAsync(fullPhone);

            var ch = start.Selected.ToString(); // "phone"
            var to = Uri.EscapeDataString(start.MaskedPhone ?? _phone);

            Nav.NavigateTo(
                $"/verify?purpose=link&channel={ch}&to={to}&title=Подтверждение%20телефона&len={start.CodeLength}&sid={start.SessionId}&returnUrl=%2Faccount");
        }
        catch (Exception ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
        }
        finally
        {
            _sendingPhoneCode = false;
        }
    }

    private void CancelPhonePrompt()
    {
        _promptPhoneConfirm = false;
        _phone = (_account?.Phone ?? "").Replace("+7-", "");
        _editPhone = false;
    }

    private void CancelPhoneEdit()
    {
        _promptPhoneConfirm = false;
        _phone = (_account?.Phone ?? "").Replace("+7-", "");
        _editPhone = false;
    }
}