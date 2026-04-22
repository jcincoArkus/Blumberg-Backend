namespace Adapters.Config;

/// <summary>
/// Alert email notification configuration. Optional: when provider is None, no emails are sent.
/// </summary>
public class EmailConfig
{
    /// <summary>Provider: None (no-op) or Smtp.</summary>
    public string Provider { get; private set; } = "None";

    /// <summary>SMTP host (required when Provider is Smtp).</summary>
    public string SmtpHost { get; private set; } = string.Empty;

    /// <summary>SMTP port. Default 25 (587 for TLS).</summary>
    public int SmtpPort { get; private set; } = 25;

    /// <summary>From address for alert emails (required when Provider is Smtp).</summary>
    public string SmtpFromAddress { get; private set; } = string.Empty;

    /// <summary>Optional SMTP user name.</summary>
    public string SmtpUser { get; private set; } = string.Empty;

    /// <summary>Optional SMTP password.</summary>
    public string SmtpPassword { get; private set; } = string.Empty;

    /// <summary>Use SSL/TLS for SMTP. Default false.</summary>
    public bool SmtpUseSsl { get; private set; }

    /// <summary>Initializes configuration from environment variables.</summary>
    public EmailConfig Init()
    {
        Provider = EnvHelper.GetEnv("ALERT_EMAIL_PROVIDER", "None");
        SmtpHost = EnvHelper.GetEnv("SMTP_HOST", "");
        SmtpPort = EnvHelper.GetEnvInt("SMTP_PORT", 25);
        SmtpFromAddress = EnvHelper.GetEnv("SMTP_FROM_ADDRESS", "");
        SmtpUser = EnvHelper.GetEnv("SMTP_USER", "");
        SmtpPassword = EnvHelper.GetEnv("SMTP_PASSWORD", "");
        SmtpUseSsl = EnvHelper.GetEnvBool("SMTP_USE_SSL", false);
        return this;
    }

    /// <summary>Validates configuration. When Provider is Smtp, host and from address are required.</summary>
    public EmailConfig Validate()
    {
        if (string.Equals(Provider, "Smtp", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(SmtpHost))
                throw new InvalidOperationException("SMTP_HOST is required when ALERT_EMAIL_PROVIDER=Smtp");
            if (string.IsNullOrWhiteSpace(SmtpFromAddress))
                throw new InvalidOperationException("SMTP_FROM_ADDRESS is required when ALERT_EMAIL_PROVIDER=Smtp");
        }
        return this;
    }

    /// <summary>True when provider is Smtp (case-insensitive).</summary>
    public bool IsSmtp => string.Equals(Provider, "Smtp", StringComparison.OrdinalIgnoreCase);
}
