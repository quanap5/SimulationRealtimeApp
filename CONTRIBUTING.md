# Contributing to SimulationRealtimeApp

Thank you for your interest in contributing to SimulationRealtimeApp! This document provides guidelines and instructions for contributing.

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/) (optional, for containerized development)
- A code editor (Visual Studio, VS Code, or Rider recommended)

### Setting Up the Development Environment

1. Clone the repository:
   ```bash
   git clone https://github.com/YOUR_USERNAME/SimulationRealtimeApp.git
   cd SimulationRealtimeApp
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the project:
   ```bash
   dotnet build
   ```

4. Run the tests:
   ```bash
   dotnet test
   ```

5. Run the application:
   ```bash
   dotnet run --project SimulationRealtimeApp
   ```

## Code Style

### C# Conventions

- Use 4 spaces for indentation (no tabs)
- Follow Microsoft's [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful names for variables, methods, and classes
- Keep methods focused and short (generally under 30 lines)
- Add XML documentation comments for public APIs

### Project Structure

```
SimulationRealtimeApp/
├── Controllers/     # API controllers
├── Data/           # Database context and entities
├── Hubs/           # SignalR hubs
├── Models/         # DTOs and view models
├── Repositories/   # Data access layer
├── Services/       # Business logic
└── wwwroot/        # Static web files
```

## Making Changes

### Branch Naming

Use descriptive branch names following this convention:
- `feature/description` - for new features
- `bugfix/description` - for bug fixes
- `docs/description` - for documentation updates
- `refactor/description` - for code refactoring

### Commit Messages

Write clear, concise commit messages:
- Use present tense ("Add feature" not "Added feature")
- Use imperative mood ("Move cursor" not "Moves cursor")
- Limit the first line to 72 characters
- Reference issues when applicable

Example:
```
Add pagination to history API

- Implement skip/take parameters in repository
- Add page and pageSize query parameters to controller
- Update API documentation

Fixes #42
```

## Pull Request Process

1. **Create a feature branch** from `main` or `develop`

2. **Make your changes** following the code style guidelines

3. **Write or update tests** for your changes
   ```bash
   dotnet test
   ```

4. **Update documentation** if needed

5. **Push your branch** and create a pull request

6. **Describe your changes** in the PR description:
   - What changes were made
   - Why the changes were made
   - Any breaking changes
   - Screenshots (for UI changes)

7. **Wait for review** - maintainers will review your PR

### PR Checklist

Before submitting:
- [ ] Code compiles without errors
- [ ] All tests pass
- [ ] New code has test coverage
- [ ] Documentation is updated
- [ ] No unnecessary files are included
- [ ] Commit messages are clear

## Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run with verbose output
dotnet test --verbosity normal

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Writing Tests

- Use xUnit for unit tests
- Use FluentAssertions for readable assertions
- Use Moq for mocking dependencies
- Follow the Arrange-Act-Assert pattern
- Name tests descriptively: `MethodName_Scenario_ExpectedResult`

Example:
```csharp
[Fact]
public void Start_ShouldSetIsRunningToTrue()
{
    // Arrange
    var service = new SimulationService();

    // Act
    service.Start();

    // Assert
    service.IsRunning.Should().BeTrue();
}
```

## Reporting Issues

When reporting issues, please include:

1. **Description** of the issue
2. **Steps to reproduce**
3. **Expected behavior**
4. **Actual behavior**
5. **Environment details** (OS, .NET version, browser)
6. **Screenshots or logs** if applicable

## Questions?

If you have questions, feel free to:
- Open an issue for discussion
- Check existing issues and documentation

Thank you for contributing!
