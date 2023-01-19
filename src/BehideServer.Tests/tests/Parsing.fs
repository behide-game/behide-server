module BehideServer.Tests.Parsing

open Expecto
open BehideServer.Tests.Common
open BehideServer.Types

let parseTest name (source: 'a) (toBytes: 'a -> byte []) (parser: byte [] -> 'a option) =
    testCase name (fun _ ->
        source
        |> toBytes
        |> parser
        |> Expect.wantSome "Should be parsable"
        |> fun parsed -> Expect.equal source parsed "Source value and parsed value should equals"
    )

let parseResponseWithContentTest responseHeader content contentToBytes contentParser =
    testCase (responseHeader |> string) (fun _ ->
        let response: Response =
            { Header = responseHeader
              Content = content |> contentToBytes }

        let parsedResponse =
            response
            |> Response.ToBytes
            |> Response.TryParse
            |> Expect.wantSome "Response should be parsable"

        let parsedContent =
            parsedResponse.Content
            |> contentParser
            |> Expect.wantSome "Response content should be parsable"

        Expect.equal response parsedResponse "Responses should equal"
        Expect.equal content parsedContent "Response contents should equal"
    )

let fakeRoom: Room =
    { Id = RoomId.Create()
      EpicId = Id.NewGuid()
      CurrentRound = 0
      MaxPlayers = 3
      Owner = Id.CreateOf PlayerId
      Players = [||] }

[<Tests>]
let tests =
    testList "Parsing" [
        parseTest "Guid" (Id.NewGuid()) Id.ToBytes Id.TryParseBytes
        parseTest "RoomId" (RoomId.Create()) RoomId.ToBytes RoomId.TryParseBytes
        parseTest "PlayerId" (Id.CreateOf PlayerId) PlayerId.ToBytes PlayerId.TryParseBytes
        parseTest "Room" fakeRoom Room.ToBytes Room.TryParse

        testCase "Msg.FailedToParse" (fun _ -> Expect.isNone ([||] |> Msg.TryParse) "Parsing an unexpected msg should return None")
        testList "Msg" (
            [ "Ping", Msg.Ping
              "RegisterPlayer", Msg.RegisterPlayer ("fake-version", utf8String)
              "CreateRoom", Msg.CreateRoom (Id.CreateOf PlayerId, Id.NewGuid())
              "DeleteRoom", Msg.DeleteRoom (RoomId.Create())
              "JoinRoom", Msg.JoinRoom (RoomId.Create())
              "LeaveRoom", Msg.LeaveRoom ]
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
            [ "Ping", (ResponseHeader.Ping, Array.empty)
              "BadServerVersion", (ResponseHeader.BadServerVersion, Array.empty)
              "FailedToParseMsg", (ResponseHeader.FailedToParseMsg, Array.empty)

              "PlayerRegistered", (ResponseHeader.PlayerRegistered, Id.CreateOf PlayerId |> PlayerId.ToBytes)
              "PlayerNotRegistered", (ResponseHeader.PlayerNotRegistered, Array.empty)

              "RoomCreated", (ResponseHeader.RoomCreated, RoomId.Create() |> RoomId.ToBytes)
              "RoomNotCreated", (ResponseHeader.RoomNotCreated, Array.empty)
              "RoomDeleted", (ResponseHeader.RoomDeleted, Array.empty)
              "RoomNotDeleted", (ResponseHeader.RoomNotDeleted, Array.empty)

              "RoomJoined", (ResponseHeader.RoomJoined, fakeRoom |> Room.ToBytes)
              "RoomNotJoined", (ResponseHeader.RoomNotJoined, Array.empty)
              "RoomLeaved", (ResponseHeader.RoomLeaved, Id.CreateOf PlayerId |> PlayerId.ToBytes)
              "RoomNotLeaved", (ResponseHeader.RoomNotLeaved, Array.empty) ]
            |> testFixture (fun (header, content) _ ->
                let response: Response = { Header = header; Content = content }
                let parsed =
                    response
                    |> Response.ToBytes
                    |> Response.TryParse
                    |> Expect.wantSome "Response should be parsable"

                Expect.equal response parsed "Responses should equal"
            )
            |> Seq.toList
        )

        testList "Response with content" [
            parseResponseWithContentTest ResponseHeader.PlayerRegistered (Id.CreateOf PlayerId) PlayerId.ToBytes PlayerId.TryParseBytes
            parseResponseWithContentTest ResponseHeader.RoomCreated (RoomId.Create()) RoomId.ToBytes RoomId.TryParseBytes
            parseResponseWithContentTest ResponseHeader.RoomJoined fakeRoom Room.ToBytes Room.TryParse
            parseResponseWithContentTest ResponseHeader.RoomLeaved (Id.CreateOf PlayerId) PlayerId.ToBytes PlayerId.TryParseBytes
        ]
    ]