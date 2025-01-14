FROM mcr.microsoft.com/devcontainers/dotnet:1-9.0-bookworm

# install deno from their official docker image
COPY --from=denoland/deno:bin-2.1.5 /deno /usr/local/bin/deno

RUN apt-get update && apt-get install -y \
    libwebkit2gtk-4.0-37 \
    libnotify4

