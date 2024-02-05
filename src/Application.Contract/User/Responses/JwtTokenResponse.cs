namespace Application.Contract.User.Responses;

public class JwtTokenResponse
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public DateTime AccessTokenValidateTo { get; set; }
}