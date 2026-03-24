# Available Agents

- general-purpose: For complex research and multi-step tasks
- chandan-code-reviewer: Comprehensive code review agent focused on code quality, security, and best practices
- security-code-reviewer: Specialized security analysis for payment integrations and sensitive data handling
- documentation-generator: Advanced documentation generation for code, APIs, and project documentation

# Git Commands and Workflow

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