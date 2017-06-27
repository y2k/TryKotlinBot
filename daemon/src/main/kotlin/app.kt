import java.security.MessageDigest.getInstance
import java.util.Base64.getDecoder
import java.util.Base64.getEncoder
import javax.script.ScriptEngineManager

fun main(args: Array<String>) {
    readLines().forEach(::handleLine)
}

private fun handleLine(line: String) {
    val pin = line.substring(0..3)
    val data = line.substring(pin.length)

    val code = data
        .let { getDecoder().decode(it) }
        .let { String(it) }
    try {
        println(engine.eval(code) ?: "")
    } catch (e: Exception) {
        println(e.message)
    }

    pin.let(::md5).let(::println)
}

private val engine by lazy { ScriptEngineManager().getEngineByExtension("kts")!! }

private fun readLines() = generateSequence { readLine() }

private fun md5(text: String) =
    text.toByteArray()
        .let { getInstance("MD5").digest(it) }
        .let { getEncoder().encodeToString(it) }