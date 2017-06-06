module Telegram

open System
open Telegram.Bot
module RX = Observable

type Message = { text: string; user: string }

let listenForMessages (token: string) =
    let bot = TelegramBotClient(token)
    let result = bot.OnUpdate 
                 |> RX.map (fun x -> x.Update)
                 |> RX.map (fun x -> { text = x.Message.Text; user = string x.Message.From.Id })
    bot.StartReceiving()
    result

let send (token: string) (user: string) message =
    try
        let bot = TelegramBotClient(token)
        bot.SendTextMessageAsync(user, message, parseMode = Types.Enums.ParseMode.Default).Result |> ignore
    with
    | e -> printfn "Log error: %O" e