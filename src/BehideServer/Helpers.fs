module BehideServer.Helpers

open BehideServer.Types

type Response with
    static member inline internal create header content : Response = { Header = header; Content = content }
    static member inline internal createNoContent header : Response = { Header = header; Content = [||] }

type ResponseBuilder =
    internal
    | Ping
    | BadServerVersion
    | FailedToParseMsg

    | PlayerRegistered of PlayerId
    | PlayerNotRegistered

    | RoomCreated of RoomId
    | RoomDeleted
    | RoomNotCreated
    | RoomNotDeleted

    | RoomJoined of Room
    | RoomLeaved of PlayerId
    | RoomNotJoined
    | RoomNotLeaved

    static member internal ToResponse responseBuilder =
        match responseBuilder with
        | Ping -> Response.createNoContent ResponseHeader.Ping
        | BadServerVersion -> Response.createNoContent ResponseHeader.BadServerVersion
        | FailedToParseMsg -> Response.createNoContent ResponseHeader.FailedToParseMsg

        | PlayerRegistered content -> Response.create ResponseHeader.PlayerRegistered (content |> PlayerId.ToBytes)
        | PlayerNotRegistered -> Response.createNoContent ResponseHeader.PlayerNotRegistered

        | RoomCreated content -> Response.create ResponseHeader.RoomCreated (content |> RoomId.ToBytes)
        | RoomDeleted -> Response.createNoContent ResponseHeader.RoomDeleted
        | RoomNotCreated -> Response.createNoContent ResponseHeader.RoomNotCreated
        | RoomNotDeleted -> Response.createNoContent ResponseHeader.RoomNotDeleted

        | RoomJoined room -> Response.create ResponseHeader.RoomJoined (room |> Room.ToBytes)
        | RoomLeaved playerId -> Response.create ResponseHeader.RoomLeaved (playerId |> PlayerId.ToBytes)
        | RoomNotJoined -> Response.createNoContent ResponseHeader.RoomNotJoined
        | RoomNotLeaved -> Response.createNoContent ResponseHeader.RoomNotLeaved

let tap f x = x |> f; x