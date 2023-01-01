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
                |> Expect.wantSome "Id should be parsable"

            Expect.equal parsedId parsedId "Ids should equal"
        )

        testCase "RoomId" (fun _ ->
            let sourceRoomId = RoomId.Create()
            let parsedRoomId =
                sourceRoomId
                |> RoomId.ToBytes
                |> RoomId.TryParseBytes
                |> Expect.wantSome "RoomId should be parsable"

            Expect.equal sourceRoomId parsedRoomId "RoomIds should equal"
        )

        testCase "PlayerId" (fun _ ->
            let source = Id.CreateOf PlayerId
            let parsed =
                source
                |> PlayerId.ToBytes
                |> PlayerId.TryParseBytes
                |> Expect.wantSome "PlayerId should be parsable"

            Expect.equal source parsed "PlayerIds should equal"
        )

        testList "Msg" (
            [ "Ping", Msg.Ping
              "RegisterPlayer", Msg.RegisterPlayer ("fake-version", utf8String)
              "CreateRoom", Msg.CreateRoom (Id.CreateOf PlayerId, Id.NewGuid())
              "DeleteRoom", Msg.DeleteRoom (RoomId.Create()) ]
            |> testFixture (fun source _ ->
                let parsed =
                    source
                    |> Msg.ToBytes
                    |> Msg.TryParse
                    |> Expect.wantSome "Msg should be parsable"

                Expect.equal source parsed "Msg should equal"
            )
            |> Seq.toList
        )

        testList "Response" (
            [ "Ping", { Response.Header = ResponseHeader.Ping; Response.Content = Array.empty }
              "BadServerVersion", { Response.Header = ResponseHeader.BadServerVersion; Response.Content = Array.empty }
              "FailedToParseMsg", { Response.Header = ResponseHeader.FailedToParseMsg; Response.Content = Array.empty }
              "PlayerNotRegistered", { Response.Header = ResponseHeader.PlayerNotRegistered; Response.Content = Array.empty }
              "PlayerRegistered", { Response.Header = ResponseHeader.PlayerRegistered; Response.Content = Id.CreateOf PlayerId |> PlayerId.ToBytes }
              "RoomCreated", { Response.Header = ResponseHeader.RoomCreated; Response.Content = RoomId.Create() |> RoomId.ToBytes }
              "RoomDeleted", { Response.Header = ResponseHeader.RoomDeleted; Response.Content = Array.empty }
              "RoomNotCreated", { Response.Header = ResponseHeader.RoomNotCreated; Response.Content = Array.empty }
              "RoomNotDeleted", { Response.Header = ResponseHeader.RoomNotDeleted; Response.Content = Array.empty } ]
            |> testFixture (fun source _ ->
                let parsed =
                    source
                    |> Response.ToBytes
                    |> Response.TryParse
                    |> Expect.wantSome "Response should be parsable"

                Expect.equal source parsed "Responses should equal"
            )
            |> Seq.toList
        )

        testList "Response with content" [
            testCase "PlayerRegistered" (fun _ ->
                let playerId = Id.CreateOf PlayerId
                let source: Response =
                    { Header = ResponseHeader.PlayerRegistered
                      Content = playerId |> PlayerId.ToBytes }

                let parsed =
                    source
                    |> Response.ToBytes
                    |> Response.TryParse
                    |> Expect.wantSome "Response should be parsable"

                let parsedPlayerId =
                    parsed.Content
                    |> PlayerId.TryParseBytes
                    |> Expect.wantSome "Response content should be parsable"

                Expect.equal source parsed "Responses should equal"
                Expect.equal playerId parsedPlayerId "Response contents should equal"
            )

            testCase "RoomCreated" (fun _ ->
                let sourceRoomId = RoomId.Create()
                let source: Response =
                    { Header = ResponseHeader.RoomCreated
                      Content = sourceRoomId |> RoomId.ToBytes }

                let parsed =
                    source
                    |> Response.ToBytes
                    |> Response.TryParse
                    |> Expect.wantSome "Response should be parsable"

                let parsedRoomId =
                    parsed.Content
                    |> RoomId.TryParseBytes
                    |> Expect.wantSome "Response content should be parsable"

                Expect.equal source parsed "Responses should equal"
                Expect.equal sourceRoomId parsedRoomId "Response contents should equal"
            )
        ]
    ]