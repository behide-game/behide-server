module BehideServer.Tests.Parsing

open Expecto
open BehideServer.Tests.Common
open BehideServer.Types

[<Tests>]
let tests =
    testList "Parsing" [
        testCase "Id (Guid)" (fun _ ->
            let sourceId = Id.NewGuid()
            let parsedId =
                sourceId
                |> Id.ToBytes
                |> Id.TryParseBytes
                |> Expect.wantSome "Id should be parsable."

            Expect.equal parsedId parsedId "Ids should equal."
        )

        testCase "RoomId" (fun _ ->
            let sourceRoomId = RoomId.Create()
            let parsedRoomId =
                sourceRoomId
                |> RoomId.ToBytes
                |> RoomId.TryParseBytes
                |> Expect.wantSome "RoomId should be parsable."

            Expect.equal sourceRoomId parsedRoomId "RoomIds should equal."
        )

        testCase "PlayerId" (fun _ ->
            let source = Id.CreateOf PlayerId
            let parsed =
                source
                |> PlayerId.ToBytes
                |> PlayerId.TryParseBytes
                |> Expect.wantSome "PlayerId should be parsable."

            Expect.equal source parsed "PlayerIds should equal."
        )
    ]