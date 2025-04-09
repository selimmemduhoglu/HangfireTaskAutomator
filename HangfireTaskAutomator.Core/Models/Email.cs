namespace HangfireTaskAutomator.Core.Models;

public class Email
{
    public int Id { get; set; }
    public string Recipient { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
