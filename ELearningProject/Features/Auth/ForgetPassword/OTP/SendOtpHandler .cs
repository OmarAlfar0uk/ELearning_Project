using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using ELearningProject.Contarcts;
using ELearningProject.Features.Auth.ForgetPassword.OTP;
using ELearningProject.Models;
using Auth.Models;
using Auth.Contarcts;

namespace ELearningProject.Features.Auth.ForgetPassword
{
    public class SendOtpHandler : IRequestHandler<SendOtpCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;
        private readonly IMailKitEmailService _emailService;

        public SendOtpHandler(UserManager<ApplicationUser> userManager, IMemoryCache cache, IMailKitEmailService emailService)
        {
            _userManager = userManager;
            _cache = cache;
            _emailService = emailService;
        }

        public async Task<bool> Handle(SendOtpCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            // Generate OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // Save OTP for 5 minutes
            _cache.Set($"otp:{user.Email}", otp, TimeSpan.FromMinutes(5));

            // Send Email
            // Send Email
            var subject = "Password Reset Code";

            string body = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
<meta charset='UTF-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
<style>
    body {{
        margin: 0;
        padding: 0;
        background-color: #f2f2f2;
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    }}
    .wrapper {{
        width: 100%;
        padding: 30px 0;
    }}
    .email-container {{
        max-width: 600px;
        margin: auto;
        background-color: #ffffff;
        border-radius: 12px;
        box-shadow: 0 10px 30px rgba(0,0,0,0.08);
        overflow: hidden;
    }}
    .header {{
        background: linear-gradient(135deg, #ff7a00, #ff9a3c);
        color: #ffffff;
        padding: 18px;
        text-align: center;
        font-size: 22px;
        font-weight: 600;
        letter-spacing: 0.5px;
    }}
    .content {{
        padding: 35px 30px;
        text-align: center;
    }}
    .title {{
        font-size: 20px;
        font-weight: 600;
        margin-bottom: 10px;
        color: #333;
    }}
    .message {{
        font-size: 15px;
        color: #555;
        line-height: 1.7;
    }}
    .otp-box {{
        margin: 30px auto;
        display: inline-block;
        background-color: #fff5ec;
        color: #ff7a00;
        padding: 18px 36px;
        font-size: 34px;
        font-weight: bold;
        border-radius: 10px;
        letter-spacing: 8px;
        border: 2px dashed #ff7a00;
        box-shadow: 0 5px 15px rgba(255,122,0,0.25);
    }}
    .divider {{
        height: 1px;
        background-color: #eee;
        margin: 30px 0;
    }}
    .note {{
        font-size: 14px;
        color: #777;
    }}
    .footer {{
        background-color: #fafafa;
        text-align: center;
        padding: 18px;
        font-size: 12px;
        color: #999;
    }}
</style>
</head>
<body>
<div class='wrapper'>
    <div class='email-container'>
        <div class='header'>
            ELearningProject
        </div>

        <div class='content'>
            <div class='title'>Verify Your Identity</div>

            <p class='message'>
                Hello,<br><br>
                We received a request to verify your identity.
                Please use the OTP code below to continue.
            </p>

            <div class='otp-box'>{otp}</div>

            <div class='divider'></div>

            <p class='note'>
                This code will expire in <strong>5 minutes</strong>.<br>
                If you didn’t request this code, you can safely ignore this email.
            </p>
        </div>

        <div class='footer'>
            © {DateTime.UtcNow.Year} AbdelwhabOutLet. All rights reserved.
        </div>
    </div>
</div>
</body>
</html>
";

            await _emailService.SendEmailAsync(user.Email, subject, body);


            return true;
        }
    }
}
