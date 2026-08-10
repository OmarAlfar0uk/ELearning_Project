# ============================================
# Stage 1: Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0-noble AS build
WORKDIR /src

# ننسخ الـ csproj الأول عشان الـ Cache
COPY ELearningProject/ELearningProject.csproj ELearningProject/

# Restore
RUN dotnet restore ELearningProject/ELearningProject.csproj

# ننسخ باقي الكود
COPY ELearningProject/ ELearningProject/

# Build + Publish
RUN dotnet publish ELearningProject/ELearningProject.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ============================================
# Stage 2: Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0-noble AS runtime
WORKDIR /app

# Non-root user
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# نسخ الـ Output
COPY --from=build /app/publish .

# Port
EXPOSE 8080

# Environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Healthcheck
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "ELearningProject.dll"]
