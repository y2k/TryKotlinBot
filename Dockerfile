# ###############################
# .NET build stage
# ###############################

FROM mcr.microsoft.com/dotnet/core/sdk:3.1.201-alpine3.10

WORKDIR /app
COPY . /app
RUN echo $PWD && ls -Rl
RUN dotnet publish -c Release -r linux-x64 --self-contained false -o bin/publish

# ###############################
# Java build stage
# ###############################

FROM openjdk:8u212-jdk-alpine

WORKDIR /app
COPY daemon /app
RUN ./gradlew --no-daemon installDist

# Install Java

RUN wget https://github.com/AdoptOpenJDK/openjdk8-binaries/releases/download/jdk8u242-b08/OpenJDK8U-jre_x64_linux_hotspot_8u242b08.tar.gz
RUN tar -xf OpenJDK8U-jre_x64_linux_hotspot_8u242b08.tar.gz

# ###############################
# Deploy stage
# ###############################

FROM mcr.microsoft.com/dotnet/core/runtime:3.1.3-buster-slim

## Set LOCALE to UTF8

ENV DEBIAN_FRONTEND noninteractive
RUN apt-get update && apt-get install -y locales
RUN echo "en_US.UTF-8 UTF-8" > /etc/locale.gen && \
    locale-gen en_US.UTF-8 && \
    dpkg-reconfigure locales && \
    /usr/sbin/update-locale LANG=en_US.UTF-8
ENV LC_ALL en_US.UTF-8

## Deploy binaries and check java

WORKDIR /app
COPY --from=0 /app/bin/publish .
COPY --from=1 /app/build/install ./bin
COPY --from=1 /app/jdk8u242-b08-jre /jre

ENV PATH="/jre/bin:${PATH}"
RUN java -version

ENTRYPOINT ["dotnet", "TryKtBot.dll"]
