FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine as build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# copy and build everything else
COPY . ./
RUN dotnet publish -c Release -o out

# Copy build output to runtime container
FROM mcr.microsoft.com/dotnet/core/runtime:3.1-alpine AS runtime
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "IotRelay.Service.dll"]
