namespace BehideServer.Types

open System

type Id = Guid
module Id =
    let CreateOf (x: Id -> _) = Guid.NewGuid() |> x

    let TryParse (str: string) : Id option =
        match Guid.TryParse str with
        | true, id -> Some id
        | false, _ -> None

    let TryParseBytes (bytes: byte []) : Id option =
        try
            new Guid(bytes) |> Some
        with
        | _ -> None

    let ToBytes (id: Id) = id.ToByteArray()

type PlayerId = PlayerId of Id
module PlayerId =
    let ToBytes (PlayerId playerId) = playerId |> Id.ToBytes
    let TryParseBytes = Id.TryParseBytes >> Option.map PlayerId

type Player = { Id: PlayerId; Username: string }


type RoomId =
    | RoomId of string

    static member private possibilities =
        [| 'A'; 'B'; 'C'; 'D'; 'E'; 'F'; 'G'; 'H'; 'I'; 'J'; 'K'; 'L';
           'M'; 'N'; 'O'; 'P'; 'Q'; 'R'; 'S'; 'T'; 'U'; 'V'; 'W'; 'X';
           'Y'; 'Z'; '0'; '1'; '2'; '3'; '4'; '5'; '6'; '7'; '8'; '9'; |]

    static member Create() : RoomId =
        [| 0..3 |]
        |> Array.map (fun _ ->
            let charIndex =
                Random().NextDouble()
                * (float <| RoomId.possibilities.Length - 1)
                |> Math.Round
                |> int

            RoomId.possibilities |> Array.item charIndex)
        |> String
        |> RoomId

    static member ToBytes(RoomId id) = id |> Text.Encoding.ASCII.GetBytes

    static member TryParse(str: string) =
        let x =
            str.ToCharArray()
            |> Array.tryFind (fun char -> Array.contains char RoomId.possibilities)
            |> Option.isNone

        if str.Length = 4 && x then
            str |> RoomId |> Some
        else
            None

    static member TryParseBytes(bytes: byte []) =
        bytes
        |> Text.Encoding.ASCII.GetString
        |> RoomId.TryParse

type Room =
    { Id: RoomId
      EpicId: Id
      Creator: PlayerId
      Players: Player [] }
