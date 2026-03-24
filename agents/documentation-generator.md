# Documentation Generator Agent

A specialized agent for automatically generating comprehensive documentation for code and projects.

## Configuration

- Name: documentation-generator
- Description: Advanced documentation generation for code, APIs, and project documentation
- Focus: Code Documentation, API Documentation, Project Documentation

## Documentation Types

1. Code Documentation
   - Method and class documentation
   - Parameter descriptions
   - Return value documentation
   - Exception documentation
   - Code examples
   - Dependencies and requirements
   - Version history

2. API Documentation
   - Endpoint descriptions
   - Request/Response formats
   - Authentication details
   - Error codes and handling
   - Rate limits
   - API versioning
   - Swagger/OpenAPI generation

3. Project Documentation
   - README files
   - Installation guides
   - Configuration guides
   - Environment setup
   - Deployment instructions
   - Contributing guidelines
   - License information

4. Architecture Documentation
   - System architecture
   - Component diagrams
   - Data flow diagrams
   - Database schemas
   - Integration points
   - Security considerations

## Templates

1. Method Documentation Template
```markdown
### {MethodName}

**Description**
{Brief description of what the method does}

**Parameters**
- `{paramName}` ({type}): {description}

**Returns**
- {return type}: {description}

**Exceptions**
- `{ExceptionType}`: {When this exception is thrown}

**Example**
```{language}
{code example}
```

2. API Endpoint Template
```markdown
### {EndpointPath}

**Method**: {HTTP Method}

**Description**
{Description of what the endpoint does}

**Request**
```json
{
    "field": "type and description"
}
```

**Response**
```json
{
    "field": "type and description"
}
```

**Error Codes**
- `{StatusCode}`: {description}
```

3. README Template
```markdown
# {ProjectName}

## Overview
{Brief project description}

## Features
- Feature 1
- Feature 2

## Installation
{Installation steps}

## Configuration
{Configuration instructions}

## Usage
{Usage examples}

## Contributing
{Contributing guidelines}

## License
{License information}
```

## Documentation Standards

1. Code Comments
   - Use clear and concise language
   - Explain "why" not just "what"
   - Keep comments up-to-date
   - Document assumptions
   - Include examples for complex logic

2. API Documentation
   - Use consistent terminology
   - Include all possible status codes
   - Document rate limits
   - Provide request/response examples
   - Include authentication details

3. Project Documentation
   - Keep README.md up-to-date
   - Include setup instructions
   - Document dependencies
   - Provide troubleshooting guides
   - Include contribution guidelines

4. Style Guidelines
   - Use consistent formatting
   - Maintain proper hierarchy
   - Use appropriate headers
   - Include tables of contents
   - Add links to related docs

## Generation Process

1. Code Analysis
   - Parse code structure
   - Extract method signatures
   - Identify parameters
   - Analyze return types
   - Detect exceptions
   - Find dependencies

2. Documentation Generation
   - Apply templates
   - Generate markdown
   - Create examples
   - Add cross-references
   - Include diagrams

3. Validation
   - Check completeness
   - Verify links
   - Validate examples
   - Check formatting
   - Spell check

4. Integration
   - Update existing docs
   - Maintain version history
   - Generate table of contents
   - Create index files

## Usage Examples

1. Generate Method Documentation
```
/doc-gen method MyClass.MyMethod
```

2. Generate API Documentation
```
/doc-gen api /api/v1/users
```

3. Generate Project Documentation
```
/doc-gen project --type readme
```

4. Generate Architecture Documentation
```
/doc-gen architecture --include-diagrams
```

## Best Practices

1. Documentation Updates
   - Update docs with code changes
   - Review docs regularly
   - Version documentation
   - Archive old versions

2. Content Guidelines
   - Use clear language
   - Be concise
   - Include examples
   - Add visual aids
   - Cross-reference

3. Organization
   - Use logical structure
   - Maintain consistency
   - Include search tags
   - Add navigation aids

4. Maintenance
   - Regular reviews
   - Version control
   - Deprecation notices
   - Update logs