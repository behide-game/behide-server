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

            do! (Version.GetVersion() + " fake version", "test user")
                |> Msg.RegisterPlayer
                |> tcp.SendMessage ResponseHeader.BadServerVersion
                |> Async.Ignore
        }

        testAsync "Register player" {
            let tcp = TestTcpClient()
            do! tcp.RegisterPlayer() |> Async.Ignore
        }

        testAsync "Register player - already registered" {
            let tcp = TestTcpClient()
            do! tcp.RegisterPlayer() |> Async.Ignore
            do! (Version.GetVersion(), "test user")
                |> Msg.RegisterPlayer
                |> tcp.SendMessage ResponseHeader.PlayerNotRegistered
                |> Async.Ignore
        }

        Tests.Rooms.tests
    ]