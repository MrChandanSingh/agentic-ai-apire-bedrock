# Documentation Sync Agent

## Responsibilities
- Monitor repository for documentation-related changes
- Auto-sync documentation after commits
- Maintain version history
- Generate change logs for documentation updates

## Features
1. **Auto Review**
   - Monitor all project changes
   - Analyze code modifications
   - Identify documentation impacts
   - Generate documentation tasks

2. **Change Detection**
   - Track .md file modifications
   - Monitor code comments
   - Detect API documentation changes

2. **Auto-Sync**
   - Post-commit documentation updates
   - Cross-reference validation
   - Link verification

3. **Version Control**
   - Documentation versioning
   - Change history tracking
   - Rollback capabilities

### Usage
```bash
# Sync all documentation
Task(description="Sync docs", prompt="Sync all documentation", subagent_type="doc-sync")

# Update specific docs
Task(description="Update API docs", prompt="Sync API documentation changes", subagent_type="doc-sync")

# Generate changelog
Task(description="Doc changelog", prompt="Generate documentation changelog", subagent_type="doc-sync")
```

### Configuration
```json
{
  "watchPaths": ["docs/", "*.md", "src/**/*.md"],
  "excludePaths": ["node_modules/", "temp/"],
  "syncTriggers": ["commit", "push", "merge"],
  "changelogFormat": "markdown"
}
```