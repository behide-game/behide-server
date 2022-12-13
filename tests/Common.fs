module BehideServer.Tests.Common

open BehideServer
open BehideServer.Types

open Expecto
open SuperSimpleTcp
open FSharpx.Control

let caughtResponses = BlockingQueueAgent<Response option> 1

let private tcpClient = new SimpleTcpClient(Common.getLocalEP 28000)
let connectTcp () =
    tcpClient.Events.DataReceived.Add (fun e ->
        e.Data
        |> Response.TryParse'
        |> caughtResponses.Add
    )

    tcpClient.Connect()

let getTcpClient () = tcpClient


let expectLastResponseHeader (expectedHeader: ResponseHeader) =
    async {
        let! responseOpt = caughtResponses.AsyncGet()
        let response = Expect.wantSome responseOpt "Response should be parsable"

        match response.Header = expectedHeader with
        | true -> ()
        | false -> failtestf "Response header should be %A but instead it's %A" expectedHeader response.Header
    }