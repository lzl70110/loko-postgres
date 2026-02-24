# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution
COPY *.sln ./

# Copy all project files (critical for dotnet restore)
COPY Loco1.web/*.csproj Loco1.web/
COPY Loco1.Data/*.csproj Loco1.Data/
COPY Loco1.Data.Models/*.csproj Loco1.Data.Models/
COPY Loco1.Service/*.csproj Loco1.Service/
COPY Loco1.Services/*.csproj Loco1.Services/
COPY Loco1.ViewModels/*.csproj Loco1.ViewModels/
COPY Loco1.Localizer/*.csproj Loco1.Localizer/
COPY GCommon/*.csproj GCommon/
# Restore
RUN dotnet restore

# Copy source
COPY . .

# Publish (Release)
RUN dotnet publish Loco1.web/Loco1.Web.csproj -c Release -o /app/publish

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Loco1.Web.dll"]
