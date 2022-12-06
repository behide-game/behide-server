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
                .MinimumLevel
                .Information()
                .WriteTo.File("logs/log.txt", rollingInterval = RollingInterval.Day)

    config.WriteTo.Console().CreateLogger()

type Log =
    static member debug format = format |> Printf.ksprintf logger.Debug
    static member verbose format = format |> Printf.ksprintf logger.Verbose
    static member info format = format |> Printf.ksprintf logger.Information
    static member warning format = format |> Printf.ksprintf logger.Warning
    static member error format = format |> Printf.ksprintf logger.Error
    static member fatal format = format |> Printf.ksprintf logger.Fatal