FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["MemoARCenter/MemoARCenter.csproj", "MemoARCenter/"]
COPY ["MemoARCenter.Services/MemoARCenter.Services.csproj", "MemoARCenter.Services/"]
COPY ["MemoARCenter.Client/MemoARCenter.Client.csproj", "MemoARCenter.Client/"]

# Restore dependencies
RUN dotnet restore "MemoARCenter/MemoARCenter.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/MemoARCenter"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy the published application
COPY --from=publish /app/publish .

# Expose HTTP and HTTPS ports
EXPOSE 80
EXPOSE 443

# Define the entry point
ENTRYPOINT ["dotnet", "MemoARCenter.dll"]
