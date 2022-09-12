#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ClassAssistantBot/ClassAssistantBot.csproj ./


ENV PATH $PATH:/root/.dotnet/tools
RUN dotnet tool install -g dotnet-ef --version 6.0.7

RUN dotnet restore
COPY ./ClassAssistantBot .

RUN dotnet add package Microsoft.EntityFrameworkCore.Tools --version 6.0.7 \
    && dotnet dev-certs https
ENTRYPOINT [ "dotnet", "watch", "run", "--no-restore", "--urls", "https://0.0.0.0:880"]
