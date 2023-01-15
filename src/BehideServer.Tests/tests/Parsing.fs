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


[<Tests>]
let tests =
    testList "Parsing" [
        parseTest "Guid" (Id.NewGuid()) Id.ToBytes Id.TryParseBytes
        parseTest "RoomId" (RoomId.Create()) RoomId.ToBytes RoomId.TryParseBytes
        parseTest "PlayerId" (Id.CreateOf PlayerId) PlayerId.ToBytes PlayerId.TryParseBytes
        testCase "Msg.FailedToParse" (fun _ -> Expect.isNone ([||] |> Msg.TryParse) "Parsing an unexpected msg should return None")

        testList "Msg" (
            [ "Ping", Msg.Ping
              "RegisterPlayer", Msg.RegisterPlayer ("fake-version", utf8String)
              "CreateRoom", Msg.CreateRoom (Id.CreateOf PlayerId, Id.NewGuid())
              "DeleteRoom", Msg.DeleteRoom (RoomId.Create())
              "GetRoom", Msg.GetRoom (RoomId.Create()) ]
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

              "PlayerRegistered", { Response.Header = ResponseHeader.PlayerRegistered; Response.Content = Id.CreateOf PlayerId |> PlayerId.ToBytes }
              "PlayerNotRegistered", { Response.Header = ResponseHeader.PlayerNotRegistered; Response.Content = Array.empty }

              "RoomCreated", { Response.Header = ResponseHeader.RoomCreated; Response.Content = RoomId.Create() |> RoomId.ToBytes }
              "RoomNotCreated", { Response.Header = ResponseHeader.RoomNotCreated; Response.Content = Array.empty }
              "RoomDeleted", { Response.Header = ResponseHeader.RoomDeleted; Response.Content = Array.empty }
              "RoomNotDeleted", { Response.Header = ResponseHeader.RoomNotDeleted; Response.Content = Array.empty }

              "RoomFound", { Response.Header = ResponseHeader.RoomFound; Response.Content = Id.NewGuid() |> Id.ToBytes }
              "RoomNotFound", { Response.Header = ResponseHeader.RoomNotFound; Response.Content = Array.empty } ]
            |> testFixture (fun response _ ->
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
            parseResponseWithContentTest ResponseHeader.RoomFound (Id.NewGuid()) Id.ToBytes Id.TryParseBytes
        ]
    ]