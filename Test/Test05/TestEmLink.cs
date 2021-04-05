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
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static TestUtils.Utils;


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
            Assert.AreEqual(null, obj.AllSource);

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

            Assert.AreEqual(null, obj.EmSdlVersion);
            Assert.AreEqual(false, obj.EmUseFullOpenGles2);
            Assert.AreEqual(false, obj.EmUseFullOpenGles3);
            Assert.AreEqual(null, obj.EmMinWebGlVersion);
            Assert.AreEqual(null, obj.EmMaxWebGlVersion);
            Assert.AreEqual(null, obj.EmPreloadFile);
            Assert.AreEqual(null, obj.EmEmbeddedFile);
        }

        private static ITaskItem[] ItemsFromString(string semiColonSeparated)
        {
            var list = semiColonSeparated.Split(';');
            if (list.Length <= 0)
                return null;

            var items = new ITaskItem[list.Length];
            for (var i = 0; i < list.Length; ++i)
                items[i] = new TaskItem(list[i]);

            return items;
        }

        [TestMethod]
        public void TestTrackerLogDirectory()
        {
            var obj = new EmscriptenTask.EmLink {
                TrackerLogDirectory = "/Some/Path/To/The/Log/Files"
            };

            Assert.AreEqual(@"\Some\Path\To\The\Log\Files\EmLink.read.1.tlog", obj.TLogReadFiles[0].ItemSpec);
            Assert.AreEqual(@"\Some\Path\To\The\Log\Files\EmLink.write.1.tlog", obj.TLogWriteFiles[0].ItemSpec);

            obj.TrackerLogDirectory = null;
            Assert.AreEqual(@"EmLink.read.1.tlog", obj.TLogReadFiles[0].ItemSpec);
            Assert.AreEqual(@"EmLink.write.1.tlog", obj.TLogWriteFiles[0].ItemSpec);

            obj.TrackerLogDirectory = "A n o t h e r/ p a t h";

            Assert.AreEqual(@"A n o t h e r\ p a t h\EmLink.read.1.tlog", obj.TLogReadFiles[0].ItemSpec);
            Assert.AreEqual(@"A n o t h e r\ p a t h\EmLink.write.1.tlog", obj.TLogWriteFiles[0].ItemSpec);
        }

        [TestMethod]
        public void TestSources()
        {
            var obj = new EmscriptenTask.EmLink {
                Sources = ItemsFromString("a.o;b.o")
            };

            Assert.AreNotEqual(null, obj.Sources);
            Assert.AreEqual(2, obj.Sources.Length);
            Assert.AreEqual("a.o", obj.Sources[0].ItemSpec);
            Assert.AreEqual(15, obj.Sources[0].MetadataCount);

            var mockFileLoc = Environment.CurrentDirectory;
            var driveRoot   = $"{Path.GetPathRoot(mockFileLoc)}";

            Assert.AreNotEqual(null, obj.Sources[0].MetadataNames);
            Assert.AreEqual($@"{mockFileLoc}\a.o", obj.Sources[0].GetMetadata("FullPath"));
            Assert.AreEqual($@"{driveRoot}", obj.Sources[0].GetMetadata("RootDir"));
            Assert.AreEqual("a", obj.Sources[0].GetMetadata("Filename"));
            Assert.AreEqual(".o", obj.Sources[0].GetMetadata("Extension"));
            Assert.AreEqual("", obj.Sources[0].GetMetadata("RelativeDir"));
            Assert.AreEqual($"{mockFileLoc.Replace(driveRoot, "")}\\", obj.Sources[0].GetMetadata("Directory"));
        }

        [TestMethod]
        public void TestOutputFileSwitch()
        {
            var obj = new EmscriptenTask.EmLink {
                OutputFile = new TaskItem("ABC.wasm")
            };

            var result      = WriteSwitchesToString(obj);
            var mockFileLoc = Environment.CurrentDirectory;

            Assert.AreEqual($@" -o {mockFileLoc}\ABC.wasm", result);
            obj.OutputFile = new TaskItem("Z:/Some Space / Separated Drive/A B C.wasm");

            var result1 = WriteSwitchesToString(obj);
            Assert.AreEqual(" -o \"Z:\\Some Space \\ Separated Drive\\A B C.wasm\"", result1);
        }

        [TestMethod]
        public void TestAdditionalDependenciesSwitch()
        {
            var obj = new EmscriptenTask.EmLink {
                AdditionalDependencies = "ANonExistingDependency.a"
            };

            var result = WriteSwitchesToString(obj);

            // AdditionalDependencies has the validate argument
            // set to true. So the expected behavior is to
            // skip it if the file does not exist.
            Assert.AreEqual(string.Empty, result);

            var curDir                 = Environment.CurrentDirectory;
            obj.AdditionalDependencies = $@"{curDir}\..\..\TestEmLink.cs";
            var expected               = $@" {curDir}\..\..\TestEmLink.cs";

            var result2 = WriteSwitchesToString(obj);

            Assert.IsTrue(expected.Equals(result2));
        }

        [TestMethod]
        public void TestAdditionalLibraryDirectories()
        {
            var obj = new EmscriptenTask.EmLink {
                AdditionalLibraryDirectories = "A/Non/Existing/Dependency/New Folder"
            };

            var result = WriteSwitchesToString(obj);

            // AdditionalLibraryDirectories has the validate argument
            // set to true. So the expected behavior is to
            // skip it if the file does not exist.
            Assert.AreEqual(string.Empty, result);

            var curDir                       = Environment.CurrentDirectory;
            obj.AdditionalLibraryDirectories = $@"{curDir}\..\..\New Folder";
            var expected                     = $" -L \"{curDir}\\..\\..\\New Folder\"";

            var result2 = WriteSwitchesToString(obj);

            Assert.IsTrue(expected.Equals(result2));
        }

        [TestMethod]
        public void TestEmWasmMode()
        {
            var obj = new EmscriptenTask.EmLink {
                EmWasmMode = "EmWasmOnlyJS"
            };
            var result1 = WriteSwitchesToString(obj);
            Assert.AreEqual(" -s WASM=0", result1);

            obj.EmWasmMode = "EmWasmOnlyWasm";
            var result2    = WriteSwitchesToString(obj);
            Assert.AreEqual(" -s WASM=1", result2);

            obj.EmWasmMode = "EmWasmBoth";
            var result3    = WriteSwitchesToString(obj);
            Assert.AreEqual(" -s WASM=2", result3);

            obj.EmWasmMode = "Anything Else Uses The Default";
            var result4    = WriteSwitchesToString(obj);
            Assert.AreEqual(" -s WASM=1", result4);
        }

        [TestMethod]
        public void TestEmSdlVersion()
        {
            var obj = new EmscriptenTask.EmLink
            {
                EmSdlVersion = ""
            };
            var result1 = WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            obj.EmSdlVersion = "-1";
            var result2 = WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result2);

            obj.EmSdlVersion = "3";
            var result3 = WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result3);

            obj.EmSdlVersion = " 1";
            var result4 = WriteSwitchesToString(obj);
            Assert.AreEqual(" -s USE_SDL=1", result4);

            obj.EmSdlVersion = " 2              ";
            var result5 = WriteSwitchesToString(obj);
            Assert.AreEqual(" -s USE_SDL=2", result5);
        }

        [TestMethod]
        public void TestEmUseFullOpenGles2()
        {
            var obj = new EmscriptenTask.EmLink
            {
                EmUseFullOpenGles2 = false
            };
            var result1 = WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            obj.EmUseFullOpenGles2 =true;
            var result4 = WriteSwitchesToString(obj);
            Assert.AreEqual(" -s FULL_ES2=1", result4);
        }

        [TestMethod]
        public void TestEmUseFullOpenGles3()
        {
            var obj = new EmscriptenTask.EmLink
            {
                EmUseFullOpenGles3 = false
            };
            var result1 = WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            obj.EmUseFullOpenGles3 = true;
            var result4 = WriteSwitchesToString(obj);
            Assert.AreEqual(" -s FULL_ES3=1", result4);
        }


        [TestMethod]
        public void TestEmMinWebGlVersion()
        {
            var obj = new EmscriptenTask.EmLink
            {
                EmMinWebGlVersion = ""
            };
            var result1 = WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            obj.EmMinWebGlVersion = "-1";
            var result2 = WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result2);

            obj.EmMinWebGlVersion = "3";
            var result3 = WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result3);

            obj.EmMinWebGlVersion = " 1";
            var result4 = WriteSwitchesToString(obj);
            Assert.AreEqual(" -s MIN_WEBGL_VERSION=1", result4);

            obj.EmMinWebGlVersion = " 2              ";
            var result5 = WriteSwitchesToString(obj);
            Assert.AreEqual(" -s MIN_WEBGL_VERSION=2", result5);
        }


        [TestMethod]
        public void TestEmMaxWebGlVersion()
        {
            var obj = new EmscriptenTask.EmLink
            {
                EmMaxWebGlVersion = ""
            };
            var result1 = WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            obj.EmMaxWebGlVersion = "-1";
            var result2 = WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result2);

            obj.EmMaxWebGlVersion = "3";
            var result3 = WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result3);

            obj.EmMaxWebGlVersion = " 1";
            var result4 = WriteSwitchesToString(obj);
            Assert.AreEqual(" -s MAX_WEBGL_VERSION=1", result4);

            obj.EmMaxWebGlVersion = " 2              ";
            var result5 = WriteSwitchesToString(obj);
            Assert.AreEqual(" -s MAX_WEBGL_VERSION=2", result5);
        }

        [TestMethod]
        public void TestEmPreloadFile()
        {
            var obj = new EmscriptenTask.EmLink
            {
                EmPreloadFile = "Foo.Bar"
            };
            var result1 = WriteSwitchesToString(obj);
            Assert.AreEqual(" --preload-file Foo.Bar", result1);

            obj.EmPreloadFile = $@"{CurrentDirectory}\Tests\New Folder\New Text Document.c";
            var result2 = WriteSwitchesToString(obj);
            Assert.AreEqual($" --preload-file \"{CurrentDirectory}\\Tests\\New Folder\\New Text Document.c\"", result2);
        }


        [TestMethod]
        public void TestEmEmbeddedFile()
        {
            var obj = new EmscriptenTask.EmLink
            {
                EmEmbeddedFile = "Foo.Bar"
            };
            var result1 = WriteSwitchesToString(obj);
            Assert.AreEqual(" --embed-file Foo.Bar", result1);

            obj.EmEmbeddedFile = $@"{CurrentDirectory}\Tests\New Folder\New Text Document.c";
            var result2 = WriteSwitchesToString(obj);
            Assert.AreEqual($" --embed-file \"{CurrentDirectory}\\Tests\\New Folder\\New Text Document.c\"", result2);
        }
    }
}
