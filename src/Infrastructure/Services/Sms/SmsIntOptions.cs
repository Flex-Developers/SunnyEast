namespace Infrastructure.Services.Sms;

public sealed class SmsIntOptions
{
    public const string SectionName = "SmsInt";
    public string BaseUrl { get; init; } = default!;
    public string Token { get; init; } = default!;
    
    public string Login { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string DefaultSender { get; init; } = "SOLVOSTOK";
    public bool Enabled { get; init; } = true;
    public bool TestMode { get; init; } = false;

    public string LoginField { get; init; } = "login";
    public string PasswordField { get; init; } = "psw";
    public string PhonesField { get; init; } = "phones";
    public string SenderField { get; init; } = "sender";
    public string TextField { get; init; } = "mes";
    
    public EndpointsSection Endpoints { get; init; } = new();
    
    public sealed class EndpointsSection
    {
        public string SendText    { get; init; } = "/sms/send/text";
        public string GetStatus   { get; init; } = "/sms/status";
        public string Cancel      { get; init; } = "/sms/cancel";
        public string Unsubscribe { get; init; } = "/sms/unsubscribe";
    }
}