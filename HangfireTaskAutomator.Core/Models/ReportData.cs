namespace HangfireTaskAutomator.Core.Models;

public class ReportData
{
    public int Id { get; set; } // Unique identifier for the report record
    public string? ReportName { get; set; } // Name of the report
    public string? ReportType { get; set; } // Type of the report (e.g., PDF, Excel)
    public DateTime GeneratedAt { get; set; } // Date and time when the report was generated
    public string? FilePath { get; set; } // Path to the generated report file
    public bool IsProcessed { get; set; } // Indicates if the report has been processed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Date and time when the report record was created
}
