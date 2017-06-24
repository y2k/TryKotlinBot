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


let private readToMark (reader: StreamReader) (endMark: string) = async {
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

let private createProcess () =
    let psi = ProcessStartInfo()
    psi.WorkingDirectory <- Path.Combine(Directory.GetCurrentDirectory(), "bin/kotlinservice/bin")
    psi.FileName <- "bash"
    psi.Arguments <- "kotlindaemon"
    psi.UseShellExecute <- false
    psi.RedirectStandardInput <- true
    psi.RedirectStandardOutput <- true
    Process.Start(psi)

type private Msg = string * string * int * AsyncReplyChannel<string option>

let private callWithTimeout timeout operation = async {
    let! child = Async.StartChild (operation, timeout) 
    try 
        let! x = child 
        return Some x
    with :? TimeoutException -> return None 
}

let private agent = MailboxProcessor<Msg>.Start (fun inbox ->
    let mutable daemon = createProcess ()
    let rec messageLoop() = async {
        let! (msg, endMark, timeout, reply) = inbox.Receive()        

        do! daemon.StandardInput.WriteLineAsync(msg) |> Async.AwaitTask

        let! response = readToMark daemon.StandardOutput endMark
                        |> callWithTimeout timeout

        match response with
        | None -> daemon.Kill ()
                  daemon <- createProcess ()
        | _ -> ignore()

        response |> reply.Reply
    }
    messageLoop())

let callKotlinService (script: string) (timeout: int) = async {
    let pin = generatePincode()
    let endMark = computeMd5 pin
    let msg = pin + (encodeBase64 script)
    let! result = agent.PostAndAsyncReply(fun x -> msg, endMark, timeout, x)
    return result |> Option.defaultValue "Error: timeout"
}