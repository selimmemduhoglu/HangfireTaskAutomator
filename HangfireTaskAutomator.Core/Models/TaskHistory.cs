namespace HangfireTaskAutomator.Core.Models;

public class TaskHistory
{
    public int Id { get; set; } 
    public string? JobId { get; set; } 
    public string? TaskName { get; set; } 
    public string? JobType { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Status { get; set; }
    public string? Result { get; set; }
    public string? Exception { get; set; } 
}
