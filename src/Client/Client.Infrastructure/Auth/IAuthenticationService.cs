namespace Client.Infrastructure.Auth;

public interface IAuthenticationService
{
    void NavigateToExternalLogin(string returnUrl);


    Task LogoutAsync();

    Task ReLoginAsync(string returnUrl);
}