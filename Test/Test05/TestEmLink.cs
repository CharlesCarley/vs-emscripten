﻿/*
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class TestEmLink
    {
        [TestMethod]
        public void TestDefaults()
        {
            var obj = new EmscriptenTask.EmLink();

            // EmTask Settings
            Assert.AreEqual(null, obj.BuildEngine);
            Assert.AreEqual(null, obj.HostObject);
            Assert.AreEqual(null, obj.TrackerLogDirectory);
            Assert.AreNotEqual(null, obj.TLogReadFiles);
            Assert.AreNotEqual(null, obj.TLogWriteFiles);
            Assert.AreEqual(@"EmLink.read.1.tlog", obj.TLogReadFiles[0].ItemSpec);
            Assert.AreEqual(@"EmLink.write.1.tlog", obj.TLogWriteFiles[0].ItemSpec);
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

            Assert.AreEqual(null, obj.ConfigurationType);
            Assert.AreEqual(null, obj.OutputFile);
            Assert.AreEqual(null, obj.AdditionalDependencies);
            Assert.AreEqual(null, obj.AdditionalLibraryDirectories);
            Assert.AreEqual(null, obj.AdditionalOptions);
            Assert.AreEqual(null, obj.EmWasmMode);
        }
    }
}
