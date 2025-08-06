using System.Net;
using System.Net.Mail;
using System.Net.Security;
using Microsoft.Extensions.Options;
using System.Security.Cryptography.X509Certificates;
namespace WebAPI1.Services;

/// <summary>
/// 提供電子郵件發送服務的介面。
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// 發送一般電子郵件。
    /// </summary>
    /// <param name="to">收件人電子郵件地址。</param>
    /// <param name="subject">郵件主旨。</param>
    /// <param name="body">郵件內容。</param>
    Task SendEmailAsync(EmailOption option);

    /// <summary>
    /// 發送驗證郵件。
    /// </summary>
    /// <param name="to">收件人電子郵件地址。</param>
    /// <param name="verificationLink">驗證連結。</param>
    Task SendVerificationEmailUrlAsync(string to, string verificationLink);

    Task<bool>SendVerificationEmailCodeAsync(string to, string code);

    /// <summary>
    /// 發送密碼重置郵件。
    /// </summary>
    /// <param name="to">收件人電子郵件地址。</param>
    /// <param name="resetLink">密碼重置連結。</param>
    Task SendPasswordResetEmailAsync(string to, string resetLink);

    /// <summary>
    /// 發送網域驗證郵件。
    /// </summary>
    /// <param name="to">收件人電子郵件地址。</param>
    /// <param name="organizationName">組織名稱。</param>
    /// <param name="verificationToken">驗證令牌。</param>
    Task SendDomainVerificationEmailAsync(string to, string organizationName, string verificationToken);
}

/// <summary>
/// 電子郵件設定類別，包含 SMTP 相關配置。
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// SMTP 伺服器地址。
    /// </summary>
    public string SmtpServer { get; set; } 

    /// <summary>
    /// SMTP 伺服器端口號。
    /// </summary>
    public int SmtpPort { get; set; }

    /// <summary>
    /// SMTP 使用者名稱。
    /// </summary>
    public string SmtpUsername { get; set; }

    /// <summary>
    /// SMTP 密碼。
    /// </summary>
    public string SmtpPassword { get; set; }

    /// <summary>
    /// 發件人電子郵件地址。
    /// </summary>
    public string FromEmail { get; set; }

    /// <summary>
    /// 發件人名稱。
    /// </summary>
    public string FromName { get; set; }

    /// <summary>
    /// 是否啟用 SSL 加密。
    /// </summary>
    public bool EnableSsl { get; set; }
}

