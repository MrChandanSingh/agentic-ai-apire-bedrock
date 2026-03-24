# AspireApp.BedRock.SonetOps

A modern .NET Aspire application integrating AWS Bedrock with a Model Control Panel (MCP) for processing and handling responses through a Sonet model interface.

## Project Structure

The solution consists of several projects:

- **AspireApp.BedRock.SonetOps.AppHost**: Orchestration project that manages all services
- **AspireApp.BedRock.SonetOps.ServiceDefaults**: Common service configurations
- **AspireApp.BedRock.SonetOps.ApiService**: AWS Bedrock integration service
- **AspireApp.BedRock.SonetOps.MCP**: Model Control Panel service
- **AspireApp.BedRock.SonetOps.Web**: Web frontend
- **AspireApp.BedRock.SonetOps.Tests**: Integration tests

## Key Features

### MCP Service

1. **Instruction Processing**
   - Database-backed instruction queue
   - Real-time status updates
   - Error handling and recovery
   - Configurable processing options

2. **Response Formatting**
   - Multiple response formats (Text, Code, Chart, etc.)
   - Interactive UI components
   - Real-time updates via SignalR
   - Customizable themes

3. **Theme System**
   - Comprehensive color palettes
   - Typography system
   - Spacing and layout controls
   - Dark mode support
   - Accessibility features

### Sonet Integration

1. **Model Configuration**
   - Model: apac.anthropic.claude-3-5-sonnet-20241022-v2:0
   - Configurable parameters
   - Request/response handling
   - Error management

2. **Real-time Communication**
   - SignalR integration
   - Live updates
   - Streaming responses
   - Connection management

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Docker Desktop
- Visual Studio 2022 (recommended) or VS Code
- AWS Account with Bedrock access

### Configuration

