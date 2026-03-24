# Code Parser Configuration

## Parsing Rules

1. Method Analysis
   ```regex
   # C# Method Signature
   (public|private|protected|internal)?\s*(static|virtual|abstract|override)?\s*(\w+)\s+(\w+)\s*\((.*?)\)

   # JavaScript/TypeScript Function
   (export\s+)?(async\s+)?(function\s+)?(\w+)\s*\((.*?)\)(\s*:\s*\w+)?

   # Python Function
   def\s+(\w+)\s*\((.*?)\)(\s*->\s*\w+)?
   ```

2. Parameter Analysis
   ```regex
   # C# Parameters
   (\w+)\s+(\w+)(\s*=\s*.*)?

   # TypeScript Parameters
   (\w+)\s*:\s*(\w+)(\s*=\s*.*)?

   # Python Parameters
   (\w+)(\s*:\s*\w+)?(\s*=\s*.*)?
   ```

3. Return Type Analysis
   ```regex
   # C# Return Type
   ^.*?\s+(\w+)\s+\w+\s*\(

   # TypeScript Return Type
   \)(\s*:\s*(\w+))?

   # Python Return Type
   \)\s*->\s*(\w+)
   ```

## Documentation Extraction

1. XML Comments (C#)
   ```regex
   /// <summary>(.*?)</summary>
   /// <param name="(\w+)">(.*?)</param>
   /// <returns>(.*?)</returns>
   /// <exception cref="(\w+)">(.*?)</exception>
   ```

2. JSDoc Comments (JavaScript/TypeScript)
   ```regex
   /\*\*\s*(.*?)\s*\*/
   @param\s+{\s*(\w+)\s*}\s+(\w+)\s+-\s+(.*?)
   @returns?\s+{\s*(\w+)\s*}\s+(.*?)
   @throws?\s+{\s*(\w+)\s*}\s+(.*?)
   ```

3. Python Docstrings
   ```regex
   """(.*?)"""
   :param\s+(\w+):\s+(.*?)
   :return:\s+(.*?)
   :raises?\s+(\w+):\s+(.*?)
   ```

## Code Analysis Rules

1. Method Analysis
   - Identify method visibility
   - Extract method name
   - Parse parameters
   - Determine return type
   - Find exceptions thrown
   - Detect async/await usage
   - Identify overrides/virtuals

2. Class Analysis
   - Extract class name
   - Identify base classes
   - Find implemented interfaces
   - List public methods
   - Document properties
   - Detect attributes/decorators
   - Note generic parameters

3. Property Analysis
   - Get property type
   - Identify accessors
   - Note default values
   - Check attributes
   - Document validation

4. Exception Analysis
   - List thrown exceptions
   - Document catch blocks
   - Note exception handling
   - Identify custom exceptions
   - Map error scenarios

## Example Parsing

1. C# Method
```csharp
/// <summary>
/// Creates a new payment order
/// </summary>
/// <param name="request">Payment request details</param>
/// <returns>Order ID string</returns>
/// <exception cref="PaymentException">If payment creation fails</exception>
public async Task<string> CreateOrderAsync(PaymentRequest request)
{
    // Method implementation
}

// Parsed Result
{
    "name": "CreateOrderAsync",
    "async": true,
    "visibility": "public",
    "returnType": "Task<string>",
    "parameters": [
        {
            "name": "request",
            "type": "PaymentRequest",
            "description": "Payment request details"
        }
    ],
    "summary": "Creates a new payment order",
    "exceptions": [
        {
            "type": "PaymentException",
            "description": "If payment creation fails"
        }
    ]
}
```

## Extension Mapping

1. Language Detection
   - .cs -> C#
   - .ts, .js -> TypeScript/JavaScript
   - .py -> Python
   - .java -> Java
   - .go -> Go
   - .rb -> Ruby

2. Documentation Style
   - C# -> XML Comments
   - TypeScript -> JSDoc
   - Python -> Docstrings
   - Java -> Javadoc
   - Go -> Go Comments
   - Ruby -> YARD

## Parsing Process

1. File Analysis
   ```python
   def analyze_file(file_path):
       extension = get_file_extension(file_path)
       parser = get_parser_for_extension(extension)
       content = read_file(file_path)
       return parser.parse(content)
   ```

2. Code Block Extraction
   ```python
   def extract_code_blocks(content):
       methods = extract_methods(content)
       classes = extract_classes(content)
       return {
           "methods": methods,
           "classes": classes
       }
   ```

3. Documentation Generation
   ```python
   def generate_documentation(parsed_content):
       template = get_template_for_type(parsed_content.type)
       doc = template.render(parsed_content)
       return format_markdown(doc)
   ```

## Error Handling

1. Parse Errors
   - Invalid syntax
   - Incomplete code blocks
   - Missing documentation
   - Unknown types

2. Recovery Strategies
   - Skip problematic sections
   - Generate partial documentation
   - Log parsing errors
   - Suggest fixes