FROM microsoft/dotnet:latest

# Java & Kotlin

RUN apt-get update && apt-get install zip unzip && \
    curl -s "https://get.sdkman.io" | bash && \
    /bin/bash -c "source $HOME/.sdkman/bin/sdkman-init.sh; \
    sdk install java; \
    sdk install kotlin;"

# .NET Core

ARG source=.
WORKDIR /app
COPY $source .

RUN dotnet restore

CMD ["/bin/bash", "-c", "dotnet run $TOKEN"]