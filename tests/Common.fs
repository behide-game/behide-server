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
        | false -> failtestf "Response header should be %A but instead it's %A." expectedHeader response.Header

    let content (response: Response) = response.Content


type TestTcpClient() =
    let tcp = new SimpleTcpClient(Common.getLocalEP 28000)
    let caughtResponses = BlockingQueueAgent<Response option> 10

    do tcp.Events.DataReceived.Add (fun e ->
        e.Data
        |> Response.TryParse'
        |> caughtResponses.Add)
    do tcp.Connect()

    member _.SendMessage msg = msg |> Msg.ToBytes |> tcp.Send
    member _.AwaitResponse() =
        caughtResponses.AsyncGet()
        |> Async.map (Expect.wantSome "Response should be parsable.")

    member this.RegisterPlayer() =
        (Version.GetVersion(), "test user")
        |> Msg.RegisterPlayer
        |> this.SendMessage

        this.AwaitResponse()
        |> Async.map (Response.expectHeader ResponseHeader.PlayerRegistered)
        |> Async.map Response.content
        |> Async.map PlayerId.TryParseBytes
        |> Async.map (Expect.wantSome "PlayerId should be parsable.")