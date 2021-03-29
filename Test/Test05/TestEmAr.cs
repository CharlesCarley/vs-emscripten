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
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class TestEmAr
    {
        [TestMethod]
        public void TestDefaults()
        {
            var obj = new EmscriptenTask.EmAr();

            // EmTask Settings
            Assert.AreEqual(null, obj.BuildEngine);
            Assert.AreEqual(null, obj.HostObject);
            Assert.AreEqual(null, obj.TrackerLogDirectory);
            Assert.AreNotEqual(null, obj.TLogReadFiles);
            Assert.AreNotEqual(null, obj.TLogWriteFiles);
            Assert.AreEqual(@"EmAr.read.1.tlog", obj.TLogReadFiles[0].ItemSpec);
            Assert.AreEqual(@"EmAr.write.1.tlog", obj.TLogWriteFiles[0].ItemSpec);
            Assert.AreEqual(null, obj.Sources);
            Assert.AreEqual(true, obj.MinimalRebuildFromTracking);
            Assert.AreEqual(null, obj.DebugProp1);
            Assert.AreEqual(null, obj.DebugProp2);
            Assert.AreEqual(false, obj.DebugProp3);
            Assert.AreEqual(false, obj.SkippedExecution);
            Assert.AreEqual(false, obj.Verbose);
            Assert.AreEqual(false, obj.EchoCommandLines);

            // Points to static data, so it's dependent on the whole test set
            // and is valid only if ValidateSdk is called. It's not public, so... 
            if (obj.EmscriptenDirectory != null)
                Assert.IsTrue(obj.EmscriptenDirectory.Contains(@"\upstream\emscripten"));
            if (obj.EmccTool != null)
                Assert.IsTrue(obj.EmccTool.EndsWith("emcc.bat"));

            // EmAr
            Assert.AreEqual(null, obj.OutputFile);
        }


        [TestMethod]
        public void TestOutputFileSwitch()
        {
            var obj = new EmscriptenTask.EmAr
            {
                OutputFile = new TaskItem("ABC.a")
            };

            var result = TestUtils.WriteSwitchesToString(obj);
            var mockFileLoc = Environment.CurrentDirectory;

            Assert.AreEqual($@" qc {mockFileLoc}\ABC.a", result);
            obj.OutputFile = new TaskItem("Z:/Some Space / Separated Drive/A B C.a");

            var result1 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" qc \"Z:\\Some Space \\ Separated Drive\\A B C.a\"", result1);
        }
    }
}
