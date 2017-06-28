import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test
import java.util.*
import javax.script.ScriptEngineManager
import javax.script.ScriptException
import javax.script.SimpleScriptContext
import kotlin.system.measureTimeMillis

/**
 * Created by y2k on 28/06/2017.
 **/
class ScriptEngineManagerTests {

    @Test fun `ignore errors`() {
        val engine = ScriptEngineManager().getEngineByExtension("kts")!!
        try {
            engine.eval("data class A(x: Int)")
        } catch (e: Exception) {
        }

        engine.context = SimpleScriptContext()

        engine.eval("data class A(val x: Int)")
    }

    @Test fun `ignore throw`() {
        val engine = ScriptEngineManager().getEngineByExtension("kts")!!
        try {
            engine.eval("throw Exception()")
        } catch (e: Exception) {
        }

        engine.context = SimpleScriptContext()

        engine.eval("2 + 2")
    }

    @Test fun `reset state`() {
        val engine = ScriptEngineManager().getEngineByExtension("kts")!!

        engine.eval("val x = 2")
        assertEquals(2, engine.eval("x"))
        assertEquals(2, engine.eval("x"))

        engine.context = SimpleScriptContext()

        try {
            assertEquals(0, engine.eval("x"))
        } catch (e: ScriptException) {
            assertTrue("" + e, e.message!!.contains("unresolved reference: x"))
        }
    }

    @Test fun `performance`() {
        val engine = ScriptEngineManager().getEngineByExtension("kts")!!
        val random = Random(42)

        engine.eval("${random.nextInt(1000)} + ${random.nextInt(1000)}")

        val time1 = measureTimeMillis {
            for (i in 0..100)
                engine.eval("${random.nextInt(1000)} + ${random.nextInt(1000)}")
        }

        val time2 = measureTimeMillis {
            for (i in 0..100) {
                engine.context = SimpleScriptContext()
                engine.eval("${random.nextInt(1000)} + ${random.nextInt(1000)}")
            }
        }

        assertTrue("time1 = $time1; time2 = $time2", time2 / time1 < 5)
    }
}