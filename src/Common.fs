module BehideServer.Common

open Serilog
open System.Net

#if DEBUG
let isRelease = false
#else
let isRelease = true
#endif

let logger =
    let mutable config = LoggerConfiguration()

    if not isRelease then
        config <- config.MinimumLevel.Debug()
    else
        config <-
            config
                .MinimumLevel.Information()
                .WriteTo.File("logs/log.txt", rollingInterval = RollingInterval.Day)

    config
        .WriteTo.Console()
        .CreateLogger()

let getLocalIP () =
    Dns.GetHostName()
    |> Dns.GetHostEntry
    |> fun x -> x.AddressList
    |> Array.find (fun x -> x.AddressFamily = Sockets.AddressFamily.InterNetwork)