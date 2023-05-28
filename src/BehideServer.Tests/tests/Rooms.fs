module BehideServer.Tests.Rooms

open Expecto
open BehideServer.Tests.Common
open BehideServer.Types
open FSharpx.Control

let tests =
    testList "Room" [
        testAsync "Create" {
            let tcp = TestTcpClient()
            let roomId = Id.NewGuid()

            do! (roomId)
                |> Msg.CreateRoom
                |> tcp.SendMessage ResponseHeader.RoomCreated
                |> Async.Ignore
        }

        testAsync "Get" {
            let tcp = TestTcpClient()
            let! roomId = tcp.CreateRoom()

            do! roomId
                |> Msg.GetRoom
                |> tcp.SendMessage ResponseHeader.RoomGet
                |> Async.map (Response.parseContent Id.TryParseBytes)
                |> Async.Ignore
        }

        testAsync "Get an nonexistent room" {
            let tcp = TestTcpClient()
            let roomId = RoomId.Create()

            do! roomId
                |> Msg.GetRoom
                |> tcp.SendMessage ResponseHeader.RoomNotGet
                |> Async.Ignore
        }

        testAsync "Delete" {
            let tcp = TestTcpClient()
            let! roomId = tcp.CreateRoom()

            do! roomId
                |> Msg.DeleteRoom
                |> tcp.SendMessage ResponseHeader.RoomDeleted
                |> Async.Ignore
        }

        testAsync "Delete an nonexistent room" {
            let tcp = TestTcpClient()
            let roomId = RoomId.Create()

            do! roomId
                |> Msg.DeleteRoom
                |> tcp.SendMessage ResponseHeader.RoomNotDeleted
                |> Async.Ignore
        }
    ]