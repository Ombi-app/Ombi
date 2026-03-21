# Contributing to Ombi

Thank you for your interest in contributing to Ombi! This document provides guidelines and information for contributors.

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)
- [Yarn](https://yarnpkg.com/)
- [Git](https://git-scm.com/)

### Development Setup

1. **Fork and clone the repository**
   ```bash
   git clone https://github.com/YOUR_USERNAME/Ombi.git
   cd Ombi
   ```

2. **Install dependencies**
   ```bash
   # Install backend dependencies
   dotnet restore src

   # Install frontend dependencies
   yarn --cwd ./src/Ombi/ClientApp install
   ```

3. **Run the application**
   ```bash
   # Start the backend
   dotnet run --project src/Ombi

   # In another terminal, start the frontend
   yarn --cwd ./src/Ombi/ClientApp start
   ```

4. **Run tests**
   ```bash
   dotnet test
   ```

## 📝 How to Contribute

### Reporting Issues
- Use the [bug report template](.github/ISSUE_TEMPLATE/bug_report.yml)
- Search existing issues before creating new ones
- Provide clear reproduction steps and logs

### Suggesting Features
- Feature requests are handled on [Feature Upvote](https://features.ombi.io)
- Search existing requests before creating new ones
- Vote on existing requests if similar to your idea

### Code Contributions

1. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes**
   - Follow the coding standards (see below)
   - Write tests for new functionality
   - Update documentation if needed

3. **Test your changes**
   ```bash
   # Run all tests
   dotnet test
   yarn --cwd ./src/Ombi/ClientApp test

   # Run linting
   yarn --cwd ./src/Ombi/ClientApp lint
   ```

4. **Commit your changes**
   ```bash
   git add .
   git commit -m "feat: add your feature description"
   ```

5. **Push and create a Pull Request**
   ```bash
   git push origin feature/your-feature-name
   ```

## 🎯 Coding Standards

### Backend (.NET)
- Follow C# naming conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Write unit tests for new functionality
- Use `async/await` for I/O operations

### Frontend (Angular)
- Follow Angular best practices
- Use standalone components
- Prefer signals over observables for state management
- Use `OnPush` change detection strategy
- Write component tests

### General
- Keep functions small and focused
- Use meaningful commit messages
- Follow the existing code style
- Add comments for complex logic

## 🧪 Testing

### Running Tests
```bash
# Backend unit tests
dotnet test

# Frontend unit tests
yarn --cwd ./src/Ombi/ClientApp test

# E2E tests
yarn --cwd ./tests cypress:run
```

### Test Requirements
- New features must include tests
- Bug fixes should include regression tests
- Aim for good test coverage
- Tests should be fast and reliable

## 📋 Pull Request Process

1. **Before submitting**
   - Ensure all tests pass
   - Update documentation if needed
   - Rebase on latest develop branch
   - Squash commits if necessary

2. **PR Requirements**
   - Clear description of changes
   - Reference related issues
   - Include screenshots for UI changes
   - Ensure CI passes

3. **Review Process**
   - Maintainers will review your PR
   - Address feedback promptly
   - Be open to suggestions
   - Keep PRs focused and small

## ❓ Getting Help

- [Discord Community](https://discord.gg/Sa7wNWb)
- [Documentation](https://docs.ombi.app/)
- [FAQ](https://docs.ombi.app/info/faq/)

## 📜 Code of Conduct

Please read and follow our [Code of Conduct](CODE_OF_CONDUCT.md).

## 🙏 Recognition

Contributors are automatically recognized in our README. Thank you for contributing!

---

**Happy coding! 🎉**
