# AspireApp.BedRock.SonetOps

A modern .NET Aspire application integrating AWS Bedrock with a Model Control Panel (MCP) for processing and handling responses through a Sonet model interface.

## Skills

- **.NET Core & ASP.NET Core**: Advanced web application development
- **AWS Bedrock**: AI/ML model integration and management
- **Microservices Architecture**: Distributed system design
- **Model Control Panel (MCP)**: AI model orchestration and monitoring
- **RESTful APIs**: API design and implementation
- **Real-time Communication**: SignalR integration
- **Docker & Containerization**: Application containerization
- **CI/CD**: Automated deployment pipelines
- **Testing**: Unit, integration, and performance testing
- **Git & GitHub**: Version control and collaboration
- **GitHub Actions**: Automated workflows and CI/CD

## Git Workflow

1. **Initialize Repository**
   ```bash
   git init
   git remote add origin <repository-url>
   ```

2. **Basic Commands**
   ```bash
   git status                    # Check status of changes
   git add .                    # Stage all changes
   git commit -m "message"      # Commit changes
   git push -u origin main      # Push to main branch
   ```

3. **Branch Management**
   ```bash
   git branch -M main           # Rename branch to main
   git checkout -b feature      # Create and switch to new branch
   git merge feature            # Merge feature into current branch
   ```

4. **Sync with Remote**
   ```bash
   git fetch                    # Get remote changes
   git pull                     # Pull remote changes
   git push                     # Push local changes
   ```

5. **Handle Merge Conflicts**
   ```bash
   git pull                     # Get changes that might conflict
   # Resolve conflicts in files
   git add .                    # Stage resolved files
   git commit -m "Resolve conflicts"
   git push                     # Push resolved changes
   ```

## Project Structure

The solution consists of several projects:

- **AspireApp.BedRock.SonetOps.AppHost**: Orchestration project that manages all services
- **AspireApp.BedRock.SonetOps.ServiceDefaults**: Common service configurations
- **AspireApp.BedRock.SonetOps.ApiService**: AWS Bedrock integration service
- **AspireApp.BedRock.SonetOps.MCP**: Model Control Panel service
- **AspireApp.BedRock.SonetOps.Web**: Web frontend
- **AspireApp.BedRock.SonetOps.Tests**: Integration tests