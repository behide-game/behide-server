module BehideServer.Tests.Parsing

open Expecto
open BehideServer.Tests.Common
open BehideServer.Types

[<Tests>]
let tests =
    testList "Parsing" [
        testCase "RoomId" (fun _ ->
            let sourceRoomId = RoomId.Create()
            let parsedRoomId =
                sourceRoomId
                |> RoomId.ToBytes
                |> RoomId.TryParseBytes
                |> Expect.wantSome "RoomId should be parsable"

            Expect.equal sourceRoomId parsedRoomId "RoomIds should equals"
        )
    ]