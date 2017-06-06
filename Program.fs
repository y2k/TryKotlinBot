module RX = Observable
module T  = Telegram
module I  = Interactive

module Domain =
    let format (message: string) =
        if message = "/start" then Error "TODO description"
        else if message.Length > 140 then Error "Code too long"
        else Ok (message.Replace('”', '"').Replace('“', '"'))
    
    let formatOut (message: string) =
        if message.Length > 100 then 
            message.Substring(0, 100) + "\n\n[RESULT TOO LONG (" + (string message.Length) + ")]"
        else message

[<EntryPoint>]
let main argv =
    T.listenForMessages argv.[0]
        |> RX.add (fun x -> 
            async {
                let pm = Domain.format x.text
                let! a = match pm with
                         | Error e   -> async.Return e
                         | Ok script -> async {
                                            let result = I.execute script
                                            return Domain.formatOut result
                                        }
                T.send argv.[0] x.user a |> ignore
            } |> Async.Start)

    printfn "Listening for updates..."
    System.Threading.Thread.Sleep(-1);
    0