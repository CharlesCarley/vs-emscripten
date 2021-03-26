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
        private string _msBuild;
        private string _wavm;

        private bool SetupEnv()
        {
            _msBuild = null;
            _wavm    = null;

            var vsDir = Environment.GetEnvironmentVariable("VS2019INSTALLDIR");
            if (vsDir == null)
                return false;

            var pathToMsBuild = $@"{vsDir}\MSBuild\Current\Bin\MSBuild.exe";
            if (File.Exists(pathToMsBuild))
                _msBuild = pathToMsBuild;

            var emDir = Environment.GetEnvironmentVariable("EMSDK");
            if (emDir == null)
                return false;

            var pathToWavm = $@"{emDir}\upstream\bin\wavm.exe";
            if (File.Exists(pathToWavm))
                _wavm = pathToWavm;

            return _msBuild != null && _wavm != null;
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
            var dir = GetWorkingDirectory() + "Debug";
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

        [TestMethod]
        public void TestFindMsBuild()
        {
            Assert.IsTrue(SetupEnv());
        }

        private static int Spawn(string program, string args, ref string output)
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
        public void CanMsBuildExec()
        {
            Assert.IsTrue(SetupEnv());

            var output = "";
            var rc     = Spawn(_msBuild, "/?", ref output);
            LogMessage(output);
            Assert.AreEqual(0, rc);
        }

        [TestMethod]
        public void RunTest01()
        {
            Assert.IsTrue(SetupEnv() && ClearIfExists());

            var output = string.Empty;

            var rc = Spawn(_msBuild, "Test1", ref output);
            Assert.AreEqual(0, rc);

            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\main.cpp.o"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\Test1.wasm"));

            output = string.Empty;
            rc     = Spawn(_wavm, @" run Debug\Test1.wasm", ref output);
            Assert.AreEqual(0, rc);
            Assert.AreEqual("Main.cpp Hello WASM World!\n", output);
            Assert.IsTrue(ClearIfExists());
        }

        [TestMethod]
        public void RunTest02()
        {
            Assert.IsTrue(SetupEnv() && ClearIfExists());

            var output = string.Empty;
            var rc     = Spawn(_msBuild, "Test2", ref output);

            Assert.AreEqual(0, rc);
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\main.abcdefg"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\Test2.wasm"));

            output = string.Empty;
            rc     = Spawn(_wavm, @" run Debug\Test2.wasm", ref output);
            Assert.AreEqual(0, rc);
            Assert.AreEqual("Main.cpp Hello WASM World!\n", output);
            Assert.IsTrue(ClearIfExists());
        }

        [TestMethod]
        public void RunTest03()
        {
            Assert.IsTrue(SetupEnv() && ClearIfExists());

            var output = string.Empty;
            var rc     = Spawn(_msBuild, "Test3", ref output);
            Assert.AreEqual(0, rc);

            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\main.c.o"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\Test3.wasm"));

            output = string.Empty;
            rc     = Spawn(_wavm, @" run Debug\Test3.wasm", ref output);
            Assert.AreEqual(0, rc);
            Assert.AreEqual("main.c Hello WASM World!\n", output);
            Assert.IsTrue(ClearIfExists());
        }

        [TestMethod]
        public void RunTest04()
        {
            Assert.IsTrue(SetupEnv() && ClearIfExists());

            var output = string.Empty;
            var rc     = Spawn(_msBuild, "Test4", ref output);
            Assert.AreEqual(0, rc);

            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\main2.c.o"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\fn1.c.o"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\fn2.c.o"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\fn3.c.o"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\Test4.wasm"));

            output = string.Empty;
            rc     = Spawn(_wavm, @" run Debug\Test4.wasm", ref output);
            Assert.AreEqual(0, rc);
            Assert.AreEqual("Main2--------------------------------Hello WASM World!\n", output);
            Assert.IsTrue(ClearIfExists());
        }

        [TestMethod]
        public void RunTest05()
        {
            Assert.IsTrue(SetupEnv() && ClearIfExists());

            var output = string.Empty;
            var rc     = Spawn(_msBuild, "Test5", ref output);
            Assert.AreEqual(0, rc);

            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\main2.c.o"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\fn1.c.o"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\fn2.c.o"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\fn3.c.o"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\fnLib.a"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\Test5.wasm"));

            output = string.Empty;
            rc     = Spawn(_wavm, @" run Debug\Test5.wasm", ref output);
            Assert.AreEqual(0, rc);
            Assert.AreEqual("Main2--------------------------------Hello WASM World!\n", output);
            Assert.IsTrue(ClearIfExists());
        }
    }
}
