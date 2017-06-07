FROM microsoft/dotnet:latest

ARG source=.
WORKDIR /app
COPY $source .

RUN dotnet restore

CMD ["/bin/bash", "-c", "dotnet run $TOKEN"]