1. Set up AWS credentials in your configuration:
\`\`\`json
{
  "AWS": {
    "AccessKey": "your-access-key",
    "SecretKey": "your-secret-key",
    "Region": "your-region"
  }
}
\`\`\`

2. Configure the SQL Server connection:
\`\`\`json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sql;Database=SonetMCP;User Id=sa;Password=Password123!;TrustServerCertificate=True"
  }
}
\`\`\`

### Running the Application

1. **Using Visual Studio:**
   - Open the solution
   - Set AspireApp.BedRock.SonetOps.AppHost as startup project
   - Press F5 or click Run

2. **Using Command Line:**
   \`\`\`bash
   dotnet run --project AspireApp.BedRock.SonetOps.AppHost/AspireApp.BedRock.SonetOps.AppHost.csproj
   \`\`\`

### Default URLs

- Aspire Dashboard: http://localhost:18888
- Web Frontend: http://localhost:5000
- API Service: http://localhost:5001
- MCP Service: http://localhost:5002

## API Usage

### MCP Endpoints

1. **Create Instruction**
\`\`\`http
POST /api/mcp/instructions
Content-Type: application/json

{
  "type": "ModelInference",
  "content": "Your prompt here",
  "parameters": "{}"
}
\`\`\`

2. **Process Instruction**
\`\`\`http
POST /api/mcp/instructions/{id}/process
\`\`\`

3. **Get Instruction Status**
\`\`\`http
GET /api/mcp/instructions/{id}
\`\`\`

### Real-time Updates

Connect to SignalR hub for real-time updates:

\`\`\`javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/response")
    .build();

connection.on("ReceiveResponse", (response) => {
    console.log(response);
});

await connection.start();
\`\`\`

## Response Formats

The system supports multiple response formats:

- Text
- Markdown
- HTML
- JSON
- Table
- Code
- Chart
- Image
- DataGrid
- Timeline
- List
- Alert
- Card
- Terminal
- Diff
- Tree

Each format supports customization through:
- Themes
- Interactions
- Animations
- Responsive layouts

## Theme Customization

Themes can be customized using the ThemeService:

\`\`\`csharp
var customTheme = themeService.CustomizeTheme("dark", theme => {
    theme.Colors.Primary.Main = "#ff0000";
    theme.Typography.Styles["body"].Size = "1.2rem";
});
\`\`\`

Available theme presets:
- default
- dark
- high-contrast
- terminal
- modern
- classic

## Security

The application implements several security measures:

1. API Key Authentication
2. Secure configuration storage
3. Comprehensive security headers
4. Input validation
5. Error handling

## Testing

Run the tests using:

\`\`\`bash
dotnet test
\`\`\`

## Development Notes

- Use the TodoWrite tool for task management
- Follow the established code review process
- Implement proper error handling
- Add XML documentation for public APIs
- Follow the defined security guidelines

## Code Review Agent (chandan-code-reviewer)

The project includes a specialized code review agent named "chandan-code-reviewer" that automatically enforces coding standards and best practices.

### Review Categories

1. **Code Quality (Critical)**
   - SOLID Principles adherence
   - Clean Code principles
   - Code complexity metrics
   - Method length and responsibility
   - Naming conventions
   - Comments and documentation

2. **Security (Critical)**
   - Authentication/Authorization checks
   - Input validation
   - Secure data handling
   - API security
   - Credential management
   - OWASP Top 10 vulnerabilities

3. **Performance (High)**
   - Algorithmic efficiency
   - Resource management
   - Memory usage optimization
   - Database query optimization
   - Caching strategies
   - Connection pooling

4. **Error Handling (High)**
   - Exception management
   - Logging practices
   - Error recovery strategies
   - User feedback
   - System resilience

5. **Architecture (High)**
   - Design patterns
   - Component coupling
   - Service boundaries
   - Dependency management
   - Layer separation
   - Interface segregation

6. **Testing (High)**
   - Unit test coverage
   - Integration test quality
   - Test data management
   - Mock/stub usage
   - Test maintainability

### Review Process

The agent follows a structured review process:

1. **Initial Scan**
   - Code structure analysis
   - Dependency review
   - Basic metrics collection

2. **Deep Analysis**
   - Line-by-line review
   - Pattern recognition
   - Anti-pattern detection
   - Best practice validation

3. **Documentation Check**
   - XML documentation completeness
   - README updates
   - API documentation
   - Code comments quality

4. **Security Audit**
   - Vulnerability scanning
   - Security configuration review
   - Authentication verification
   - Authorization validation

### Review Examples

#### 1. Code Quality Review Example

**Original Code:**
```csharp
public void ProcessData(List<string> items)
{
    for(int i = 0; i < items.Count; i++)
    {
        var item = items[i];
        if(item != null && item.Length > 0)
        {
            var processed = item.ToUpper();
            Console.WriteLine(processed);
            // Save to database
            _context.Items.Add(new Item { Value = processed });
            _context.SaveChanges();
        }
    }
}
```

**Review Feedback:**
```json
{
  "review": {
    "file": "DataProcessor.cs",
    "issues": [
      {
        "severity": "High",
        "category": "Quality",
        "line": 1,
        "message": "Method violates Single Responsibility Principle and contains mixed abstraction levels",
        "suggestion": "Split into smaller methods with single responsibilities",
        "codeExample": "public void ProcessData(List<string> items) => items.Where(IsValid).Select(ProcessItem).ForEach(SaveItem);"
      },
      {
        "severity": "Medium",
        "category": "Performance",
        "line": 9,
        "message": "Database operations inside loop can cause performance issues",
        "suggestion": "Batch database operations",
        "codeExample": "await _context.Items.AddRangeAsync(processedItems);"
      }
    ]
  }
}
```

**Improved Code:**
```csharp
public class DataProcessor
{
    private readonly ILogger<DataProcessor> _logger;
    private readonly IRepository<Item> _repository;

    public async Task ProcessDataAsync(IEnumerable<string> items)
    {
        var processedItems = items
            .Where(IsValid)
            .Select(ProcessItem)
            .ToList();

        await SaveItemsAsync(processedItems);
    }

    private bool IsValid(string item) => !string.IsNullOrEmpty(item);

    private string ProcessItem(string item)
    {
        _logger.LogDebug("Processing item: {Item}", item);
        return item.ToUpper();
    }

