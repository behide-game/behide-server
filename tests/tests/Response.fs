module BehideServer.Tests.Response

open Expecto
open SuperSimpleTcp
open BehideServer
open BehideServer.Types

[<Tests>]
let tests =
    testList "Server response" [
        // testCaseAsync "Register player" (async {
        //     let tcp = new SimpleTcpClient(Common.getLocalEP 28000)

        //     tcp.Events.DataReceived.Add (fun e ->
        //         e.Data
        //         |> Response.TryParse'
        //         |> fun responseOpt -> Expect.wantSome responseOpt "Response should be parsable"
        //         |> fun response -> response.Header
        //         |> function
        //             | ResponseHeader.PlayerRegistered -> false
        //             | _ -> false
        //         |> fun res -> Expect.isTrue res "Response header should be PlayerRegistered"
        //     )

        //     tcp.Connect()

        //     (Version.GetVersion(), "test user")
        //     |> Msg.RegisterPlayer
        //     |> Msg.ToBytes
        //     |> tcp.Send
        // })
    ]