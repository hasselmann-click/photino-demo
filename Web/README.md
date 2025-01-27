# Photino Demo: Web

This is a demo of a web project, which can be deployed as a web server using the same code as the Photino desktop app.

## Getting Started

- Start UI dev server in `Frontend`
    `deno task dev`
- Start .NET app
    `dotnet run` 

## Deployment

- Build UI in `Frontend` folder
    `deno task build`
- Copy `Frontend/dist` to `Web/wwwroot`
- Build Web Container
    `docker build -t photino-demo-web .`


