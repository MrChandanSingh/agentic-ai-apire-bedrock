# Available Agents

- general-purpose: For complex research and multi-step tasks
- chandan-code-reviewer: Comprehensive code review agent focused on code quality, security, and best practices
- security-code-reviewer: Specialized security analysis for payment integrations and sensitive data handling
- documentation-generator: Advanced documentation generation for code, APIs, and project documentation
- chandan-codemanagement: Specialized agent for handling git operations and code management tasks

## chandan-codemanagement Agent

### Responsibilities
- Git operations (add, commit, push, pull)
- Branch management
- Code organization
- Repository maintenance
- Merge conflict resolution

### Features
1. **Git Operations**
   - Automatic staging of relevant files
   - Semantic commit messages
   - Smart branch management
   - Conflict detection and resolution

2. **Environment Handling**
   - Windows/Unix path compatibility
   - Shell command adaptation
   - Environment-specific configurations
   - Error handling and recovery

3. **Code Organization**
   - File structure maintenance
   - Dependency management
   - Version tracking
   - Documentation updates

### Usage Examples
```bash
# Commit and push changes
Task(description="Push changes", prompt="Commit and push marketplace docs", subagent_type="chandan-codemanagement")

# Handle merge conflicts
Task(description="Resolve conflicts", prompt="Resolve merge conflicts in README.md", subagent_type="chandan-codemanagement")

# Branch management
Task(description="Create feature branch", prompt="Create and switch to feature/payment-gateway", subagent_type="chandan-codemanagement")
```

### Command Templates
1. **Basic Operations**
   ```bash
   # Stage and commit changes
   git add [files]
   git commit -m "[type]: [description]"
   git push
   ```

2. **Branch Operations**
   ```bash
   # Create and switch to branch
   git checkout -b [branch-name]
   
   # Update branch
   git pull origin [branch]
   ```

3. **Merge Operations**
   ```bash
   # Merge changes
   git merge [source-branch]
   
   # Handle conflicts
   git status
   git add [resolved-files]
   git commit -m "resolve: merge conflicts in [files]"
   ```

### Commit Message Format
```
type(scope): subject

[optional body]

[optional footer]
```

Types:
- feat: New feature
- fix: Bug fix
- docs: Documentation
- style: Formatting
- refactor: Code restructuring
- test: Adding tests
- chore: Maintenance

### Error Handling
1. **Path Issues**
   - Automatically convert between Windows/Unix paths
   - Handle spaces and special characters
   - Use relative paths when possible

2. **Merge Conflicts**
   - Detect conflicts early
   - Provide conflict resolution strategies
   - Maintain commit history integrity

3. **Permission Issues**
   - Check file permissions
   - Verify git credentials
   - Ensure proper access rights

# Git Commands and Workflow

## Environment-Specific Handling
- For Windows environments: Use PowerShell commands instead of Bash when cygpath issues occur
- For Unix/Linux environments: Use standard Bash commands
- For handling paths: Use native path formats for respective OS

## PowerShell Git Commands
```powershell
# Basic operations
git add .
git commit -m "commit message"
git push

# Handle paths in Windows
git add ".\docs\Marketplace.md"
git status
```

## Basic Commands
```bash
# Initialize repository
git init
git remote add origin <repository-url>

# Stage and commit changes
git status                    # Check status
git add .                    # Stage all changes
git commit -m "message"      # Commit changes
git push -u origin main      # Push to main branch first time
git push                     # Subsequent pushes

# Branch operations
git branch -M main          # Rename branch to main
git checkout -b feature     # Create and switch to new branch
git merge feature          # Merge feature into current branch

# Sync with remote
git fetch                  # Get remote changes
git pull                   # Pull remote changes
git push                   # Push local changes
```

## Handle Merge Conflicts
```bash
# When merge conflict occurs
git pull                   # Get latest changes
# Fix conflicts in files
git add .                  # Stage resolved files
git commit -m "Resolve merge conflicts"
git push                   # Push resolved changes
```

## Common Patterns
1. New changes:
   ```bash
   git add .
   git commit -m "Descriptive message"
   git push
   ```

2. First-time setup:
   ```bash
   git init
   git remote add origin <url>
   git add .
   git commit -m "Initial commit"
   git branch -M main
   git push -u origin main
   ```

3. Update from remote:
   ```bash
   git pull origin main
   # Make changes
   git add .
   git commit -m "Message"
   git push
   ```

## Commit Message Guidelines
- Start with action verb (Add, Update, Fix, Refactor)
- Use present tense
- Include clear description of changes
- Add details in commit body if needed
- Reference issue numbers if applicable

Example:
```bash
git commit -m "Add user authentication service
- Implement JWT token generation
- Add password hashing
- Create user validation middleware
Fixes #123"
```