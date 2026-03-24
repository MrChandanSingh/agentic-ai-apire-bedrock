namespace AspireApp.BedRock.SonetOps.MCP.Exceptions;

public class MCPException : Exception
{
    public string ErrorCode { get; }
    public string Operation { get; }
    public Dictionary<string, string> Context { get; }

    public MCPException(string message, string errorCode, string operation, Dictionary<string, string>? context = null) 
        : base(message)
    {
        ErrorCode = errorCode;
        Operation = operation;
        Context = context ?? new Dictionary<string, string>();
    }

    public MCPException(string message, string errorCode, string operation, Exception innerException, Dictionary<string, string>? context = null) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Operation = operation;
        Context = context ?? new Dictionary<string, string>();
    }
}

public class ProcessingException : MCPException
{
    public string RequestType { get; }
    public string ProcessorType { get; }

    public ProcessingException(
        string message,
        string requestType,
        string processorType,
        Dictionary<string, string>? context = null)
        : base(message, "PROCESSING_ERROR", "request_processing", context)
    {
        RequestType = requestType;
        ProcessorType = processorType;
    }
}

public class ResourceExhaustedException : MCPException
{
    public string ResourceType { get; }
    public string Limit { get; }

    public ResourceExhaustedException(
        string resourceType,
        string limit,
        Dictionary<string, string>? context = null)
        : base($"Resource exhausted: {resourceType} exceeded limit of {limit}", 
            "RESOURCE_EXHAUSTED", 
            "resource_management",
            context)
    {
        ResourceType = resourceType;
        Limit = limit;
    }
}

public class InvalidRequestException : MCPException
{
    public List<string> ValidationErrors { get; }

    public InvalidRequestException(
        string message,
        List<string> validationErrors,
        Dictionary<string, string>? context = null)
        : base(message, "INVALID_REQUEST", "request_validation", context)
    {
        ValidationErrors = validationErrors;
    }
}

public class ProcessorNotFoundException : MCPException
{
    public string RequestType { get; }

    public ProcessorNotFoundException(
        string requestType,
        Dictionary<string, string>? context = null)
        : base($"No processor found for request type: {requestType}", 
            "PROCESSOR_NOT_FOUND", 
            "processor_lookup",
            context)
    {
        RequestType = requestType;
    }
}

public class RegistryException : MCPException
{
    public string RegistryOperation { get; }

    public RegistryException(
        string message,
        string registryOperation,
        Dictionary<string, string>? context = null)
        : base(message, "REGISTRY_ERROR", "registry_operation", context)
    {
        RegistryOperation = registryOperation;
    }
}

public class HealthCheckException : MCPException
{
    public string Component { get; }
    public HealthStatus CurrentStatus { get; }

    public HealthCheckException(
        string component,
        HealthStatus status,
        string message,
        Dictionary<string, string>? context = null)
        : base(message, "HEALTH_CHECK_ERROR", "health_monitoring", context)
    {
        Component = component;
        CurrentStatus = status;
    }
}