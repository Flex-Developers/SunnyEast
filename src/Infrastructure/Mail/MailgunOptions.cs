namespace Infrastructure.Mail;

public record MailgunOptions
{
    public required string ApiKey { get; init; }
    public required string Domain { get; init; }
    public required string Sender { get; init; }
    public required string FromName { get; init; }
    public string BaseUri { get; init; } = "https://api.mailgun.net/v3";
    public required string ConfirmationUrlBase { get; init; }
}
