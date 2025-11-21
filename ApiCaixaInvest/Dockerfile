# Acesse https://aka.ms/customizecontainer para saber como personalizar seu contêiner de depuração
# e como o Visual Studio usa este Dockerfile para criar suas imagens para uma depuração mais rápida.

# Fase base: runtime ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Fase de build: SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Se o Dockerfile está na MESMA pasta do csproj, copia só o csproj
COPY ["ApiCaixaInvest.csproj", "./"]

RUN dotnet restore "./ApiCaixaInvest.csproj"

# Agora copia o resto do código
COPY . .

RUN dotnet build "./ApiCaixaInvest.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Fase de publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ApiCaixaInvest.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Fase final: imagem para rodar
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiCaixaInvest.dll"]
