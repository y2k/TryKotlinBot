FROM microsoft/dotnet:latest

RUN deb http://httpredir.debian.org/debian/ jessie main contrib
RUN apt-get update && apt-get install java-package && exit

RUN curl -s https://get.sdkman.io | bash
RUN sdk install kotlin

ARG source=.
WORKDIR /app
COPY $source .

RUN dotnet restore

CMD ["/bin/bash", "-c", "dotnet run $TOKEN"]