    private async Task SaveItemsAsync(IEnumerable<string> items)
    {
        await _repository.AddRangeAsync(items.Select(i => new Item { Value = i }));
    }
}
```

#### 2. Security Review Example

**Original Code:**
```csharp
[ApiController]
public class UserController : ControllerBase
{
    private readonly string _connectionString = "Server=myserver;Database=mydb;User=admin;Password=password123";

    [HttpPost("login")]
    public IActionResult Login(string username, string password)
    {
        using var conn = new SqlConnection(_connectionString);
        var cmd = new SqlCommand($"SELECT * FROM Users WHERE Username='{username}' AND Password='{password}'", conn);
        conn.Open();
        var user = cmd.ExecuteReader();
        if(user.HasRows)
        {
            return Ok(new { token = GenerateToken(username) });
        }
        return BadRequest("Invalid credentials");
    }
}
```

**Review Feedback:**
```json
{
  "review": {
    "file": "UserController.cs",
    "issues": [
      {
        "severity": "Critical",
        "category": "Security",
        "line": 4,
        "message": "Hardcoded credentials in source code",
        "suggestion": "Use secure configuration management",
        "codeExample": "private readonly string _connectionString = _configuration.GetConnectionString(\"DefaultConnection\");"
      },
      {
        "severity": "Critical",
        "category": "Security",
        "line": 9,
        "message": "SQL Injection vulnerability in query",
        "suggestion": "Use parameterized queries",
        "codeExample": "cmd.CommandText = \"SELECT * FROM Users WHERE Username=@Username AND Password=@Password\";"
      },
      {
        "severity": "Critical",
        "category": "Security",
        "line": 9,
        "message": "Storing plain text passwords",
        "suggestion": "Use password hashing",
        "codeExample": "WHERE Username=@Username AND PasswordHash=@PasswordHash"
      }
    ]
  }
}
```

**Improved Code:**
```csharp
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(LoginRequest request)
    {
        try
        {
            var result = await _userService.AuthenticateAsync(
                request.Username,
                request.Password);

            if (!result.Success)
            {
                _logger.LogWarning("Failed login attempt for user {Username}", request.Username);
                return BadRequest(new ErrorResponse("Invalid credentials"));
            }

            return Ok(new LoginResponse(result.Token));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new ErrorResponse("An error occurred"));
        }
    }
}
```

#### 3. Performance Review Example

**Original Code:**
```csharp
public async Task<List<OrderSummary>> GetOrderSummariesAsync()
{
    var orders = await _context.Orders
        .Include(o => o.Customer)
        .Include(o => o.Items)
        .ThenInclude(i => i.Product)
        .ToListAsync();

    return orders.Select(o => new OrderSummary
    {
        OrderId = o.Id,
        CustomerName = o.Customer.Name,
        TotalAmount = o.Items.Sum(i => i.Quantity * i.Product.Price),
        ItemCount = o.Items.Count
    }).ToList();
}
```

**Review Feedback:**
```json
{
  "review": {
    "file": "OrderService.cs",
    "issues": [
      {
        "severity": "High",
        "category": "Performance",
        "line": 3,
        "message": "Inefficient query loading unnecessary data",
        "suggestion": "Project directly to DTO in query",
        "codeExample": "Select(o => new OrderSummary { ... }).ToListAsync()"
      },
      {
        "severity": "Medium",
        "category": "Performance",
        "line": 9,
        "message": "Calculations performed in memory instead of database",
        "suggestion": "Move calculations to database query",
        "codeExample": "Sum(i => i.Quantity * i.UnitPrice)"
      }
    ]
  }
}
```

**Improved Code:**
```csharp
public async Task<List<OrderSummary>> GetOrderSummariesAsync()
{
    return await _context.Orders
        .Select(o => new OrderSummary
        {
            OrderId = o.Id,
            CustomerName = o.Customer.Name,
            TotalAmount = o.Items.Sum(i => i.Quantity * i.Product.Price),
            ItemCount = o.Items.Count
        })
        .TagWith("Get Order Summaries Query")
        .AsSplitQuery()
        .AsNoTracking()
        .ToListAsync();
}
```

### Review Output Format

Reviews are provided in a structured format:

\`\`\`json
{
  "review": {
    "file": "path/to/file.cs",
    "issues": [
      {
        "severity": "Critical|High|Medium|Low",
        "category": "Quality|Security|Performance|Error|Architecture|Testing",
        "line": 123,
        "message": "Detailed description of the issue",
        "suggestion": "Proposed solution or improvement",
        "codeExample": "Example of correct implementation"
      }
    ]
  }
}
\`\`\`

### Code Style Standards

1. **Naming Conventions**
   - PascalCase for classes and methods
   - camelCase for variables and parameters
   - Descriptive and meaningful names
   - Avoid abbreviations
   - Use verb-noun pairs for methods

2. **Code Organization**
   - One class per file
   - Logical grouping of methods
   - Related functionality together
   - Clear separation of concerns
   - Proper namespace organization

3. **Documentation Requirements**
   - XML comments for public APIs
   - Method parameter documentation
   - Return value documentation
   - Exception documentation
   - Usage examples where needed

4. **Testing Standards**
   - Test method naming: [UnitOfWork_Scenario_ExpectedBehavior]
   - Arrange-Act-Assert pattern
   - One assertion per test
   - Proper test isolation
   - Meaningful test data

### Automatic Fixes

The agent can automatically fix common issues:

1. **Code Formatting**
   - Indentation
   - Spacing
   - Line breaks
   - Brace placement
   - File organization

2. **Code Style**
   - Naming convention violations
   - Using directives organization
   - Access modifier ordering
   - Member ordering
   - Region organization

3. **Documentation**
   - Missing XML comments
   - Parameter documentation
   - Return value documentation
   - Exception documentation

### Integration

To use the code review agent:

1. **Manual Review**
   \`\`\`csharp
   await reviewer.ReviewCode("path/to/file.cs");
   \`\`\`

2. **Automated PR Review**
   \`\`\`csharp
   await reviewer.ReviewPullRequest(prNumber);
   \`\`\`

3. **Continuous Integration**
   \`\`\`yaml
   - name: Run Code Review
     uses: ./chandan-code-reviewer
     with:
       paths: "**/*.cs"
       severity: "Critical,High"
   \`\`\`

### Advanced Standards

#### 1. Exception Handling Standards
```csharp
// ❌ Bad Practice
public void ProcessOrder(Order order)
{
    try
    {
        // Process order
    }
    catch (Exception ex)
    {
        // Generic catch-all
        Console.WriteLine(ex.Message);
    }
}

