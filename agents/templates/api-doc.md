# API Endpoint Documentation Template

### {EndpointPath}

**Method**: {HTTP_Method}

**Description**
{Description of what the endpoint does}

**Authentication**
{authentication_requirements}

**Request**
- Content-Type: {content_type}
- Accept: {accept_type}

**Headers**
{#each headers}
- `{name}`: {description}
{/each}

**Query Parameters**
{#each query_params}
- `{name}` ({type}): {description} {required_optional}
{/each}

**Request Body**
```json
{request_body_example}
```

**Response**
- Status: {success_status_code}
```json
{response_body_example}
```

**Error Responses**
{#each error_responses}
- Status: {status_code}
```json
{error_response_example}
```
{/each}

**Rate Limits**
{rate_limit_description}

**Example**
```bash
{curl_example}
```

**Notes**
{additional_notes}

**See Also**
{#each related_endpoints}
- [{name}]({link})
{/each}