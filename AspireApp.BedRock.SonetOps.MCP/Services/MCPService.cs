using AspireApp.BedRock.SonetOps.MCP.Data;
using AspireApp.BedRock.SonetOps.MCP.Models;
using Microsoft.EntityFrameworkCore;

namespace AspireApp.BedRock.SonetOps.MCP.Services;

public interface IMCPService
{
    Task<Instruction> ProcessInstructionAsync(int instructionId);
    Task<Instruction> CreateInstructionAsync(string type, string content, string? parameters = null);
    Task<IEnumerable<Instruction>> GetPendingInstructionsAsync();
    Task<Instruction?> GetInstructionByIdAsync(int id);
}

public class MCPService : IMCPService
{
    private readonly MCPContext _context;
    private readonly ILogger<MCPService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ISonetService _sonetService;
    private readonly IUINotificationService _uiNotification;

    public MCPService(
        MCPContext context,
        ILogger<MCPService> logger,
        IConfiguration configuration,
        ISonetService sonetService,
        IUINotificationService uiNotification)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _sonetService = sonetService;
        _uiNotification = uiNotification;
    }

    public async Task<Instruction> CreateInstructionAsync(string type, string content, string? parameters = null)
    {
        var instruction = new Instruction
        {
            Type = type,
            Content = content,
            Parameters = parameters,
            Status = InstructionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Instructions.Add(instruction);
        await _context.SaveChangesAsync();

        return instruction;
    }

    public async Task<IEnumerable<Instruction>> GetPendingInstructionsAsync()
    {
        return await _context.Instructions
            .Where(i => i.Status == InstructionStatus.Pending)
            .OrderBy(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<Instruction?> GetInstructionByIdAsync(int id)
    {
        return await _context.Instructions.FindAsync(id);
    }

    public async Task<Instruction> ProcessInstructionAsync(int instructionId)
    {
        var instruction = await _context.Instructions.FindAsync(instructionId)
            ?? throw new KeyNotFoundException($"Instruction with ID {instructionId} not found.");

        if (instruction.Status != InstructionStatus.Pending)
        {
            throw new InvalidOperationException($"Instruction {instructionId} is not in Pending status.");
        }

        try
        {
            instruction.Status = InstructionStatus.Processing;
            await _context.SaveChangesAsync();

            // Process the instruction based on its type
            instruction.Response = await ProcessByTypeAsync(instruction);
            instruction.Status = InstructionStatus.Completed;
            instruction.ProcessedAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing instruction {InstructionId}", instructionId);
            instruction.Status = InstructionStatus.Failed;
            instruction.Response = $"Error: {ex.Message}";
        }

        await _context.SaveChangesAsync();
        return instruction;
    }

    private async Task<string> ProcessByTypeAsync(Instruction instruction)
    {
        return instruction.Type switch
        {
            "DatabaseQuery" => await ProcessDatabaseQueryAsync(instruction),
            "DataAnalysis" => await ProcessDataAnalysisAsync(instruction),
            "ModelInference" => await ProcessModelInferenceAsync(instruction),
            _ => throw new NotSupportedException($"Instruction type {instruction.Type} is not supported.")
        };
    }

    private async Task<string> ProcessDatabaseQueryAsync(Instruction instruction)
    {
        // Implement database query processing
        await Task.Delay(100); // Placeholder for actual processing
        return $"Processed database query: {instruction.Content}";
    }

    private async Task<string> ProcessDataAnalysisAsync(Instruction instruction)
    {
        // Implement data analysis processing
        await Task.Delay(100); // Placeholder for actual processing
        return $"Analyzed data: {instruction.Content}";
    }

    private async Task<string> ProcessModelInferenceAsync(Instruction instruction)
    {
        await _uiNotification.NotifyProcessingStartedAsync(instruction.Id);

        try
        {
            var sonetResponse = await _sonetService.GetModelResponseAsync(instruction.Content);
            await _uiNotification.NotifySonetResponseAsync(sonetResponse);
            return sonetResponse.Content;
        }
        catch (Exception ex)
        {
            await _uiNotification.NotifyProcessingFailedAsync(instruction.Id, ex.Message);
            throw;
        }
    }
}