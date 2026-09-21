namespace Clarificador.Api.Exceptions;

public class CaptchaDetectedException : Exception
{
    public CaptchaDetectedException() : base("TikTok mostró un CAPTCHA durante el scraping.") { }
}

public class ScrapingFailedException : Exception
{
    public ScrapingFailedException(string mensaje) : base(mensaje) { }
}