using System.Net;
using System.Net.Mail;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendOtpAsync(string toEmail, string otp)
    {
        var smtp = new SmtpClient()
        {
            Host = "smtp.gmail.com",
            Port = 587,
            EnableSsl = true,
            Credentials = new NetworkCredential(
                _config["Email:Address"],
                _config["Email:AppPassword"]
            )
        };

        var message = new MailMessage(
            from: _config["Email:Address"],
            to: toEmail,
            subject: "Your OTP Code",
            body: $"Your OTP code is: {otp}"
        );

        await smtp.SendMailAsync(message);
    }
}
