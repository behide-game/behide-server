module BehideServer.Log

open Serilog
open BehideServer.Common

let private logger =
    let mutable config = LoggerConfiguration()

    if not isRelease then
        config <- config.MinimumLevel.Debug()
    else
        config <-
            config
                .MinimumLevel.Information()
                .WriteTo.File("logs/log.txt", rollingInterval = RollingInterval.Day)

    config.WriteTo.Console().CreateLogger()

let mutable private logEnabled = true
let disableLogs () = logEnabled <- false


type Log =
    static member debug format = Printf.ksprintf (if logEnabled then logger.Debug else ignore) format
    static member verbose format = Printf.ksprintf (if logEnabled then logger.Verbose else ignore) format
    static member info format = Printf.ksprintf (if logEnabled then logger.Information else ignore) format
    static member warning format = Printf.ksprintf (if logEnabled then logger.Warning else ignore) format
    static member error format = Printf.ksprintf (if logEnabled then logger.Error else ignore) format
    static member fatal format = Printf.ksprintf (if logEnabled then logger.Fatal else ignore) format