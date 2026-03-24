# Registry Service API Documentation

## Overview
The Registry Service manages MCP registration, agent routing, and context management. It acts as the central hub for service discovery and intelligent routing decisions.

## Authentication
All API endpoints require authentication using:
- Service-to-service JWT tokens (for MCP-Registry communication)
- API keys (for agent-Registry communication)

Example:
```http
X-API-Key: your_api_key_here
Authorization: Bearer your_jwt_token_here
```

## API Endpoints

### MCP Registration

#### Register MCP
Register a new MCP with the registry.

```http
POST /api/registry/mcp
```

Request body:
```json
{
  "name": "MCP-CodeGen-01",
  "url": "https://mcp-codegen-01.example.com",
  "capabilities": {
    "codeGeneration": "true",
    "codeReview": "true",
    "maxTokens": "8000"
  },
  "supportedAgentTypes": ["code-assistant", "security-reviewer"],
  "metadata": {
    "region": "us-west-2",
    "version": "2.0.0"
  }
}
```

#### Update MCP Status
Update an MCP's status and health.

```http
PUT /api/registry/mcp/{mcpId}/status
```

Request body:
```json
{
  "status": "Online",
  "healthStatus": "Healthy",
  "currentLoad": 65
}
```

#### Send Heartbeat
Send heartbeat to maintain active status.

```http
POST /api/registry/heartbeat
```

Query parameters:
- id: MCP or Agent ID
- isMCP: true/false

### Agent Registration

#### Register Agent
Register a new agent with the registry.

```http
POST /api/registry/agent
```

Request body:
```json
{
  "name": "CodeReviewAgent-01",
  "type": "code-reviewer",
  "capabilities": {
    "languages": ["python", "javascript"],
    "reviewTypes": ["security", "performance"],
    "version": "1.0.0"
  },
  "preferredMCPId": "mcp-123"
}
```

#### Update Agent Status
Update an agent's status.

```http
PUT /api/registry/agent/{agentId}/status
```

Request body:
```json
{
  "status": "Available",
  "lastActivity": "2024-03-23T10:00:00Z"
}
```

### Context Management

#### Get MCP Context
Get the context for a specific MCP.

```http
GET /api/registry/mcp/{mcpId}/context
```

#### Update MCP Context
Update an MCP's context.

```http
PUT /api/registry/mcp/{mcpId}/context
```

Request body:
```json
{
  "variables": {
    "expertise": {
      "value": "security_analysis",
      "weight": 2.0,
      "isCritical": true
    }
  },
  "keywords": ["security", "code-review"],
  "specializations": ["python-security", "web-security"],
  "modelCapabilities": {
    "maxTokens": 8000,
    "supportedModels": ["claude-2", "claude-3"],
    "supportedLanguages": ["python", "javascript"]
  }
}
```

### Routing

#### Get Routing Decision
Get routing decision for an agent request.

```http
GET /api/registry/routing
```

Query parameters:
- agentId: ID of the requesting agent
- requestType: Type of request

Response:
```json
{
  "mcpId": "mcp-123",
  "routingMetadata": {
    "mcpLoad": "65",
    "mcpHealth": "Healthy",
    "isPreferred": "true"
  },
  "reason": "Best match based on context and load"
}
```

#### Get Available MCPs
Get list of available MCPs matching criteria.

```http
GET /api/registry/mcps
```

Query parameters:
- capability: Required capability
- maxLoad: Maximum acceptable load
- healthStatus: Required health status

### Metrics and Monitoring

#### Get Registry Status
Get overall registry status.

```http
GET /api/registry/status
```

#### Get System Metrics
Get system-wide metrics.

```http
GET /api/registry/metrics
```

## WebSocket Endpoints

### Registry Hub
Connect to receive real-time updates:

```
ws://your-server/hubs/registry
```

Events:
- `MCPStatusChanged`: When MCP status changes
- `AgentStatusChanged`: When agent status changes
- `ContextUpdated`: When MCP context is updated
- `RoutingDecisionMade`: When routing decision is made

## Error Codes

| Code | Description |
|------|-------------|
| 401  | Authentication failed |
| 403  | Authorization failed |
| 404  | MCP/Agent not found |
| 409  | Registration conflict |
| 429  | Too many requests |
| 500  | Internal server error |
| 503  | Service unavailable |

## Best Practices

1. Registration
   - Send regular heartbeats
   - Keep capabilities up to date
   - Maintain accurate load information

2. Context Management
   - Update context when capabilities change
   - Set appropriate weights for variables
   - Keep specializations focused

3. Routing
   - Include all relevant requirements
   - Handle routing failures gracefully
   - Implement circuit breakers

4. Performance
   - Use WebSocket for real-time updates
   - Cache routing decisions when appropriate
   - Implement request coalescing

## Support
For technical support or questions:
- Email: support@registry-service.com
- Status: status.registry-service.com
- Documentation: docs.registry-service.com