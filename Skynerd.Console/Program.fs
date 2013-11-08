open System

open Microsoft.Owin.Hosting
open Owin

open Skynerd.Server

let startWebServer (app: IAppBuilder) =
    app.UseRequestProcessor("GET", "/", fun i -> "Hello World!") |> ignore

[<EntryPoint>]
let main argv =
    let url = "http://localhost:5002"
    use disposable = WebApp.Start(url, startWebServer)
    Console.WriteLine("Server running on " + url)
    Console.WriteLine("Press Enter to stop.")
    Console.ReadLine() |> ignore
    0
