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


let fakeRoom: Room =
    { Id = RoomId.Create()
      EpicId = Id.NewGuid() }

[<Tests>]
let tests =
    testList "Parsing" [
        parseTest "Guid" (Id.NewGuid()) Id.ToBytes Id.TryParseBytes
        parseTest "RoomId" (RoomId.Create()) RoomId.ToBytes RoomId.TryParseBytes
        testCase "Invalid msg parsing" (fun _ -> Expect.isNone ([||] |> Msg.TryParse) "Parsing an invalid msg should return None")

        testList "Msg" (
            [ "Ping", Msg.Ping
              "FailedToParse", Msg.FailedToParse
              "CheckServerVersion", Msg.CheckServerVersion (BehideServer.Version.GetVersion())
              "CreateRoom", Msg.CreateRoom (Id.NewGuid())
              "GetRoom", Msg.GetRoom (RoomId.Create())
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
            [ "Ping", (ResponseHeader.Ping, Array.empty)
              "FailedToParseMsg", (ResponseHeader.FailedToParseMsg, Array.empty)

              "BadServerVersion", (ResponseHeader.BadServerVersion, Array.empty)
              "CorrectServerVersion", (ResponseHeader.CorrectServerVersion, Array.empty)

              "RoomCreated", (ResponseHeader.RoomCreated, RoomId.Create() |> RoomId.ToBytes)
              "RoomNotCreated", (ResponseHeader.RoomNotCreated, Array.empty)

              "RoomGet", (ResponseHeader.RoomGet, Id.NewGuid() |> Id.ToBytes)
              "RoomNotGet", (ResponseHeader.RoomNotGet, Array.empty)

              "RoomDeleted", (ResponseHeader.RoomDeleted, Array.empty)
              "RoomNotDeleted", (ResponseHeader.RoomNotDeleted, Array.empty) ]
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
            testCase "RoomCreated" (fun _ ->
                let content = RoomId.Create()

                let response: Response =
                    { Header = ResponseHeader.RoomCreated
                      Content = content |> RoomId.ToBytes }

                let parsedResponse =
                    response
                    |> Response.ToBytes
                    |> Response.TryParse
                    |> Expect.wantSome "Response should be parsable"

                let parsedContent =
                    parsedResponse.Content
                    |> RoomId.TryParseBytes
                    |> Expect.wantSome "Response content should be parsable"

                Expect.equal response parsedResponse "Responses should equal"
                Expect.equal content parsedContent "Response contents should equal"
            )
        ]
    ]