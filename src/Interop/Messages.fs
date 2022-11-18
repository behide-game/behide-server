namespace BehideServer.Types

open System

[<RequireQualifiedAccess>]
type private MsgHeader =
    | Ping = 1uy
    | RegisterPlayer = 2uy
    | RegisterRoom = 3uy

[<RequireQualifiedAccess>]
type Msg =
    | Ping
    /// Send the server version (string) and the player's username (string)
    | RegisterPlayer of string * string
    /// Send the PlayerId and the EpicId
    | RegisterRoom of PlayerId * Id

    static member ToBytes msg =
        let header =
            match msg with
            | Ping _ -> MsgHeader.Ping
            | RegisterPlayer _ -> MsgHeader.RegisterPlayer
            | RegisterRoom _ -> MsgHeader.RegisterRoom
            |> byte

        let content =
            match msg with
            | Ping -> [||]
            | RegisterPlayer (x, y) ->
                Array.concat [
                    (x |> Text.Encoding.ASCII.GetBytes)
                    [| 255uy; 255uy; 255uy; 255uy |]
                    (y |> Text.Encoding.UTF8.GetBytes)
                ]
            | RegisterRoom (x, y) -> Array.append (x |> PlayerId.ToBytes) (y |> Id.ToBytes)

        Array.append [| header |] content

    member this.ToBytes() = this |> Msg.ToBytes

    static member TryParse(bytes: #seq<byte>) =
        let header = bytes |> Seq.head
        let content = bytes |> Seq.tail |> Seq.toArray

        match header |> LanguagePrimitives.EnumOfValue with
        | MsgHeader.Ping -> Some Ping
        | MsgHeader.RegisterPlayer ->
            let delimiterStartIndex =
                content
                |> Array.indexed
                |> Array.fold
                    (fun (startIndex, chain) (index, byte) ->
                        if chain = 4 then
                            startIndex, chain
                        else
                            if byte = 255uy && startIndex = -1 then
                                index, 1
                            elif byte = 255uy && startIndex <> -1 then
                                startIndex, chain + 1
                            else
                                -1, 0
                    )
                    (-1, 0)
                |> fst

            let contentPart1, contentPart2 =
                content
                |> Array.splitAt delimiterStartIndex
                |> fun (part1, part2) ->
                    part1, part2 |> Array.splitAt 4 |> snd

            RegisterPlayer
                (contentPart1 |> Text.Encoding.ASCII.GetString,
                 contentPart2 |> Text.Encoding.UTF8.GetString)
            |> Some
        | MsgHeader.RegisterRoom ->
            let contentPart1, contentPart2 = content |> Array.splitAt 16

            contentPart1
            |> Id.TryParseBytes
            |> Option.map PlayerId
            |> Option.bind (fun playerId ->
                contentPart2
                |> Id.TryParseBytes
                |> Option.map (fun epicId -> playerId, epicId)
                |> Option.map RegisterRoom)
        | _ -> None
