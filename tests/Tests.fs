module BehideServer.Tests.Program

open Expecto

let server =
    async {
        BehideServer.Program.main [||] |> ignore
    }

[<EntryPoint>]
let main args =
    server |> Async.Start

    runTestsInAssemblyWithCLIArgs [] args