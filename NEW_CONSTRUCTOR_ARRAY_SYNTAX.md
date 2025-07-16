# DotVVM Binding Expression: New Constructor and Array Syntax Support

## Summary

Enhanced the DotVVM binding expression parser to support additional C# constructor and array creation syntax patterns, including type-inferred constructors and array construction syntax.

## New Features Implemented

### 1. Type-Inferred Constructor Calls
- **Syntax**: `new(arg1, arg2, arg3)`
- **Description**: Allows constructor calls without explicitly specifying the type name, relying on type inference
- **Examples**:
  - `new(1, 2, 3)` - Constructor with arguments
  - `new()` - Parameterless constructor

### 2. Array Construction with Size
- **Syntax**: `new int[size]`
- **Description**: Creates arrays with a specified size
- **Examples**:
  - `new int[5]` - Creates an integer array of size 5
  - `new string[length]` - Creates a string array with variable size

### 3. Array Construction with Initializers
- **Syntax**: `new int[] { 1, 2, 3 }` or `new[] { 1, 2, 3 }`
- **Description**: Creates arrays with explicit initializer values
- **Examples**:
  - `new int[] { 1, 2, 3 }` - Typed array with initializers
  - `new[] { 1, 2, 3 }` - Type-inferred array with initializers
  - `new string[] { }` - Empty array with explicit type

### 4. Enhanced Error Handling
- **Multi-dimensional Arrays**: Provides clear error message that multi-dimensional arrays are not supported
- **Missing Parentheses**: Constructor calls without parentheses (`new MyClass`) now produce syntax errors as required
- **Missing Initializers**: Array syntax without proper initializers produces helpful error messages

## Technical Implementation

### New Parser Nodes
1. **`TypeInferredConstructorCallBindingParserNode`**: Handles `new(...)` syntax
2. **`ArrayConstructionBindingParserNode`**: Handles all array construction patterns

### Extended Tokenizer
- Added support for curly braces `{` and `}` tokens (`OpenCurlyBrace`, `CloseCurlyBrace`)
- Updated tokenizer to recognize these new tokens in array initializer syntax

### Parser Enhancements
- Extended `ReadConstructorCallExpression()` to handle all three patterns:
  - Regular constructor calls: `new MyClass(args)`
  - Type-inferred constructor calls: `new(args)`
  - Array construction: `new int[size]` and `new[] { ... }`

### Comprehensive Test Coverage
- Added tests for all new syntax patterns
- Verified error handling for unsupported features
- Ensured backward compatibility with existing syntax

## Usage Examples in DotVVM Bindings

```html
<!-- Type-inferred constructor -->
{value: new(someArg, anotherArg)}

<!-- Array with size -->
{value: new int[10]}

<!-- Array with initializers -->
{value: new[] { item1, item2, item3 }}
{value: new string[] { "hello", "world" }}

<!-- Nested in expressions -->
{value: SomeMethod(new[] { 1, 2, 3 })}
```

## Breaking Changes
- `new MyClass` (without parentheses) is now a syntax error, as per C# language requirements
- Multi-dimensional array syntax like `new int[5, 3]` produces clear error messages

## Test Results
- All existing tests continue to pass (2019+ tests)
- New comprehensive test suite covering all new features
- Proper error message validation for unsupported patterns
