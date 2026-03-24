namespace AspireApp.BedRock.SonetOps.MCP.Models;

public class Instruction
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Parameters { get; set; }
    public InstructionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? Response { get; set; }
}

public enum InstructionStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}