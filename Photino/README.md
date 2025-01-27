# Photino Demo: Desktop

This is a demo of a desktop application, build with Photino.NET using static web assets as frontend (SPA).

## Getting Started

- Start UI dev server in `Frontend`
    `deno task dev`
- Start .NET app
    `dotnet run` 

## Deployment

- Build UI in `Frontend` folder
    `deno task build`
- Build Desktop
    `dotnet publish -c Release -r win-x64 -v diag`
    `dotnet publish -c Release -r linux-x64 -v diag`
    