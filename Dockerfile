# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first (better Docker cache)
COPY *.sln ./
COPY Loco1.web/*.csproj Loco1.web/
COPY Loco1.Data/*.csproj Loco1.Data/
COPY Loco1.Data.Models/*.csproj Loco1.Data.Models/
COPY Loco1.Service/*.csproj Loco1.Service/
COPY Loco1.ViewModels/*.csproj Loco1.ViewModels/
COPY GCommon/*.csproj GCommon/

RUN dotnet restore

# Copy the remaining source and publish the web app
COPY . .
RUN dotnet publish Loco1.web/Loco1.Web.csproj -c Release -o /app/publish

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# (Optional) Keep ICU globalization enabled (useful for bg-BG)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

COPY --from=build /app/publish .

# EN: Program.cs binds to $PORT; no need to set ASPNETCORE_URLS here.
ENTRYPOINT ["dotnet", "Loco1.Web.dll"]