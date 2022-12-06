module BehideServer.Helpers

open BehideServer.Types

type Response with
    static member inline internal create header content : Response = { Header = header; Content = content }
    static member inline internal create' header : Response = { Header = header; Content = [||] }

type ResponseBuilder =
    internal
    | Ping
    | PlayerRegistered of PlayerId
    | PlayerNotRegistered
    | RoomCreated of RoomId
    | RoomNotCreated
    | RoomDeleted
    | RoomNotDeleted
    | BadServerVersion
    | FailedToParseMsg

    static member internal ToResponse responseBuilder =
        match responseBuilder with
        | Ping -> Response.create' ResponseHeader.Ping
        | PlayerRegistered content -> Response.create ResponseHeader.PlayerRegistered (content |> PlayerId.ToBytes)
        | PlayerNotRegistered -> Response.create' ResponseHeader.PlayerNotRegistered
        | RoomCreated content -> Response.create ResponseHeader.RoomCreated (content |> RoomId.ToBytes)
        | RoomDeleted -> Response.create' ResponseHeader.RoomDeleted
        | RoomNotCreated -> Response.create' ResponseHeader.RoomNotCreated
        | RoomNotDeleted -> Response.create' ResponseHeader.RoomNotDeleted
        | BadServerVersion -> Response.create' ResponseHeader.BadServerVersion
        | FailedToParseMsg -> Response.create' ResponseHeader.FailedToParseMsg

let tap f x = x |> f; x