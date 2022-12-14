module BehideServer.Tests.Program

open Expecto

[<EntryPoint>]
let main args =
    BehideServer.Log.disableLogs ()

    async { BehideServer.Program.main [||] |> ignore }
    |> Async.Start

    runTestsInAssemblyWithCLIArgs [ JUnit_Summary "TestResults.xml" ] args
