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

        testAsync "Create room" {
            let tcp = TestTcpClient()
            let! playerId = tcp.RegisterPlayer()
            let roomId = Id.NewGuid()

            do! (playerId, roomId)
                |> Msg.CreateRoom
                |> tcp.SendMessage ResponseHeader.RoomCreated
                |> Async.Ignore
        }

        testAsync "Delete room" {
            let tcp = TestTcpClient()
            let! playerId = tcp.RegisterPlayer()
            let! roomId = tcp.CreateRoom playerId

            // Delete room
            do! roomId
                |> Msg.DeleteRoom
                |> tcp.SendMessage ResponseHeader.RoomDeleted
                |> Async.Ignore
        }

        testList "Join room" [
            testAsync "Join room" {
                // Create room
                let ownerTcp = TestTcpClient()
                let! ownerId = ownerTcp.RegisterPlayer()

                let! roomId =
                    (ownerId, Id.NewGuid())
                    |> Msg.CreateRoom
                    |> ownerTcp.SendMessage ResponseHeader.RoomCreated
                    |> Async.map (Response.parseContent RoomId.TryParseBytes)

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

                let! roomId =
                    (ownerId, Id.NewGuid())
                    |> Msg.CreateRoom
                    |> ownerTcp.SendMessage ResponseHeader.RoomCreated
                    |> Async.map (Response.parseContent RoomId.TryParseBytes)

                do! roomId
                    |> Msg.JoinRoom
                    |> ownerTcp.SendMessage ResponseHeader.RoomNotJoined
                    |> Async.Ignore
            }

            testAsync "Unregistered player" {
                // Create room
                let ownerTcp = TestTcpClient()
                let! ownerId = ownerTcp.RegisterPlayer()

                let! roomId =
                    (ownerId, Id.NewGuid())
                    |> Msg.CreateRoom
                    |> ownerTcp.SendMessage ResponseHeader.RoomCreated
                    |> Async.map (Response.parseContent RoomId.TryParseBytes)

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

                let! roomId =
                    (ownerId, Id.NewGuid())
                    |> Msg.CreateRoom
                    |> ownerTcp.SendMessage ResponseHeader.RoomCreated
                    |> Async.map (Response.parseContent RoomId.TryParseBytes)

                // Join room
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

        testList "Leave room" [
            testAsync "Leave room" {
                // Create room
                let ownerTcp = TestTcpClient()
                let! ownerId = ownerTcp.RegisterPlayer()

                let! roomId =
                    (ownerId, Id.NewGuid())
                    |> Msg.CreateRoom
                    |> ownerTcp.SendMessage ResponseHeader.RoomCreated
                    |> Async.map (Response.parseContent RoomId.TryParseBytes)

                // Join room
                let playerTcp = TestTcpClient()
                let! playerId = playerTcp.RegisterPlayer()

                do! roomId
                    |> Msg.JoinRoom
                    |> playerTcp.SendMessage ResponseHeader.RoomJoined
                    |> Async.map (Response.parseContent Room.TryParse)
                    |> Async.Ignore

                // Leave room
                let! leavedPlayerId =
                    Msg.LeaveRoom
                    |> playerTcp.SendMessage ResponseHeader.RoomLeaved
                    |> Async.map (Response.parseContent PlayerId.TryParseBytes)

                Expect.equal playerId leavedPlayerId "PlayerIds should equal"
            }

            testAsync "Player didn't join" {
                // Create room
                let ownerTcp = TestTcpClient()
                let! ownerId = ownerTcp.RegisterPlayer()

                let! _roomId =
                    (ownerId, Id.NewGuid())
                    |> Msg.CreateRoom
                    |> ownerTcp.SendMessage ResponseHeader.RoomCreated
                    |> Async.map (Response.parseContent RoomId.TryParseBytes)

                // Leave room
                let playerTcp = TestTcpClient()
                let! _playerId = playerTcp.RegisterPlayer()

                do! Msg.LeaveRoom
                    |> playerTcp.SendMessage ResponseHeader.RoomNotLeaved
                    |> Async.Ignore
            }
        ]
    ]