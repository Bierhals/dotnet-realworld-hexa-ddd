FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY src/backend/RestAPI/*.csproj RestAPI/
COPY src/backend/Application/*.csproj Application/
COPY src/backend/Domain/*.csproj Domain/
COPY src/backend/Persistence/*.csproj Persistence/
COPY src/backend/Directory.* ./
RUN dotnet restore RestAPI/RestAPI.csproj

# copy and build app and libraries
COPY src/backend/RestAPI/* RestAPI/
COPY src/backend/Application/* Application/
COPY src/backend/Domain/* Domain/
COPY src/backend/Persistence/* Persistence/

# test stage -- exposes optional entrypoint
# target entrypoint with: docker build --target test
FROM build AS test
COPY src/backend/* .
RUN dotnet restore
RUN dotnet build --no-restore
ENTRYPOINT ["dotnet", "test", "--logger:trx", "--no-build"]


FROM build AS publish
WORKDIR /src/RestAPI
RUN dotnet publish --no-restore -o /app

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 8080
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["./Conduit.RestAPI"]
