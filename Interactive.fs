module Interactive

open System
open System.Diagnostics
open System.IO

let private encodeBase64 (text: string) =
    text |> Text.Encoding.UTF8.GetBytes |> Convert.ToBase64String

let private computeMd5 (text: string) =
    text 
    |> Text.Encoding.UTF8.GetBytes 
    |> (Security.Cryptography.MD5.Create()).ComputeHash
    |> Convert.ToBase64String

let private generatePincode =
    let rand = Random()
    fun () -> lock rand (fun () -> rand.Next(1000, 9999).ToString())

let private readWhile (reader: StreamReader) (endMark: string) = async {
    use sw = new StringWriter()
    let rec readWhileRec () = async {
        let! line = reader.ReadLineAsync() |> Async.AwaitTask
        if line <> endMark then 
            sw.WriteLine line
            do! readWhileRec ()
    }
    do! readWhileRec ()
    return sw.ToString()
}

type private Msg = string * string * AsyncReplyChannel<string>
let private agent = MailboxProcessor<Msg>.Start (fun inbox ->
    let psi = ProcessStartInfo()
    psi.WorkingDirectory <- Path.Combine(Directory.GetCurrentDirectory(), "bin/kotlinservice/bin")
    psi.FileName <- "bash"
    psi.Arguments <- "kotlindaemon"
    psi.UseShellExecute <- false
    psi.RedirectStandardInput <- true
    psi.RedirectStandardOutput <- true
    let kotlinService = Process.Start(psi)

    let rec messageLoop() = async {
        let! (msg, endMark, reply) = inbox.Receive()        

        do! kotlinService.StandardInput.WriteLineAsync(msg) |> Async.AwaitTask
        let! result = readWhile kotlinService.StandardOutput endMark
        reply.Reply result

        return! messageLoop()  
    }
    messageLoop())

let callKotlinService (script: string) =
    async {
        let pin = generatePincode()
        let endMark = computeMd5 pin
        let msg = pin + (encodeBase64 script)
        return! agent.PostAndAsyncReply(fun x -> (msg, endMark, x))
    }