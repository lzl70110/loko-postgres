# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1) Solution
COPY *.sln ./

# 2) Copy ONLY csproj for restore layering (добавяме липсващия проект)
COPY Loco1.web/*.csproj           Loco1.web/
COPY Loco1.Data/*.csproj          Loco1.Data/
COPY Loco1.Data.Models/*.csproj   Loco1.Data.Models/
COPY Loco1.Service/*.csproj       Loco1.Service/
COPY Loco1.Services/*.csproj      Loco1.Services/
COPY Loco1.ViewModels/*.csproj    Loco1.ViewModels/
COPY Loco1.Localizer/*.csproj     Loco1.Localizer/
COPY GCommon/*.csproj             GCommon/
# 👉 Нов ред:
COPY Loco1.Repositories/*.csproj  Loco1.Repositories/

# 3) Restore (на .sln)
RUN dotnet restore

# 4) Copy all sources
COPY . .

# 5) Publish (внимавай с ИМЕТО на csproj: Web или web)
# Ако файлът е Loco1.web.csproj:
# RUN dotnet publish Loco1.web/Loco1.web.csproj -c Release -o /app/publish --no-restore
# Ако файлът е Loco1.Web.csproj:
RUN dotnet publish Loco1.web/Loco1.Web.csproj -c Release -o /app/publish --no-restore

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Render ще ти подаде $PORT; вържи Kestrel за него
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

COPY --from=build /app/publish .

# Също тук внимавай за името на DLL:
# Ако е Loco1.web.dll → сложи него. Ако е Loco1.Web.dll → остави така:
ENTRYPOINT ["dotnet", "Loco1.Web.dll"]