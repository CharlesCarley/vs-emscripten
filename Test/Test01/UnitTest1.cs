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
        private string msBuild;
        private string wavm;

        bool SetupEnv()
        {
            msBuild = null;
            wavm = null;
            var vsDir = Environment.GetEnvironmentVariable("VS2019INSTALLDIR");
            if (vsDir == null)
            {
                return false;
            }

            var pathToMsBuild = $@"{vsDir}\MSBuild\Current\Bin\MSBuild.exe";
            if (File.Exists(pathToMsBuild))
            {
                msBuild = pathToMsBuild;
            }

            var emDir = Environment.GetEnvironmentVariable("EMSDK");
            if (emDir == null)
            {
                return false;
            }

            var pathToWAVM = $@"{emDir}\upstream\bin\wavm.exe";
            if (File.Exists(pathToWAVM))
            {
                wavm = pathToWAVM;
            }

            return msBuild != null && wavm != null;
        }

        private static void LogMessage(string message)
        {
            Logger.LogMessage("{0}", message);
        }
        string GetWorkingDirectory()
        {
            return $@"{Environment.CurrentDirectory}\..\..\";
        }

        bool ClearIfExists()
        {
            var dir = GetWorkingDirectory() + "Debug";
            if (Directory.Exists(dir))
            {
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
            return true;
        }

        [TestMethod]
        public void TestFindMSBuild()
        {
            Assert.IsTrue(SetupEnv());
        }

        public int Spawn(string prog, string args, ref string output)
        {
            var proc = Process.Start(new ProcessStartInfo(prog)
            {
                CreateNoWindow = true,
                RedirectStandardOutput = output != null,
                UseShellExecute = false,
                WorkingDirectory = GetWorkingDirectory(),
                Arguments = args
            });

            if (output != null)
            {
                output = proc.StandardOutput.ReadToEnd();
                LogMessage(output);
            }

            proc.WaitForExit();
            return proc.ExitCode;
        }

        [TestMethod]
        public void CanMSBuildExec()
        {
            Assert.IsTrue(SetupEnv());

            string output = "";
            int rc = Spawn(msBuild, "/?", ref output);
            LogMessage(output);
            Assert.AreEqual(0, rc);
        }

        [TestMethod]
        public void RunTest01()
        {
            Assert.IsTrue(SetupEnv() && ClearIfExists());

            string output = string.Empty;
            int rc = Spawn(msBuild, "Test1", ref output);
            Assert.AreEqual(0, rc);

            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\main.cpp.o"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\Test1.wasm"));

            output = string.Empty;
            rc = Spawn(wavm, @" run Debug\Test1.wasm", ref output);
            Assert.AreEqual(0, rc);
            Assert.AreEqual("Main.cpp Hello WASM World!\n", output);
            Assert.IsTrue(ClearIfExists());
        }

        [TestMethod]
        public void RunTest02()
        {
            Assert.IsTrue(SetupEnv() && ClearIfExists());

            string output = string.Empty;
            int rc = Spawn(msBuild, "Test2", ref output);
            Assert.AreEqual(0, rc);

            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\main.abcdefg"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\Test2.wasm"));

            output = string.Empty;
            rc = Spawn(wavm, @" run Debug\Test2.wasm", ref output);
            Assert.AreEqual(0, rc);
            Assert.AreEqual("Main.cpp Hello WASM World!\n", output);
            Assert.IsTrue(ClearIfExists());
        }

        [TestMethod]
        public void RunTest03()
        {
            Assert.IsTrue(SetupEnv() && ClearIfExists());

            string output = string.Empty;
            int rc = Spawn(msBuild, "Test3", ref output);
            Assert.AreEqual(0, rc);

            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\main.c.o"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\Test3.wasm"));

            output = string.Empty;
            rc = Spawn(wavm, @" run Debug\Test3.wasm", ref output);
            Assert.AreEqual(0, rc);
            Assert.AreEqual("main.c Hello WASM World!\n", output);
            Assert.IsTrue(ClearIfExists());
        }

        [TestMethod]
        public void RunTest04()
        {
            Assert.IsTrue(SetupEnv() && ClearIfExists());

            string output = string.Empty;
            int rc = Spawn(msBuild, "Test4", ref output);
            Assert.AreEqual(0, rc);

            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\main2.c.o"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\fn1.c.o"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\fn2.c.o"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\fn3.c.o"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\Test4.wasm"));

            output = string.Empty;
            rc = Spawn(wavm, @" run Debug\Test4.wasm", ref output);
            Assert.AreEqual(0, rc);
            Assert.AreEqual("Main2--------------------------------Hello WASM World!\n", output);
            Assert.IsTrue(ClearIfExists());
        }

        [TestMethod]
        public void RunTest05()
        {
            Assert.IsTrue(SetupEnv() && ClearIfExists());

            string output = string.Empty;
            int rc = Spawn(msBuild, "Test5", ref output);
            Assert.AreEqual(0, rc);

            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\main2.c.o"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\fn1.c.o"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\fn2.c.o"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\fn3.c.o"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\fnLib.a"));
            Assert.IsTrue(File.Exists($@"{GetWorkingDirectory()}\Debug\Test5.wasm"));

            output = string.Empty;
            rc = Spawn(wavm, @" run Debug\Test5.wasm", ref output);
            Assert.AreEqual(0, rc);
            Assert.AreEqual("Main2--------------------------------Hello WASM World!\n", output);
            Assert.IsTrue(ClearIfExists());
        }
    }

}
