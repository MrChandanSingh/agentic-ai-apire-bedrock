# MCP Routing Service API Documentation

## Overview
The MCP Routing Service provides optimized routing solutions for multiple transportation modes with features like real-time updates, fare estimation, and weather integration.

## Authentication
All API endpoints require an API key to be included in the `X-API-Key` header.

Example:
```http
X-API-Key: your_api_key_here
```

## Subscription Tiers

### Free Tier
- 100 requests per day
- Basic routing (Walk, Car)
- No real-time updates
- No alternative routes

### Basic Tier ($29.99/month)
- 1,000 requests per day
- Additional transport mode: Bus
- Alternative routes
- Basic analytics

### Premium Tier ($99.99/month)
- 10,000 requests per day
- All transport modes
- Real-time updates
- Weather integration
- Route optimization
- Advanced analytics

### Enterprise Tier ($499.99/month)
- 100,000 requests per day
- Custom integration options
- Priority support
- SLA guarantees

## API Endpoints

### Get Routes
Calculate routes between two points with various transportation modes.

```http
POST /api/routing/routes
```

Request body:
```json
{
  "source": {
    "latitude": 47.6062,
    "longitude": -122.3321,
    "address": "Seattle, WA"
  },
  "destination": {
    "latitude": 47.6205,
    "longitude": -122.3493,
    "address": "Space Needle, Seattle"
  },
  "mode": "Car",
  "preferences": [
    "minimize_time",
    "avoid_tolls"
  ],
  "departureTime": "2024-03-23T15:00:00Z"
}
```

### Get Route Details
Get detailed information about a specific route.

```http
GET /api/routing/routes/{routeId}
```

### Get Real-Time Updates
Get real-time updates for a route (Premium and Enterprise tiers only).

```http
GET /api/routing/routes/{routeId}/updates
```

### Get Fare Estimate
Get detailed fare estimates for a route.

```http
GET /api/routing/routes/{routeId}/fare
```

### Get Weather Information
Get weather information along a route (Premium and Enterprise tiers only).

```http
GET /api/routing/routes/{routeId}/weather
```

### Get Route Analytics
Get detailed analytics for a route (Basic tier and above).

```http
GET /api/routing/routes/{routeId}/analytics
```

### Optimize Route
Optimize a route based on specific preferences (Premium and Enterprise tiers only).

```http
POST /api/routing/routes/{routeId}/optimize
```

Request body:
```json
{
  "minimizeTime": true,
  "minimizeCost": false,
  "avoidHighTraffic": true,
  "preferScenicRoute": false,
  "considerWeather": true,
  "customWeights": {
    "traffic": 0.8,
    "weather": 0.6
  }
}
```

## Error Codes

| Code | Description |
|------|-------------|
| 401  | Invalid or missing API key |
| 403  | Feature not available in current subscription tier |
| 429  | Rate limit or quota exceeded |
| 500  | Internal server error |

## Rate Limiting
- Rate limits are enforced per API key
- Limits reset daily at midnight UTC
- Rate limit headers are included in responses

## Best Practices
1. Cache route results when possible
2. Use webhook notifications for real-time updates
3. Implement exponential backoff for retries
4. Monitor your usage through analytics endpoints

## Support
For technical support or questions:
- Email: support@mcprouting.com
- API Status: status.mcprouting.com
- Documentation: docs.mcprouting.com