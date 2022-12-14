module BehideServer.Tests.Response

open Expecto
open BehideServer
open BehideServer.Tests.Common
open BehideServer.Types
open FSharpx.Control

[<Tests>]
let tests =
    testList "Server response" [
        testAsync "Server version checking" {
            let tcp = TestTcpClient()

            (Version.GetVersion() + " fake version", "test user")
            |> Msg.RegisterPlayer
            |> tcp.SendMessage

            do!
                tcp.AwaitResponse()
                |> Async.map (Response.expectHeader ResponseHeader.BadServerVersion)
                |> Async.Ignore
        }

        testAsync "Register player" {
            let tcp = TestTcpClient()
            do! tcp.RegisterPlayer() |> Async.Ignore
        }

        testAsync "Create room" {
            let tcp = TestTcpClient()
            let! playerId = tcp.RegisterPlayer()
            let roomId = Id.NewGuid()

            (playerId, roomId)
            |> Msg.CreateRoom
            |> tcp.SendMessage

            do!
                tcp.AwaitResponse()
                |> Async.map (Response.expectHeader ResponseHeader.RoomCreated)
                |> Async.Ignore
        }
    ]