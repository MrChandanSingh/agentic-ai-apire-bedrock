# MCP Service API Documentation

## Overview
The MCP (Master Control Program) Service provides AI-powered code generation, code review, and other software development tasks. It uses advanced language models and maintains context for optimal processing.

## Authentication
All API endpoints require authentication using either:
1. API Key (X-API-Key header)
2. JWT Token (Authorization: Bearer token)

Example:
```http
X-API-Key: your_api_key_here
Authorization: Bearer your_jwt_token_here
```

## Subscription Tiers

### Free Tier
- 100 requests per day
- Basic code generation
- Basic code review
- Max 1000 tokens per request
- Single model (claude-instant-1)

### Basic Tier ($29.99/month)
- 1,000 requests per day
- Advanced code generation
- Comprehensive code review
- Max 4000 tokens per request
- Multiple models (claude-instant-1, claude-2)
- Custom instructions support

### Premium Tier ($99.99/month)
- 10,000 requests per day
- Expert-level code generation
- Advanced security analysis
- Max 8000 tokens per request
- All models including latest versions
- Web access for documentation lookup
- Priority processing

### Enterprise Tier ($499.99/month)
- 100,000 requests per day
- Custom model fine-tuning
- Dedicated support
- Unlimited tokens
- Custom model deployment
- Advanced analytics
- SLA guarantees

## API Endpoints

### Instructions

#### Create Instruction
Create a new instruction for processing.

```http
POST /api/mcp/instructions
```

Request body:
```json
{
  "type": "code_generation",
  "content": "Generate a Python function that implements quicksort",
  "parameters": {
    "language": "python",
    "style": "functional",
    "includeTests": true
  }
}
```

#### Get Instruction Status
Get the status of a specific instruction.

```http
GET /api/mcp/instructions/{id}
```

#### Process Instruction
Process a pending instruction.

```http
POST /api/mcp/instructions/{id}/process
```

#### Get Pending Instructions
Get all pending instructions.

```http
GET /api/mcp/instructions/pending
```

### Code Review

#### Submit Code Review
Submit code for review.

```http
POST /api/mcp/review
```

Request body:
```json
{
  "code": "def example():\n    print('hello')",
  "language": "python",
  "reviewType": "security",
  "options": {
    "checkStyle": true,
    "checkSecurity": true,
    "checkPerformance": true
  }
}
```

#### Get Review Status
Get the status of a code review.

```http
GET /api/mcp/review/{id}
```

### Context Management

#### Get Context
Get the current context for this MCP.

```http
GET /api/mcp/context
```

#### Update Context
Update the MCP's context.

```http
PUT /api/mcp/context
```

Request body:
```json
{
  "specializations": ["python", "security"],
  "modelCapabilities": {
    "maxTokens": 8000,
    "supportedModels": ["claude-2", "claude-3"]
  },
  "variables": {
    "expertise": {
      "value": "security_analysis",
      "weight": 2.0
    }
  }
}
```

### Health and Metrics

#### Get Health Status
Get the health status of the MCP.

```http
GET /api/mcp/health
```

#### Get Metrics
Get performance metrics.

```http
GET /api/mcp/metrics
```

#### Update Load
Update current load information.

```http
PUT /api/mcp/load
```

Request body:
```json
{
  "currentLoad": 75,
  "requestsPerMinute": 30
}
```

### Authentication

#### Get API Key Status
Get the status and quota of your API key.

```http
GET /api/mcp/auth/status
```

#### Refresh Token
Refresh an expired JWT token.

```http
POST /api/mcp/auth/refresh
```

## WebSocket Endpoints

### Response Hub
Connect to receive real-time updates:

```
ws://your-server/hubs/response
```

Events:
- `InstructionComplete`: When an instruction is completed
- `ReviewComplete`: When a code review is completed
- `ContextUpdate`: When MCP context is updated
- `LoadUpdate`: When load changes significantly

## Error Codes

| Code | Description |
|------|-------------|
| 401  | Invalid or missing API key/token |
| 403  | Feature not available in current subscription tier |
| 429  | Rate limit or quota exceeded |
| 500  | Internal server error |
| 503  | Service overloaded or in maintenance |

## Request Limits

- Request timeout: 30 seconds
- Maximum code size: 1MB
- Maximum concurrent requests: Based on tier
- Rate limits: Based on tier

## Best Practices

1. Context Management
   - Keep context up to date
   - Set appropriate specializations
   - Update load regularly

2. Request Processing
   - Use appropriate review types
   - Include relevant parameters
   - Handle timeouts gracefully

3. Error Handling
   - Implement exponential backoff
   - Monitor rate limits
   - Handle failover scenarios

4. Performance
   - Use WebSocket for real-time updates
   - Cache responses when appropriate
   - Batch requests when possible

## Support
For technical support or questions:
- Email: support@mcpservice.com
- Status: status.mcpservice.com
- Documentation: docs.mcpservice.com