/// <summary>
/// 提供電子郵件發送功能的服務。
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// 初始化 EmailService 類別。
    /// </summary>
    /// <param name="settings">電子郵件設定。</param>
    /// <param name="logger">日誌記錄器。</param>
    public EmailService(
        IOptions<EmailSettings> settings,
        ILogger<EmailService> logger,
        IConfiguration configuration)
    {
        _settings = settings.Value;
        _logger = logger;
        _configuration = configuration;
        _settings.SmtpServer = _configuration.GetValue<string>("Email_Setting:SmtpServer");
        _settings.SmtpPort = _configuration.GetValue<int>("Email_Setting:SmtpPort");
        _settings.SmtpUsername = _configuration.GetValue<string>("Email_Setting:SmtpUsername");
        _settings.SmtpPassword = _configuration.GetValue<string>("Email_Setting:SmtpPassword");
        _settings.FromEmail = _configuration.GetValue<string>("Email_Setting:FromEmail");
        _settings.FromName = _configuration.GetValue<string>("Email_Setting:FromName");
        _settings.EnableSsl = _configuration.GetValue<bool>("Email_Setting:EnableSSL");
        
        ConfigureServerCertificateValidation();
    }
    
    /// <summary>
    /// 配置伺服器憑證驗證回調。
    /// </summary>
    private void ConfigureServerCertificateValidation()
    {
        ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            _logger.LogError($"[SMTP 憑證錯誤]：{sslPolicyErrors}");
            Console.WriteLine($"[SMTP 憑證錯誤]：{sslPolicyErrors}");
            return false;
        };
    }


    /// <summary>
    /// 發送電子郵件，支援收件人、副本 (CC) 和密件副本 (BCC)。
    /// </summary>
    /// <param name="option">包含電子郵件發送資訊的選項。</param>
    /// <exception cref="EmailServiceException">當郵件發送失敗時拋出異常。</exception>
    public async Task SendEmailAsync(EmailOption option)
    {
        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = option.Subject,
                Body = option.Body,
                IsBodyHtml = true
            };

            // 添加收件人
            message.To.Add(option.To);

            // 添加 CC
            if (option.Cc != null)
            {
                foreach (var cc in option.Cc)
                {
                    message.CC.Add(cc);
                }
            }

            // 添加 BCC
            if (option.Bcc != null)
            {
                foreach (var bcc in option.Bcc)
                {
                    message.Bcc.Add(bcc);
                }
            }

            using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword),
                EnableSsl = _settings.EnableSsl
            };

            await client.SendMailAsync(message);
            _logger.LogInformation($"Email sent successfully to {option.To}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send email to {option.To}");
            throw new EmailServiceException("Failed to send email", ex);
        }
    }

    /// <summary>
    /// 發送驗證電子郵件。
    /// </summary>
    /// <param name="to">收件人電子郵件地址。</param>
    /// <param name="verificationLink">驗證連結。</param>
    public async Task SendVerificationEmailUrlAsync(string to, string verificationLink)
    {
        var subject = "驗證您的電子郵件地址";
        var body = GenerateEmailTemplate(
            "電子郵件驗證",
            @$"您好，

    請點擊下方按鈕來驗證您的電子郵件地址。此驗證連結將在24小時後失效。

    <a href='{verificationLink}' style='background-color: #4CAF50; color: white; padding: 14px 20px; text-decoration: none; border-radius: 4px;'>驗證電子郵件</a>

    如果您沒有註冊帳號，請忽略此郵件。

    謝謝"
        );
        var option = new EmailOption
        {
            To = to,
            Subject = subject,
            Body = body,
        };

        await SendEmailAsync(option);
    }
    
    public async Task<bool>SendVerificationEmailCodeAsync(string to, string code)
    {
        var subject = "電子郵件地址驗證碼";
        var body = GenerateEmailTemplate(
            "電子郵件登入驗證",
            @$"
            <p style='margin-bottom: 16px;'>親愛的用戶，您好！</p>

            <p style='margin-bottom: 16px;'>您正在進行電子郵件登入驗證，請使用以下驗證碼完成登入流程：</p>

            <div style='background-color: #f8f9fa; border-left: 4px solid #4285f4; padding: 16px; margin: 20px 0; font-family: monospace; font-size: 24px; text-align: center; letter-spacing: 5px;'>{code}</div>

            <p style='margin-bottom: 16px;'>此驗證碼將在 5 分鐘內有效。<br>如果您並未要求進行此操作，請忽略此郵件，並考慮檢查您的帳號安全。</p>

            "
        );
    
        var option = new EmailOption
        {
            To = to,
            Subject = subject,
            Body = body,
        };

        await SendEmailAsync(option);
        return true;
    }

    /// <summary>
    /// 發送密碼重設郵件。
    /// </summary>
    /// <param name="to">收件人電子郵件地址。</param>
    /// <param name="resetLink">密碼重設連結。</param>
    public async Task SendPasswordResetEmailAsync(string to, string resetLink)
    {
        var subject = "密碼重設請求";
        var body = GenerateEmailTemplate(
            "重設密碼",
            @$"您好，

    我們收到了您的密碼重設請求。請點擊下方按鈕來重設您的密碼。此連結將在1小時後失效。

    <a href='{resetLink}' style='background-color: #4CAF50; color: white; padding: 14px 20px; text-decoration: none; border-radius: 4px;'>重設密碼</a>
    <br>
    如果這不是您發起的請求，請忽略此郵件並確保您的帳號安全。

    謝謝"
        );
        var option = new EmailOption
        {
            To = to,
            Subject = subject,
            Body = body,
        };

        await SendEmailAsync(option);
    }

    /// <summary>
    /// 發送組織域名驗證郵件。
    /// </summary>
    /// <param name="to">收件人電子郵件地址。</param>
    /// <param name="organizationName">組織名稱。</param>
    /// <param name="verificationToken">驗證令牌。</param>
    public async Task SendDomainVerificationEmailAsync(string to, string organizationName, string verificationToken)
    {
        var subject = "組織域名驗證";
        var body = GenerateEmailTemplate(
            "域名驗證",
            @$"您好，
                您的組織 {organizationName} 已請求域名驗證。請使用以下驗證令牌來完成域名驗證流程：

                <div style='background-color: #f5f5f5; padding: 10px; margin: 10px 0; border-radius: 4px;'>
                    <code>{verificationToken}</code>
                </div>

                您可以通過以下兩種方式之一來驗證您的域名：

                1. 添加 DNS TXT 記錄：
                   - 主機名：@ 或域名
                   - 值：{verificationToken}

                2. 上傳 HTML 檔案：
                   - 檔案名：{verificationToken}.html
                   - 放置於網站根目錄

                完成上述步驟後，請返回系統點擊「驗證域名」按鈕。

                如果您沒有請求域名驗證，請忽略此郵件。

                謝謝"
        );

        var option = new EmailOption
        {
            To = to,
            Subject = subject,
            Body = body,
        };

        await SendEmailAsync(option);
    }
    
    /// <summary>
    /// 產生電子郵件 HTML 模板。
    /// </summary>
    /// <param name="title">郵件標題。</param>
    /// <param name="content">郵件內容。</param>
    /// <returns>返回完整的 HTML 格式郵件內容。</returns>
    private string GenerateEmailTemplate(string title, string content)
    {
        string svgContent = System.IO.File.ReadAllText("wwwroot/images/logo.svg");
        
        string styledSvgContent = svgContent.Replace("<svg ", "<svg style='width: 70%; height: auto;' ");

        return @$"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='UTF-8'>
        <title>{title}</title>
    </head>
    <body style='font-family: Arial, sans-serif; line-height: 1.6; max-width: 600px; margin: 0 auto; padding: 20px;'>
        <div style='background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
            <h2 style='color: #333; margin-bottom: 20px;'>{title}</h2>
            <div style='color: #666;'>
                {content}
            </div>
            <hr style='margin: 20px 0; border: none; border-top: 1px solid #eee;'>
            <p style='color: #999; font-size: 12px;'>
                此郵件由系統自動發送，請勿直接回覆。如有問題，請聯繫系統管理員。
            </p>
        </div>
        <div style='text-align: center; margin-top: 20px;'>
            {styledSvgContent}
        </div>
    </body>
    </html>";
    }
    
    
}

public class EmailServiceException : Exception
{
    public EmailServiceException(string message) : base(message)
    {
    }

    public EmailServiceException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// 表示電子郵件的選項，包括收件人、副本、密件副本等資訊。
/// </summary>
public class EmailOption
{
    /// <summary>
    /// 收件人電子郵件地址。
    /// </summary>
    public string To { get; set; }

    /// <summary>
    /// 郵件主旨。
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// 郵件內容。
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// 副本 (CC) 電子郵件地址列表。
    /// </summary>
    public List<string>? Cc { get; set; } = new List<string>();

    /// <summary>
    /// 密件副本 (BCC) 電子郵件地址列表。
    /// </summary>
    public List<string>? Bcc { get; set; } = new List<string>();
}