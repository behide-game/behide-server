module BehideServer.Tests.Rooms

open Expecto
open BehideServer.Tests.Common
open BehideServer.Types
open FSharpx.Control

let tests =
    testList "Room" [
        testAsync "Create" {
            let tcp = TestTcpClient()
            let! playerId = tcp.RegisterPlayer()
            let roomId = Id.NewGuid()

            do! (playerId, roomId)
                |> Msg.CreateRoom
                |> tcp.SendMessage ResponseHeader.RoomCreated
                |> Async.Ignore
        }

        testAsync "Delete" {
            let tcp = TestTcpClient()
            let! playerId = tcp.RegisterPlayer()
            let! roomId = tcp.CreateRoom playerId

            // Delete room
            do! roomId
                |> Msg.DeleteRoom
                |> tcp.SendMessage ResponseHeader.RoomDeleted
                |> Async.Ignore
        }

        testList "Join" [
            testAsync "Join room" {
                // Create room
                let ownerTcp = TestTcpClient()
                let! ownerId = ownerTcp.RegisterPlayer()
                let! roomId = ownerTcp.CreateRoom ownerId

                // Join room
                let playerTcp = TestTcpClient()
                let! _ = playerTcp.RegisterPlayer()

                do! roomId
                    |> Msg.JoinRoom
                    |> playerTcp.SendMessage ResponseHeader.RoomJoined
                    |> Async.map (Response.parseContent Room.TryParse)
                    |> Async.Ignore
            }

            testAsync "Owner try to join" {
                // Create room
                let ownerTcp = TestTcpClient()
                let! ownerId = ownerTcp.RegisterPlayer()
                let! roomId = ownerTcp.CreateRoom ownerId

                do! roomId
                    |> Msg.JoinRoom
                    |> ownerTcp.SendMessage ResponseHeader.RoomNotJoined
                    |> Async.Ignore
            }

            testAsync "Unregistered player" {
                // Create room
                let ownerTcp = TestTcpClient()
                let! ownerId = ownerTcp.RegisterPlayer()
                let! roomId = ownerTcp.CreateRoom ownerId

                // Join room
                let playerTcp = TestTcpClient()

                do! roomId
                    |> Msg.JoinRoom
                    |> playerTcp.SendMessage ResponseHeader.RoomNotJoined
                    |> Async.Ignore
            }

            testAsync "Non-existant room" {
                // Join room
                let playerTcp = TestTcpClient()
                let! _ = playerTcp.RegisterPlayer()

                do! RoomId.Create()
                    |> Msg.JoinRoom
                    |> playerTcp.SendMessage ResponseHeader.RoomNotJoined
                    |> Async.Ignore
            }

            testAsync "Already joined room" {
                // Create room
                let ownerTcp = TestTcpClient()
                let! ownerId = ownerTcp.RegisterPlayer()
                let! roomId = ownerTcp.CreateRoom ownerId

                let playerTcp = TestTcpClient()
                let! _ = playerTcp.RegisterPlayer()

                // 1st join
                do! roomId
                    |> Msg.JoinRoom
                    |> playerTcp.SendMessage ResponseHeader.RoomJoined
                    |> Async.map (Response.parseContent Room.TryParse)
                    |> Async.Ignore

                // 2nd join
                do! roomId
                    |> Msg.JoinRoom
                    |> playerTcp.SendMessage ResponseHeader.RoomNotJoined
                    |> Async.Ignore
            }
        ]

        testList "Leave" [
            testAsync "Leave room" {
                // Create room
                let ownerTcp = TestTcpClient()
                let! ownerId = ownerTcp.RegisterPlayer()
                let! roomId = ownerTcp.CreateRoom ownerId

                // Join room
                let playerTcp = TestTcpClient()
                let! _playerId = playerTcp.RegisterPlayer()

                do! roomId
                    |> Msg.JoinRoom
                    |> playerTcp.SendMessage ResponseHeader.RoomJoined
                    |> Async.Ignore

                // Leave room
                do! Msg.LeaveRoom
                    |> playerTcp.SendMessage ResponseHeader.RoomLeaved
                    |> Async.Ignore
            }

            testAsync "Player didn't join" {
                // Create room
                let ownerTcp = TestTcpClient()
                let! ownerId = ownerTcp.RegisterPlayer()
                let! _roomId = ownerTcp.CreateRoom ownerId

                // Leave room
                let playerTcp = TestTcpClient()
                let! _playerId = playerTcp.RegisterPlayer()

                do! Msg.LeaveRoom
                    |> playerTcp.SendMessage ResponseHeader.RoomNotLeaved
                    |> Async.Ignore
            }
        ]
    ]