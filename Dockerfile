# ###############################
# .NET build stage
# ###############################

FROM microsoft/dotnet:2.1-sdk

WORKDIR /app
COPY host /app
RUN dotnet publish -c Release -r linux-x64 --self-contained false -o out

# ###############################
# Java build stage
# ###############################

FROM openjdk:8u131-jdk-alpine

WORKDIR /app
COPY daemon /app
RUN ./gradlew --no-daemon installDist

# ###############################
# Deploy stage
# ###############################

FROM microsoft/dotnet:2.1.2-runtime

## Set LOCALE to UTF8

ENV DEBIAN_FRONTEND noninteractive
RUN apt-get update && apt-get install -y locales
RUN echo "en_US.UTF-8 UTF-8" > /etc/locale.gen && \
    locale-gen en_US.UTF-8 && \
    dpkg-reconfigure locales && \
    /usr/sbin/update-locale LANG=en_US.UTF-8
ENV LC_ALL en_US.UTF-8

## Java & Kotlin

RUN apt-get update && apt-get install zip unzip && \
    curl -s "https://get.sdkman.io" | bash && \
    /bin/bash -c "source $HOME/.sdkman/bin/sdkman-init.sh; \
    sdk install java;"
ENV PATH=$PATH:/root/.sdkman/candidates/kotlin/current/bin:/root/.sdkman/candidates/java/current/bin

WORKDIR /app
COPY --from=0 /app/publish .
COPY --from=1 /app/build/install /bin

ENTRYPOINT ["dotnet", "SlackToTelegramBot.dll"]
