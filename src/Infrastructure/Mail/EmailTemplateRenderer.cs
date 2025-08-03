using System.Text;

namespace Infrastructure.Mail;

public static class EmailTemplateRenderer
{
    public static string RenderEmailConfirmation(string confirmationLink, string? code = null)
    {
        var sb = new StringBuilder();
        sb.Append("<html><body>");
        sb.Append("<p>Здравствуйте!</p>");
        sb.Append("<p>Для завершения регистрации подтвердите адрес электронной почты.</p>");
        sb.Append($"<p><a href='{confirmationLink}'>Подтвердить e-mail</a></p>");
        sb.Append($"<p>Если кнопка не работает, перейдите по ссылке: {confirmationLink}</p>");
        if (code != null)
            sb.Append($"<p>Код подтверждения: <b>{code}</b></p>");
        sb.Append("</body></html>");
        return sb.ToString();
    }
}
