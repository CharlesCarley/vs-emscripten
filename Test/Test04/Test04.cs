using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using static TestUtils.Utils;

namespace Test04
{
    [TestClass]
    public class Test04
    {
        [TestMethod]
        public void BuildWithDevEnv()
        {
            ClearIfExists(@"\Manual\Build");
            File.Delete($"{CurrentDirectory}output.log");

            var cmdLine = $@"Manual\Manual.sln /rebuild Debug /Out output.log";
            var output = Spawn(DevEnv, cmdLine);
            Log(output);

            output = $@"{CurrentDirectory}output.log";
            output = File.ReadAllText(output);
            Assert.IsTrue(output.Contains("========== Rebuild All: 5 succeeded, 0 failed, 0 skipped =========="));
        }

        [TestMethod]
        public void TestDefaultSwitchOutput()
        {
            // All the manual tests have Echo command line set to true.
            // This test should build the whole project, 
            // then search through the output log for any possible 
            // switch values that should NOT be set. 
            // 
            // The whole point is to not have to pass the switch,
            // and use what is default in Emscripten.
            // Which in turn reduces redundant code that needs to 
            // be executed.

            ClearIfExists(@"\Manual\Build");
            File.Delete($"{CurrentDirectory}output.log");

            var cmdLine =$@"Manual\Manual.sln /rebuild Debug /Out output.log";

            var output = Spawn(DevEnv, cmdLine);
            Log(output);

            output = $@"{CurrentDirectory}output.log";
            var lines = File.ReadAllLines(output);

            var switchTests = new[]
            {
                "ASSERTIONS",
                "STACK_OVERFLOW_CHECK",
                "RUNTIME_LOGGING",
                "VERBOSE",
                "ALLOW_MEMORY_GROWTH",
                "INITIAL_MEMORY",
                "sanitize",
            };

            foreach (var line in lines)
            {
                foreach (var sv in switchTests)
                    Assert.IsFalse(line.Contains(sv));
            }
        }

    }
}
