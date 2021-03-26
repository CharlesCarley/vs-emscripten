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
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using System;
using System.Diagnostics;
using System.IO;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        private string _devEnv;
        private string _wavm;

        // This does not search for an environment variable
        // So cmake must be in the path somewhere
        private string _cmake;

        private bool SetupEnv()
        {
            _devEnv   = null;
            _cmake    = "cmake";
            var vsDir = Environment.GetEnvironmentVariable("VS2019INSTALLDIR");
            if (vsDir == null)
                return false;

            var pathToDevEnv = $@"{vsDir}\Common7\IDE\devenv.exe";
            if (File.Exists(pathToDevEnv))
                _devEnv = pathToDevEnv;

            var emDir = Environment.GetEnvironmentVariable("EMSDK");
            if (emDir == null)
                return false;

            var pathToWavm = $@"{emDir}\upstream\bin\wavm.exe";
            if (File.Exists(pathToWavm))
                _wavm = pathToWavm;

            return _devEnv != null && _wavm != null;
        }

        private static void LogMessage(string message)
        {
            Logger.LogMessage("{0}", message);
        }

        private static string GetWorkingDirectory()
        {
            return $@"{Environment.CurrentDirectory}\..\..\";
        }

        private static bool ClearIfExists()
        {
            var dir = GetWorkingDirectory() + "__build__";
            if (!Directory.Exists(dir))
                return true;

            try
            {
                Directory.Delete(dir, true);
                return true;
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message);
                return false;
            }
        }

        public int Spawn(string program, string args, ref string output)
        {
            var proc = Process.Start(new ProcessStartInfo(program) {
                CreateNoWindow         = true,
                RedirectStandardOutput = output != null,
                UseShellExecute        = false,
                WorkingDirectory       = GetWorkingDirectory(),
                Arguments              = args
            });

            if (proc == null)
                return 1;

            if (output != null)
            {
                output = proc.StandardOutput.ReadToEnd();
                LogMessage(output);
            }

            proc.WaitForExit();
            return proc.ExitCode;
        }

        [TestMethod]
        public void CanCMakeExecute()
        {
            Assert.IsTrue(SetupEnv());

            var output = "";
            var rc     = Spawn(_cmake, "--help", ref output);

            LogMessage(output);
            Assert.AreEqual(0, rc);
        }

        private static string GetPartialPathName(string root)
        {
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
            Assert.IsTrue(SetupEnv() && ClearIfExists());

            var output = "";
            var rc = Spawn(_cmake,
                           $"-B __build__ -S {GetWorkingDirectory()} -G \"Visual Studio 16 2019\" -A Emscripten -T emsdk",
                           ref output);
            LogMessage(output);
            Assert.AreEqual(0, rc);

            var testDir = GetPartialPathName($@"{ GetWorkingDirectory()}\__build__\CMakeFiles");
            Assert.IsNotNull(testDir);
            Assert.IsTrue(File.Exists($@"{testDir}\CompilerIdC\CompilerIdC.wasm"));
            Assert.IsTrue(File.Exists($@"{testDir}\CompilerIdCxx\CompilerIdCxx.wasm"));
        }

        [TestMethod]
        public void BuildWithDevEnv()
        {
            Assert.IsTrue(SetupEnv() && ClearIfExists());

            var output = "";
            var rc     = Spawn(_cmake,
                           $"-B __build__ -S {GetWorkingDirectory()} -G \"Visual Studio 16 2019\" -A Emscripten -T emsdk",
                           ref output);
            LogMessage(output);
            Assert.AreEqual(0, rc);

            var testDir = GetPartialPathName($@"{ GetWorkingDirectory()}\__build__\CMakeFiles");
            Assert.IsNotNull(testDir);

            Assert.IsTrue(File.Exists($@"{testDir}\CompilerIdC\CompilerIdC.wasm"));
            Assert.IsTrue(File.Exists($@"{testDir}\CompilerIdCxx\CompilerIdCxx.wasm"));

            output = "";
            rc = Spawn(_devEnv,
                       @"__build__\Main1.sln /rebuild Debug /Out __build__\output.log",
                       ref output);
            LogMessage(output);
            Assert.AreEqual(0, rc);

            output = $@"{GetWorkingDirectory()}__build__\output.log";
            output = File.ReadAllText(output);
            Assert.IsTrue(output.Contains("========== Rebuild All: 2 succeeded, 0 failed, 1 skipped =========="));

            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}__build__\Debug\Main1.wasm"));
            output = string.Empty;
            rc     = Spawn(_wavm, @" run __build__\Debug\Main1.wasm", ref output);
            Assert.AreEqual(0, rc);
            Assert.AreEqual("Hello Wasm World\n", output);
            Assert.IsTrue(ClearIfExists());
        }
    }
}
