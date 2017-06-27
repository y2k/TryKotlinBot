FROM microsoft/dotnet:latest

## Set LOCALE to UTF8

ENV DEBIAN_FRONTEND noninteractive

RUN apt-get update && apt-get install -y locales \
    echo "en_US.UTF-8 UTF-8" > /etc/locale.gen && \
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

## .NET Core

ARG source=.
WORKDIR /app
COPY $source .
RUN dotnet restore

CMD ["/bin/bash", "-c", "dotnet run $TOKEN"]