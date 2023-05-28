module BehideServer.Helpers

open BehideServer.Types

type Response with
    static member inline internal create header content : Response = { Header = header; Content = content }
    static member inline internal createNoContent header : Response = { Header = header; Content = [||] }

type ResponseBuilder =
    internal
    | Ping
    | BadServerVersion
    | CorrectServerVersion
    | FailedToParseMsg

    | RoomCreated of RoomId
    | RoomNotCreated

    | RoomGet of Id
    | RoomNotGet

    | RoomDeleted
    | RoomNotDeleted

    static member internal ToResponse responseBuilder =
        match responseBuilder with
        | Ping -> Response.createNoContent ResponseHeader.Ping
        | BadServerVersion -> Response.createNoContent ResponseHeader.BadServerVersion
        | CorrectServerVersion -> Response.createNoContent ResponseHeader.CorrectServerVersion
        | FailedToParseMsg -> Response.createNoContent ResponseHeader.FailedToParseMsg

        | RoomCreated content -> Response.create ResponseHeader.RoomCreated (content |> RoomId.ToBytes)
        | RoomNotCreated -> Response.createNoContent ResponseHeader.RoomNotCreated

        | RoomGet content -> Response.create ResponseHeader.RoomGet (content |> Id.ToBytes)
        | RoomNotGet -> Response.createNoContent ResponseHeader.RoomNotGet

        | RoomDeleted -> Response.createNoContent ResponseHeader.RoomDeleted
        | RoomNotDeleted -> Response.createNoContent ResponseHeader.RoomNotDeleted

let tap f x = x |> f; x