﻿module RX = Observable
module T  = Telegram
module I  = Interactive

module Domain =
    let format (message: string) =
        if message = "/start" then Error "TODO description"
        else if message.Length > 150 then Error "Code too long (limit is 150 charactes)"
        else Ok (message.Replace('”', '"').Replace('“', '"'))
    
    let formatOut (message: string) =
        if message.Length > 200 then 
            message.Substring(0, 200) + "...\n\n[RESULT TOO LONG (" + (string message.Length) + ")]"
        else message
        |> sprintf "```\n%O\n```"

[<EntryPoint>]
let main argv =
    T.listenForMessages argv.[0]
        |> RX.add (fun x -> 
            async {
                let pm = Domain.format x.text
                let! resp = match pm with
                            | Error e   -> async.Return e
                            | Ok script -> async {
                                               do! T.setProgress argv.[0] x.user
                                               let result = I.execute script
                                               return Domain.formatOut result
                                           }
                do! T.send argv.[0] x.user resp
            } |> Async.Start)

    printfn "Listening for updates..."
    System.Threading.Thread.Sleep(-1);
    0