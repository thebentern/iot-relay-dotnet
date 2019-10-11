FROM mcr.microsoft.com/dotnet/core/sdk:3.0-alpine
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# copy and build everything else
COPY . ./
RUN dotnet publish -c Release -o out
ENTRYPOINT ["dotnet", "out/IotRelay.Service.dll"]