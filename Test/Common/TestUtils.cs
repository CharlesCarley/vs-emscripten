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

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Logger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;

namespace TestUtils
{
    public static class Utils
    {
        public static string CurrentDirectory => $@"{Environment.CurrentDirectory}\..\..\";
        public static string IntDir           => "Debug";

        private static string _wavm;
        private static string _msBuild;
        private static string _devEnv;
        private static string _emcpp;

        public static string Wavm    => _wavm ?? (_wavm = FindWavm());
        public static string MsBuild => _msBuild ?? (_msBuild = FindMsBuild());
        public static string DevEnv  => _devEnv ?? (_devEnv = FindDevEnv());
        public static string EmCppBatch => _emcpp ?? (_emcpp = FindEmCppBatch());

        public static string WriteSwitchesToString(object obj)
        {
            var writer = new StringWriter();
            EmscriptenTask.EmSwitchWriter.Write(writer, obj.GetType(), obj);
            return writer.ToString();
        }

        public static void ClearIfExists(string directory)
        {
            var dir = CurrentDirectory + directory;
            if (!Directory.Exists(dir))
                return;
            try
            {
                Directory.Delete(dir, true);
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }

            Assert.IsFalse(Directory.Exists(dir));
        }

        public static void Log(string message)
        {
            Logger.LogMessage("{0}", message);
        }

        public static string Spawn(string program, string args, int expectedReturn = 0)
        {
            var proc = Process.Start(new ProcessStartInfo(program) {
                CreateNoWindow         = true,
                RedirectStandardOutput = true,
                UseShellExecute        = false,
                WorkingDirectory       = CurrentDirectory,
                Arguments              = args
            });

            Assert.IsNotNull(proc);
            var output = proc.StandardOutput.ReadToEnd();

            proc.WaitForExit();
            Assert.AreEqual(expectedReturn, proc.ExitCode);
            return output;
        }

        private static string FindWavm()
        {
            var emDir = Environment.GetEnvironmentVariable("EMSDK");
            if (emDir == null)
                Assert.Fail("Failed to find the EMSDK environment variable.");

            var pathToWavm = $@"{emDir}\upstream\bin\wavm.exe";
            if (!File.Exists(pathToWavm))
                Assert.Fail($"Failed to find WAVM in {pathToWavm}.");
            return pathToWavm;
        }

        private static string FindEmCppBatch()
        {
            var emDir = Environment.GetEnvironmentVariable("EMSDK");
            if (emDir == null)
                Assert.Fail("Failed to find the EMSDK environment variable.");

            var path = $@"{emDir}\upstream\emscripten\em++.bat";
            if (!File.Exists(path))
                Assert.Fail($"Failed to find em++ in {path}.");
            return path;
        }

        private static string FindMsBuild()
        {
            var vsDir = Environment.GetEnvironmentVariable("VS2019INSTALLDIR");
            if (vsDir == null)
                Assert.Fail("Failed to find the VS2019INSTALLDIR environment variable.");

            var pathToMsBuild = $@"{vsDir}\MSBuild\Current\Bin\MSBuild.exe";
            if (!File.Exists(pathToMsBuild))
                Assert.Fail($"Failed to find MSBuild in {pathToMsBuild}.");

            return pathToMsBuild;
        }

        private static string FindDevEnv()
        {
            var vsDir = Environment.GetEnvironmentVariable("VS2019INSTALLDIR");
            if (vsDir == null)
                Assert.Fail("Failed to find the VS2019INSTALLDIR environment variable.");

            var pathToDevEnv = $@"{vsDir}\Common7\IDE\devenv.exe";
            if (!File.Exists(pathToDevEnv))
                Assert.Fail($"Failed to find devenv in {pathToDevEnv}.");

            return pathToDevEnv;
        }
    }
}
