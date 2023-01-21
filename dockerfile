FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /App

COPY ./src ./src
COPY ./.config ./.config
COPY ./paket.dependencies ./paket.dependencies
COPY ./paket.lock ./paket.lock

RUN dotnet tool restore
RUN dotnet paket restore
RUN dotnet restore ./src/BehideServer
RUN dotnet publish ./src/BehideServer -c Release -o ./build

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /App
COPY --from=build-env /App/build .
ENTRYPOINT ["dotnet", "BehideServer.dll"]