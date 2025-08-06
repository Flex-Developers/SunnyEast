namespace Application.Common.Utils;

public static class PhoneMasking
{
    /// <summary>
    /// Маска в формате "+7-***-***-XX-YY". Берёт последние 4 цифры из любого входного формата.
    /// </summary>
    public static string MaskPhoneLast4(string? phone)
    {
        var digits = new string((phone ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.Length < 4) return "+7-***-***-**-**";

        var last4 = digits[^4..];      // "4567"
        var last2a = last4[..2];       // "45"
        var last2b = last4[2..];       // "67"

        return $"+7-***-***-{last2a}-{last2b}";
    }

    /// <summary>
    /// Для SMSInt: "+7-901-123-45-67" -> "+7 901 123 45 67".
    /// Работает и с "79011234567".
    /// </summary>
    public static string ToSmsIntRecipient(string? phone)
    {
        var digits = new string((phone ?? string.Empty).Where(char.IsDigit).ToArray());
        // ожидаем российский номер: 11 цифр, первая — 7
        if (digits.Length == 11 && digits[0] == '7')
        {
            var p = digits;
            return $"+7 {p[1..4]} {p[4..7]} {p[7..9]} {p[9..11]}"; // "+7 901 123 45 67"
        }

        // fallback: аккуратно заменяем дефисы пробелами
        return (phone ?? string.Empty).Replace("+7-", "+7 ").Replace("-", " ");
    }
    
    /// Приводит российский номер к E.164: +7XXXXXXXXXX
    public static string NormalizeE164(string raw)
    {
        var digits = new string((raw ?? "").Where(char.IsDigit).ToArray());

        // 8XXXXXXXXXX -> +7XXXXXXXXXX
        if (digits.Length == 11 && digits.StartsWith("8"))
            digits = "7" + digits[1..];

        if (digits.Length == 11 && digits.StartsWith("7"))
            return "+" + digits;

        throw new ArgumentException("Некорректный номер телефона");
    }
}