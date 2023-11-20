namespace Transcriber.Client
open System
open System.Net.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Components.WebAssembly.Hosting
open Bolero.Remoting.Client

module Program =
    let createHttpClient baseAddr _ = new HttpClient(BaseAddress = Uri(baseAddr))

    [<EntryPoint>]
    let Main args =
        let builder = WebAssemblyHostBuilder.CreateDefault(args)
        builder.RootComponents.Add<App.MyApp>("#main")
        builder.Services.AddScoped<HttpClient>(createHttpClient builder.HostEnvironment.BaseAddress) |> ignore
        builder.Services.AddBoleroRemoting(builder.HostEnvironment) |> ignore
        let app = builder.Build()
        //app.Services.SetupErrorHandlingJSInterop() |> Async.AwaitTask |> Async.Ignore |> Async.Start
        app.RunAsync() |> ignore
        0
