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
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using EmscriptenTask;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Logger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;

namespace UnitTest
{
    public class BuildEngine : Microsoft.Build.Framework.IBuildEngine
    {
        public void LogErrorEvent(BuildErrorEventArgs e)
        {
            Logger.LogMessage("error : {0}", e.Message);
        }

        public void LogWarningEvent(BuildWarningEventArgs e)
        {
            Logger.LogMessage("warning : {0}", e.Message);
        }

        public void LogMessageEvent(BuildMessageEventArgs e)
        {
            Logger.LogMessage("{0}", e.Message);
        }

        public void LogCustomEvent(CustomBuildEventArgs e)
        {
            Logger.LogMessage("{0}", e.Message);
        }

        public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties,
                                     IDictionary targetOutputs)
        {
            return true;
        }

        public bool   ContinueOnError { get; }        = true;
        public int    LineNumberOfTaskNode { get; }   = 0;
        public int    ColumnNumberOfTaskNode { get; } = 0;
        public string ProjectFileOfTaskNode { get; }  = string.Empty;
    }

    [TestClass]
    public class CodeTests
    {
        private static string CurrentDirectory => $@"{Environment.CurrentDirectory}\..\..\";

        [TestMethod]
        public void SanitizeTest()
        {
            var result = EmUtils.Sanitize("A//////////B//C//////D/E//////////////F");
            Assert.AreEqual(@"A\B\C\D\E\F", result);
        }

        private static bool ClearIfExists()
        {
            var dir = CurrentDirectory + "Debug";
            if (!Directory.Exists(dir))
                return true;

            try
            {
                Directory.Delete(dir, true);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogMessage("{0}", ex.Message);
                return false;
            }
        }

        [TestMethod]
        public void Create()
        {
            ClearIfExists();

            var sourceFile = $@"{CurrentDirectory}\Tests\New Folder\New Text Document.c";
            var objFile    = $@"{CurrentDirectory}\Tests\t1.c.o";
            var wasmFile = $@"{CurrentDirectory}\Tests\t1.wasm";
            var trackerDir = $@"{CurrentDirectory}\Debug\";

            if (!Directory.Exists(trackerDir))
                Directory.CreateDirectory(trackerDir);
            var be = new BuildEngine();
            var task = new EmCxx {
                BuildEngine         = be,
                TrackerLogDirectory = trackerDir,
                ObjectFileName      = objFile,
                Sources             = new ITaskItem[] { new TaskItem(sourceFile) },
            };
            Assert.IsTrue(task.Execute());


            var link = new EmLink
            {
                BuildEngine = be,
                TrackerLogDirectory = trackerDir,
                OutputFile = wasmFile,
                Sources = new ITaskItem[] {new TaskItem(objFile)},
            };
            Assert.IsTrue(link.Execute());
        }
    }
}
