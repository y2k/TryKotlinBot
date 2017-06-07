module Interactive

open System
open System.Diagnostics
open System.IO

let execute (script: string) =
    try
        let tmpDir = Path.GetTempPath()
        let tmp = Path.Combine(tmpDir, sprintf "tmp_%O.kts" (Guid.NewGuid()))

        File.WriteAllText(tmp, script)

        let psi = ProcessStartInfo()
        psi.FileName <- "kotlinc"
        psi.Arguments <- "-script " + tmp
        psi.UseShellExecute <- false
        psi.RedirectStandardOutput <- true

        let p = Process.Start(psi)
        let out = p.StandardOutput.ReadToEnd()
        
        File.Delete tmp

        out
    with
    | e -> (string e)