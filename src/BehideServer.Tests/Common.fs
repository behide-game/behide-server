module BehideServer.Tests.Common

open BehideServer
open BehideServer.Types

open Expecto
open SuperSimpleTcp
open FSharpx.Control

module Expect =
    let wantSome message x = Expect.wantSome x message

module Response =
    let expectHeader (expectedHeader: ResponseHeader) (response: Response) : Response =
        match response.Header = expectedHeader with
        | true -> response
        | false -> failtestf "Response header should be %A but instead it's %A" expectedHeader response.Header

    let parseContent parser (response: Response) =
        response.Content
        |> parser
        |> Expect.wantSome "Response content should be parsable"

type TestTcpClient() =
    let tcp = new SimpleTcpClient(Common.getLocalEP 28000)
    let caughtResponses = BlockingQueueAgent<Response option> 10

    do tcp.Events.DataReceived.Add (fun e ->
        e.Data
        |> Seq.toArray
        |> Response.TryParse
        |> caughtResponses.Add)
    do tcp.Connect()

    member _.SendMessage expectedHeader msg =
        msg |> Msg.ToBytes |> tcp.Send

        caughtResponses.AsyncGet()
        |> Async.map (Expect.wantSome "Response should be parsable")
        |> Async.map (Response.expectHeader expectedHeader)

    member this.CreateRoom () =
        Id.NewGuid() // Fake epic id
        |> Msg.CreateRoom
        |> this.SendMessage ResponseHeader.RoomCreated
        |> Async.map (Response.parseContent RoomId.TryParseBytes)

let utf8String = System.IO.File.ReadAllText (__SOURCE_DIRECTORY__ + "/UTF8-text.txt")