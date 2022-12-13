module BehideServer.Tests.Response

open Expecto
open BehideServer
open BehideServer.Tests.Common
open BehideServer.Types

[<Tests>]
let tests =
    testList "Server response" [
        testAsync "Server version checking" {
            let tcp = getTcpClient()

            (Version.GetVersion() + " fake version", "test user")
            |> Msg.RegisterPlayer
            |> Msg.ToBytes
            |> tcp.Send

            do! expectLastResponseHeader ResponseHeader.BadServerVersion
        }

        testAsync "Register player" {
            let tcp = getTcpClient()

            (Version.GetVersion(), "test user")
            |> Msg.RegisterPlayer
            |> Msg.ToBytes
            |> tcp.Send

            do! expectLastResponseHeader ResponseHeader.PlayerRegistered
        }
    ]
    |> testSequenced