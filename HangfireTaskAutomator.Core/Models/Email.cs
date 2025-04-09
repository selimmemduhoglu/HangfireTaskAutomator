namespace HangfireTaskAutomator.Core.Models;

public class Email
{
    public int Id { get; set; } // Unique identifier for the email record
    public string? Recipient { get; set; }   // Email address of the recipient
    public string? Subject { get; set; } // Subject of the email
    public string? Body { get; set; } // Body of the email
    public bool IsSent { get; set; } // Indicates if the email has been sent
    public DateTime? SentAt { get; set; } // Date and time when the email was sent
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Date and time when the email was created
}
