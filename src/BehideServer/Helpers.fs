module BehideServer.Helpers

open BehideServer.Types

type Response with
    static member inline internal create header content : Response = { Header = header; Content = content }
    static member inline internal createNoContent header : Response = { Header = header; Content = [||] }

type ResponseBuilder =
    internal
    | Ping

    | PlayerRegistered of PlayerId
    | PlayerNotRegistered

    | RoomCreated of RoomId
    | RoomNotCreated
    | RoomDeleted
    | RoomNotDeleted

    | RoomFound of Id
    | RoomNotFound

    | BadServerVersion
    | FailedToParseMsg

    static member internal ToResponse responseBuilder =
        match responseBuilder with
        | Ping -> Response.createNoContent ResponseHeader.Ping

        | PlayerRegistered content -> Response.create ResponseHeader.PlayerRegistered (content |> PlayerId.ToBytes)
        | PlayerNotRegistered -> Response.createNoContent ResponseHeader.PlayerNotRegistered

        | RoomCreated content -> Response.create ResponseHeader.RoomCreated (content |> RoomId.ToBytes)
        | RoomDeleted -> Response.createNoContent ResponseHeader.RoomDeleted
        | RoomNotCreated -> Response.createNoContent ResponseHeader.RoomNotCreated
        | RoomNotDeleted -> Response.createNoContent ResponseHeader.RoomNotDeleted

        | RoomFound epicId -> Response.create ResponseHeader.RoomFound (epicId |> Id.ToBytes)
        | RoomNotFound -> Response.createNoContent ResponseHeader.RoomNotFound

        | BadServerVersion -> Response.createNoContent ResponseHeader.BadServerVersion
        | FailedToParseMsg -> Response.createNoContent ResponseHeader.FailedToParseMsg

let tap f x = x |> f; x