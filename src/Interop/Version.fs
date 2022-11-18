namespace BehideServer

open System.IO
open Microsoft.Extensions.Configuration

type Version =
    static member private config =
        ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("./appsettings.json", false, false)
            .Build()

    static member GetVersion() : string =
        Version.config.GetValue("version")
