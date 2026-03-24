# Markdown Generation Rules

## Basic Formatting

1. Headers
   ```markdown
   # First Level Header
   ## Second Level Header
   ### Third Level Header
   #### Fourth Level Header
   ##### Fifth Level Header
   ###### Sixth Level Header
   ```

2. Emphasis
   ```markdown
   *italic* or _italic_
   **bold** or __bold__
   ***bold italic*** or ___bold italic___
   ```

3. Lists
   ```markdown
   1. Ordered Item 1
   2. Ordered Item 2
      - Unordered Sub-item
      - Another sub-item
   3. Ordered Item 3

   - Unordered Item
   - Another Item
     * Sub-item
     * Another sub-item
   ```

4. Code Blocks
   ````markdown
   ```language
   code goes here
   ```
   ````

## Documentation Sections

1. Method Documentation
   ```markdown
   ### MethodName

   **Description**
   Clear description of the method's purpose

   **Parameters**
   - `param1` (type): description
   - `param2` (type): description

   **Returns**
   - type: description

   **Exceptions**
   - `ExceptionType`: when it's thrown
   ```

2. Class Documentation
   ```markdown
   ## ClassName

   **Inheritance:** BaseClass

   **Implements:** Interface1, Interface2

   **Description**
   Class purpose and usage

   ### Properties
   | Name | Type | Description |
   |------|------|-------------|
   | Prop1 | Type | Details |

   ### Methods
   | Name | Return Type | Description |
   |------|-------------|-------------|
   | Method1 | Type | Details |
   ```

3. API Documentation
   ```markdown
   ### Endpoint Path

   **Method:** HTTP_METHOD

   **Description**
   What the endpoint does

   **Request**
   ```json
   {
     "field": "type"
   }
   ```

   **Response**
   ```json
   {
     "field": "type"
   }
   ```
   ```

## Link Generation

1. Internal Links
   ```markdown
   [Link Text](#section-name)
   ```

2. External Links
   ```markdown
   [Link Text](https://example.com)
   ```

3. Reference Links
   ```markdown
   [Link Text][1]
   [1]: https://example.com
   ```

## Tables

1. Simple Table
   ```markdown
   | Header 1 | Header 2 |
   |----------|----------|
   | Cell 1   | Cell 2   |
   ```

2. Aligned Table
   ```markdown
   | Left | Center | Right |
   |:-----|:------:|------:|
   | Left | Center | Right |
   ```

## Special Elements

1. Blockquotes
   ```markdown
   > Important note
   > Continues here
   ```

2. Horizontal Rules
   ```markdown
   ---
   ***
   ___
   ```

3. Task Lists
   ```markdown
   - [x] Completed task
   - [ ] Pending task
   ```

## Code Documentation

1. Method Example
   ```markdown
   ### CreateOrderAsync

   **Description**
   Creates a new payment order asynchronously.

   **Parameters**
   - `request` (PaymentRequest): Payment order details including amount and currency

   **Returns**
   - `Task<string>`: Order ID of the created payment

   **Exceptions**
   - `PaymentException`: Thrown when payment creation fails
   - `ValidationException`: Thrown when request validation fails

   **Example**
   ```csharp
   var request = new PaymentRequest { Amount = 100, Currency = "USD" };
   var orderId = await paymentService.CreateOrderAsync(request);
   ```
   ```

2. API Example
   ```markdown
   ### POST /api/v1/payments

   **Description**
   Creates a new payment order.

   **Request**
   ```json
   {
     "amount": 100.00,
     "currency": "USD"
   }
   ```

   **Response**
   ```json
   {
     "orderId": "order_123",
     "status": "created"
   }
   ```
   ```

## Generation Rules

1. Content Organization
   - Use consistent header levels
   - Keep related content together
   - Use clear section separators
   - Maintain logical flow

2. Formatting
   - Use consistent spacing
   - Align table columns
   - Indent nested lists
   - Preserve code formatting

3. Links and References
   - Use descriptive link text
   - Check link validity
   - Use reference style for repeated links
   - Add section anchors

4. Code Examples
   - Include language identifier
   - Use proper indentation
   - Add helpful comments
   - Show complete examples

## Output Validation

1. Check Structure
   - Valid headers
   - Proper nesting
   - Complete tables
   - Matched brackets

2. Verify Links
   - Internal links
   - External links
   - Section references
   - Image links

3. Code Blocks
   - Language tags
   - Syntax highlighting
   - Code indentation
   - Closing blocks