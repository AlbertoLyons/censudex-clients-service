# Makefile for windows

# Setup for dotnet
setup:
	@echo "$(GREEN)Setting up project...$(NC)"
	@echo "$(YELLOW)1. Restoring packages...$(NC)"
	@dotnet restore
	@echo "$(YELLOW)2. Installing tools...$(NC)"
	@dotnet tool install --global dotnet-ef || echo "$(YELLOW)EF Core tools already installed$(NC)"
	@echo "$(YELLOW)3. Building project...$(NC)"
	@dotnet build
	@echo "$(GREEN)Project setup completed!$(NC)"
	@echo "$(YELLOW)Run 'make dev' to start development$(NC)"

# Build the project
build:
	@echo "$(GREEN)Building project...$(NC)"
	@dotnet build
	@echo "$(GREEN)Build completed$(NC)"

# Restore NuGet packages
restore:
	@echo "$(GREEN)Restoring NuGet packages...$(NC)"
	@dotnet restore
	@echo "$(GREEN)Packages restored$(NC)"

# Start PSQL container
db-up:
	@echo "$(GREEN)Starting PSQL container with user postgres and password 12345678...$(NC)"
	@docker run --name censudex-db-clients -e POSTGRES_PASSWORD=12345678 -p 5432:5432 -d postgres:latest || \
	(echo "$(YELLOW)Container already exists, starting...$(NC)" && docker start censudex-db-clients)
	@echo "$(YELLOW)Waiting for PSQL to be ready...$(NC)"
	@powershell -Command "Start-Sleep" 5
	@echo "$(GREEN)PSQL is ready$(NC)"

# Stop PSQL container
db-down:
	@echo "$(GREEN)Stopping PSQL container...$(NC)"
	@docker stop censudex-db-clients 2>/dev/null || echo "$(YELLOW)Container was not running$(NC)"
	@echo "$(GREEN)PSQL stopped$(NC)"
# Create initial migration
migrate-create:
	@echo "$(GREEN)Creating initial migration...$(NC)"
	@cd $(HOST_PATH) && dotnet ef migrations add InitialCreate --output-dir src/data/migrations
	@echo "$(GREEN)Initial migration created$(NC)"
# Apply migrations to the database
migrate-apply:
	@echo "$(GREEN)Applying migrations to the database...$(NC)"
	@cd $(HOST_PATH) && dotnet ef database update
	@echo "$(GREEN)Migrations applied$(NC)"
# Run the application
run:
	@echo "$(GREEN)Starting application with watch (no hot reload)...$(NC)"
	@cd $(HOST_PATH) && dotnet watch run --no-hot-reload