// ✅ Good Practice
public async Task<OrderResult> ProcessOrderAsync(Order order)
{
    try
    {
        await ValidateOrderAsync(order);
        var result = await _orderProcessor.ProcessAsync(order);
        await _notificationService.NotifyCustomerAsync(order);
        return result;
    }
    catch (OrderValidationException ex)
    {
        _logger.LogWarning(ex, "Order validation failed: {OrderId}", order.Id);
        throw new BusinessException("Invalid order", ex);
    }
    catch (ProcessingException ex)
    {
        _logger.LogError(ex, "Order processing failed: {OrderId}", order.Id);
        await _orderCompensation.RollbackAsync(order);
        throw;
    }
}
```

#### 2. Async/Await Best Practices
```csharp
// ❌ Bad Practice
public async Task<User> GetUserAsync(int id)
{
    var user = await _userRepository.GetByIdAsync(id);
    if (user == null)
        return null;
    var orders = await _orderRepository.GetByUserIdAsync(id);
    user.Orders = orders.ToList();
    return user;
}

// ✅ Good Practice
public async Task<UserDetails> GetUserDetailsAsync(int id)
{
    var userTask = _userRepository.GetByIdAsync(id);
    var ordersTask = _orderRepository.GetByUserIdAsync(id);
    
    await Task.WhenAll(userTask, ordersTask);
    
    var user = await userTask;
    if (user == null)
        throw new NotFoundException($"User {id} not found");
        
    return new UserDetails
    {
        User = user,
        Orders = await ordersTask
    };
}
```

#### 3. CQRS Pattern Implementation
```csharp
// Command
public record CreateOrderCommand(
    Guid CustomerId,
    IReadOnlyList<OrderItem> Items,
    Address ShippingAddress
);

