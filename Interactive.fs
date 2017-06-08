module Interactive

open System
open System.Diagnostics
open System.IO

let private shell command args =
    let psi = ProcessStartInfo()
    psi.FileName <- command
    psi.Arguments <- args
    psi.UseShellExecute <- false
    psi.RedirectStandardOutput <- true
    Process.Start(psi)

let compileAndExecute (script: string) =
    try
        let code = sprintf "fun main(args: Array<String>) {\n%O\n}" script
        let tmp = Path.Combine(Path.GetTempPath(), sprintf "tmp_%O.kt" (Guid.NewGuid()))
        File.WriteAllText(tmp, code)
        let jar = tmp + ".jar"

        shell "kotlinc" (tmp + " -include-runtime -d " + jar) 
        |> (fun x -> x.WaitForExit())

        let p = shell "java" ("-Xmx16m -jar " + jar)
        let success = p.WaitForExit(5000)
        let out = if success then p.StandardOutput.ReadToEnd()
                  else
                      p.Kill()
                      "Timeout exception"

        File.Delete tmp
        File.Delete jar
        out                  
    with
    | e -> (string e)

let execute (script: string) =
    try
        let tmp = Path.Combine(Path.GetTempPath(), sprintf "tmp_%O.kts" (Guid.NewGuid()))
        File.WriteAllText(tmp, script)

        let p = shell "kotlinc" ("-script " + tmp)
        let out = p.StandardOutput.ReadToEnd()
        
        File.Delete tmp
        out
    with
    | e -> (string e)