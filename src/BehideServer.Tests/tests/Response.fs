module BehideServer.Tests.Response

open Expecto
open BehideServer
open BehideServer.Tests.Common
open BehideServer.Types
open FSharpx.Control

[<Tests>]
let tests =
    testList "Server response" [
        testAsync "Ping" {
            let tcp = TestTcpClient()

            do! Msg.Ping
                |> tcp.SendMessage ResponseHeader.Ping
                |> Async.Ignore
        }

        testList "Server version checking" [
            testAsync "Bad version" {
                let tcp = TestTcpClient()

                do! (Version.GetVersion() + " fake version")
                    |> Msg.CheckServerVersion
                    |> tcp.SendMessage ResponseHeader.BadServerVersion
                    |> Async.Ignore
            }

            testAsync "Correct version" {
                let tcp = TestTcpClient()

                do! Version.GetVersion()
                    |> Msg.CheckServerVersion
                    |> tcp.SendMessage ResponseHeader.CorrectServerVersion
                    |> Async.Ignore
            }
        ]

        Tests.Rooms.tests
    ]