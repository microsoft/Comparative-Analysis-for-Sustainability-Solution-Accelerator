# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT License.

#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app

ENV AZURE_TOKEN_CREDENTIALS=ManagedIdentityCredential

#ENV ASPNETCORE_ENVIRONMENT Development
#ENV ASPNETCORE_URLS http://+:80

WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["CFS.SK.Sustainability.AI.Host/CFS.SK.Sustainability.AI.Host.csproj", "CFS.SK.Sustainability.AI.Host/"]
COPY ["CFS.SK.Sustainability.AI.Storage/CFS.SK.Sustainability.AI.Storage.csproj", "CFS.SK.Sustainability.AI.Storage/"]
COPY ["CFS.SK.Sustainability.Abstract/CFS.SK.Sustainability.Abstracts.csproj", "CFS.SK.Sustainability.Abstract/"]
COPY ["CFS.SK.Sustainability.AI/CFS.SK.Sustainability.AI.csproj", "CFS.SK.Sustainability.AI/"]
COPY ["WebClient/WebClient.csproj", "WebClient/"]
RUN dotnet restore "./CFS.SK.Sustainability.AI.Host/CFS.SK.Sustainability.AI.Host.csproj"
COPY . .
WORKDIR "/src/CFS.SK.Sustainability.AI.Host"
RUN dotnet build "./CFS.SK.Sustainability.AI.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "./CFS.SK.Sustainability.AI.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final

WORKDIR /app

USER root
RUN apt-get update -y && apt-get install -y wkhtmltopdf
# Set the XDG_RUNTIME_DIR environment variable
ENV XDG_RUNTIME_DIR=/tmp/runtime-root
# Create the runtime directory
RUN mkdir -p /tmp/runtime-root && chmod 700 /tmp/runtime-root

USER app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "CFS.SK.Sustainability.AI.Host.dll"]