open System

module RX = Observable

module Domain =
    let validate (message: string) =
        if message = "/start" then Error "TODO description"
        else if message.Length > 140 then Error "Code to long"
        else Ok (message.Replace('”', '"').Replace('“', '"'))

[<EntryPoint>]
let main argv =
    Telegram.listenForMessages argv.[0]
        |> RX.add (fun x -> 
            async {
                let pm = Domain.validate x.text
                let! a = match pm with
                         | Error e   -> async.Return e
                         | Ok script -> async {
                                            let result = Interactive.execute script
                                            return result
                                        }
                Telegram.send argv.[0] x.user a |> ignore
            } |> Async.Start)

    printfn "Listening for updates..."
    Threading.Thread.Sleep(-1);
    0