// Command Handler
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventPublisher _eventPublisher;
    
    public async Task<Result> HandleAsync(CreateOrderCommand command)
    {
        var order = Order.Create(
            command.CustomerId,
            command.Items,
            command.ShippingAddress);
            
        await _orderRepository.SaveAsync(order);
        await _eventPublisher.PublishAsync(new OrderCreatedEvent(order));
        
        return Result.Success();
    }
}

// Query
public record GetOrderSummaryQuery(Guid OrderId);

// Query Handler
public class GetOrderSummaryQueryHandler : IQueryHandler<GetOrderSummaryQuery, OrderSummary>
{
    private readonly IOrderReadModel _orderReadModel;
    
    public async Task<OrderSummary> HandleAsync(GetOrderSummaryQuery query)
    {
        return await _orderReadModel.GetSummaryAsync(query.OrderId);
    }
}
```

### Advanced Configuration Options

The agent supports advanced configuration through multiple layers:

1. **Rule Sets Configuration**
```json
{
  "ruleSets": {
    "security": {
      "enabled": true,
      "severity": "Critical",
      "rules": {
        "secure-config": true,
        "input-validation": true,
        "authentication": true,
        "authorization": true,
        "data-protection": true,
        "secure-communication": true
      }
    },
    "performance": {
      "enabled": true,
      "severity": "High",
      "rules": {
        "async-await": true,
        "memory-management": true,
        "database-optimization": true,
        "caching": true
      }
    }
  }
}
```

2. **Analysis Configuration**
```json
{
  "analysis": {
    "maxMethodComplexity": 10,
    "maxClassLength": 300,
    "maxMethodLength": 30,
    "maxParameterCount": 4,
    "minTestCoverage": 80,
    "cyclomaticComplexityThreshold": 15,
    "maintainabilityThreshold": 75,
    "duplicationThreshold": 5,
    "staticAnalysis": {
      "enabled": true,
      "tools": ["roslynator", "sonarqube", "stylecop"]
    },
    "metrics": {
      "enabled": true,
      "collectors": ["code-coverage", "complexity", "duplications"]
    }
  }
}
```

3. **Documentation Requirements**
```json
{
  "documentation": {
    "publicMembers": {
      "required": true,
      "style": "xml",
      "minimum": {
        "summary": true,
        "parameters": true,
        "returns": true,
        "exceptions": true
      }
    },
    "privateMembers": {
      "required": false,
      "style": "inline"
    },
    "classes": {
      "required": true,
      "sections": ["summary", "example", "remarks"]
    }
  }
}
```

4. **Quality Gates**
```json
{
  "qualityGates": {
    "critical": {
      "maxIssues": 0,
      "categories": ["security", "data-loss"]
    },
    "high": {
      "maxIssues": 3,
      "categories": ["performance", "maintainability"]
    },
    "medium": {
      "maxIssues": 5,
      "categories": ["style", "documentation"]
    },
    "metrics": {
      "codeCoverage": {
        "min": 80
      },
      "duplicatedLines": {
        "max": 3
      },
      "maintainability": {
        "min": 75
      }
    }
  }
}
```

5. **Custom Rules**
```json
{
  "customRules": {
    "namingConventions": {
      "interfaces": "^I[A-Z][a-zA-Z0-9]*$",
      "abstractClasses": "^Abstract[A-Z][a-zA-Z0-9]*$",
      "testMethods": "^Should[A-Z][a-zA-Z0-9]*$"
    },
    "architecture": {
      "layers": {
        "web": ["Controllers", "Models", "Views"],
        "business": ["Services", "Domain"],
        "data": ["Repositories", "Context"]
      },
      "dependencies": {
        "web": ["business"],
        "business": ["data"],
        "data": []
      }
    }
  }
}

\`\`\`json
{
  "reviewRules": {
    "maxMethodLength": 30,
    "maxClassLength": 300,
    "maxCyclomaticComplexity": 10,
    "requiredTestCoverage": 80,
    "documentationRequired": true,
    "securityScanEnabled": true
  }
}
\`\`\`

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request
6. Ensure all code review checks pass

## License

This project is licensed under the MIT License - see the LICENSE file for details.