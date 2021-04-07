/*
-------------------------------------------------------------------------------
    Copyright (c) Charles Carley.

  This software is provided 'as-is', without any express or implied
  warranty. In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.
-------------------------------------------------------------------------------
*/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using static TestUtils.Utils;

namespace UnitTest
{
    [TestClass]
    public class Test02
    {
        private string CmakeIntDir => "__build__";
        // This does not search for an environment variable
        // So cmake must be in the path somewhere
        private string Cmake => "cmake";

        [TestMethod]
        public void CanCMakeExecute()
        {
            var output = Spawn(Cmake, "--help");
            Log(output);
        }

        private static string GetPartialPathName(string root)
        {
            // Cmake creates a varying directory with it's version number
            // so, for this to pass, cmake must be >= 3
            var enumDirs = Directory.EnumerateDirectories(root);
            foreach (var dir in enumDirs)
            {
                var split = dir.Split('\\');
                if (split.Length > 0 && split[split.Length - 1].StartsWith("3."))
                    return dir;
            }
            return null;
        }

        [TestMethod]
        public void ConfigureWithCMake()
        {
            ClearIfExists(CmakeIntDir);
            var cmdLine = $"-B __build__ -S {CurrentDirectory} -G \"Visual Studio 16 2019\" -A Emscripten -T emsdk";

            var output = Spawn(Cmake, cmdLine);
            Log(output);

            var testDir = GetPartialPathName($@"{CurrentDirectory}\{CmakeIntDir}\CMakeFiles");

            Assert.IsNotNull(testDir);
            Assert.IsTrue(File.Exists($@"{testDir}\CompilerIdC\CompilerIdC.wasm"));
            Assert.IsTrue(File.Exists($@"{testDir}\CompilerIdCxx\CompilerIdCxx.wasm"));
        }

        [TestMethod]
        public void BuildWithDevEnv()
        {
            ClearIfExists(CmakeIntDir);

            var cmdLine = $"-B __build__ -S {CurrentDirectory} -G \"Visual Studio 16 2019\" -A Emscripten -T emsdk";

            var output = Spawn(Cmake, cmdLine);
            Log(output);

            var testDir = GetPartialPathName($@"{CurrentDirectory}\{CmakeIntDir}\CMakeFiles");

            Assert.IsNotNull(testDir);

            Assert.IsTrue(File.Exists($@"{testDir}\CompilerIdC\CompilerIdC.wasm"));
            Assert.IsTrue(File.Exists($@"{testDir}\CompilerIdCxx\CompilerIdCxx.wasm"));

            cmdLine = $@"{CmakeIntDir}\Main1.sln /rebuild Debug /Out {CmakeIntDir}\output.log";

            output = Spawn(DevEnv, cmdLine);
            Log(output);

            output = $@"{CurrentDirectory}\{CmakeIntDir}\output.log";
            output = File.ReadAllText(output);
            Assert.IsTrue(output.Contains("========== Rebuild All: 2 succeeded, 0 failed, 1 skipped =========="));

            Assert.IsTrue(File.Exists($@"{CurrentDirectory}\{CmakeIntDir}\Debug\Main1.wasm"));

            output = Spawn(Wavm, $@" run {CmakeIntDir}\Debug\Main1.wasm");

            Assert.AreEqual("Hello Wasm World\n", output);
        }

        [TestMethod]
        public void TestVS_GLOBALS()
        {
            ClearIfExists(CmakeIntDir);
            File.Delete($"{CurrentDirectory}output.log");

            var cmdLine = $"-B __build__ -S {CurrentDirectory}\\VsGlobals -G \"Visual Studio 16 2019\" -A Emscripten -T emsdk";
            var output  = Spawn(Cmake, cmdLine);
            Log(output);

            cmdLine = $@"{CmakeIntDir}\VsGlobals.sln /rebuild Debug /Out output.log";
            output  = Spawn(DevEnv, cmdLine);
            Log(output);

            var lines = File.ReadAllLines($"{CurrentDirectory}output.log");

            var switchTests = new[] {
                "ASSERTIONS=2",
                "STACK_OVERFLOW_CHECK=2",
                "RUNTIME_LOGGING=1",
                "VERBOSE=1",
                "sanitize=undefined",
                "sanitize=address",
            };

            foreach (var line in lines)
            {
                Log(line);
                if (!string.IsNullOrEmpty(line) && 
                    line.Contains($"{EmCppBatch}") && 
                    !line.Contains("-c"))
                {
                    foreach (var sv in switchTests)
                        Assert.IsTrue(line.Contains(sv));
                }
            }
        }
    }
}
