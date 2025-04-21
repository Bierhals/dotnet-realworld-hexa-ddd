FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
RUN mkdir /app/db  && chown app /app/db
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080
USER app


FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
# copy csproj and restore as distinct layers
COPY src/backend/Host/*.csproj Host/
#COPY src/backend/RestAPI/*.csproj RestAPI/
#COPY src/backend/Application/*.csproj Application/
#COPY src/backend/Domain/*.csproj Domain/
#COPY src/backend/Persistence/*.csproj Persistence/
COPY src/backend/Directory.* ./
RUN dotnet restore "Host/Host.csproj"

# copy and build app and libraries
COPY src/backend/Host/ Host/
#COPY src/backend/RestAPI/ RestAPI/
#COPY src/backend/Application/ Application/
#COPY src/backend/Domain/ Domain/
#COPY src/backend/Persistence/ Persistence/
RUN dotnet build "Host/Host.csproj" --no-restore -c $configuration -o /app/build


# test stage -- exposes optional entrypoint
# target entrypoint with: docker build --target test
FROM build AS test
ARG configuration=Release
COPY src/backend/ .
RUN dotnet build "Host/Host.csproj" -c $configuration -o /app/build
ENTRYPOINT ["dotnet", "test", "--logger:trx", "--no-build"]


FROM build AS publish
ARG configuration=Release
RUN dotnet publish "Host/Host.csproj"  -c $configuration -o /app/publish /p:UseAppHost=false


# final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Host.dll"]
