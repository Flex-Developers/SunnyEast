using System.Text.RegularExpressions;
using Application.Contract.Account.Commands;
using Application.Contract.Account.Responses;
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

    private MyAccountResponse? _me;

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

    // Состояния
    private bool _savingProfile, _savingEmail, _savingPhone, _savingPwd, _logoutAllLoading;

    protected override async Task OnInitializedAsync()
    {
        var me = await AccountService.GetAsync();
        if (me is null)
        {
            Nav.NavigateTo($"/login?returnUrl={Uri.EscapeDataString("/account")}");
            return;
        }

        _me = me;

        // Уже заполненные значения в TextField-ах
        _profile.Name = me.Name;
        _profile.Surname = me.Surname;
        _email.NewEmail = me.Email ?? string.Empty; // <-- ВАЖНО: сразу показываем текущий e‑mail
        _phone = (me.Phone ?? "").Replace("+7-", ""); // показываем без +7-

        _loading = false;
    }

    private async Task CopyToClipboard(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        await JS.InvokeVoidAsync("navigator.clipboard.writeText", value);
        Snackbar.Add("Скопировано.", Severity.Success);
    }

    // --- Делегаты валидации (единый тип для MudForm) ---
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
            return Array.Empty<string>();
        }
        catch (System.ComponentModel.DataAnnotations.ValidationException ex)
        {
            return new[] { ex.Message };
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

    // --- Обработчики ---

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
                _me = await AccountService.GetAsync();
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

            // Доп. проверка: новый пароль и подтверждение должны совпадать
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
                // очистим поля
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


    private void CancelProfileEdit()
    {
        if (_me is null) return;
        _profile.Name = _me.Name;
        _profile.Surname = _me.Surname;
        _editProfile = false;
    }

    private async Task SaveEmail()
    {
        _savingEmail = true;
        try
        {
            await _emailForm.Validate();
            if (!_emailForm.IsValid)
            {
                Snackbar.Add("Введите корректный e‑mail.", Severity.Warning);
                return;
            }

            var ok = await AccountService.ChangeEmailAsync(_email);
            if (ok)
            {
                await AccountService.RefreshTokenAsync();
                _me = await AccountService.GetAsync();
                _editEmail = false;
                Snackbar.Add("E‑mail изменён.", Severity.Success);
            }
            else Snackbar.Add("E‑mail уже занят или ошибка сервера.", Severity.Error);
        }
        finally
        {
            _savingEmail = false;
        }
    }

    private void CancelEmailEdit()
    {
        _email.NewEmail = _me?.Email ?? "";
        _editEmail = false;
    }

    private async Task SavePhone()
    {
        _savingPhone = true;
        try
        {
            await _phoneForm.Validate();
            if (!_phoneForm.IsValid)
            {
                Snackbar.Add("Телефон должен быть в формате +7-XXX-XXX-XX-XX.", Severity.Warning);
                return;
            }

            var ok = await AccountService.ChangePhoneAsync(new ChangePhoneCommand { NewPhone = $"+7-{_phone}" });
            if (ok)
            {
                await AccountService.RefreshTokenAsync();
                _me = await AccountService.GetAsync();
                _editPhone = false;
                Snackbar.Add("Телефон изменён.", Severity.Success);
            }
            else Snackbar.Add("Телефон уже занят или ошибка сервера.", Severity.Error);
        }
        finally
        {
            _savingPhone = false;
        }
    }

    private void CancelPhoneEdit()
    {
        _phone = (_me?.Phone ?? "").Replace("+7-", "");
        _editPhone = false;
    }

    private async Task ConfirmLogoutAll()
    {
        var ok = await DialogService.ShowMessageBox("Подтверждение", "Вы действительно хотите завершить все остальные сессии?",
            yesText: "Да", cancelText: "Отмена");
        if (ok == true)
        {
            _logoutAllLoading = true;
            try
            {
                if (await AccountService.LogoutAllAsync())
                    Snackbar.Add("Другие сессии завершены.", Severity.Success);
                else
                    Snackbar.Add("Ошибка при завершении сессий.", Severity.Error);
            }
            finally
            {
                _logoutAllLoading = false;
            }
        }
    }
}