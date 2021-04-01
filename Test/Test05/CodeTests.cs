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
using System.Collections.Generic;
using System.IO;
using EmscriptenTask;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using static EmscriptenTask.EmSwitchWriter;
using static UnitTest.TestUtils;

namespace UnitTest
{
    public class TestSwitch
    {
        [IntSwitch("-a=", new[] { -2, -1, 0, 1, 2 })]
        public int Prop1 { get; set; }
    }

    [TestClass]
    public class CodeTests
    {
        [TestMethod]
        public void Create()
        {
            ClearIfExists(Debug);

            var sourceFile = $@"{CurrentDirectory}\Tests\New Folder\New Text Document.c";
            var objFile    = $@"{CurrentDirectory}\{Debug}\New Folder\t1.c.o";
            var depFile    = $@"{CurrentDirectory}\{Debug}\New Folder\t1.c.d";
            var wasmFile   = $@"{CurrentDirectory}\{Debug}\New Folder\t 1 2.wasm";
            var trackerDir = $@"{CurrentDirectory}\{Debug}\";

            if (!Directory.Exists(trackerDir))
                Directory.CreateDirectory(trackerDir);

            var be = new EmptyBuildEngine();

            var task = new EmCxx {
                BuildEngine            = be,
                TrackerLogDirectory    = trackerDir,
                ObjectFileName         = objFile,
                DependencyFileName     = depFile,
                GenerateDependencyFile = true,
                Sources                = new ITaskItem[] { new TaskItem(sourceFile) },
            };

            for (int i = 0; i < 3; i++)
                Assert.IsTrue(task.Execute());

            var link = new EmLink {
                BuildEngine         = be,
                TrackerLogDirectory = trackerDir,
                OutputFile          = new TaskItem(wasmFile),
                Sources             = new ITaskItem[] { new TaskItem(task.ObjectFileName) },
            };
            Assert.IsTrue(link.Execute());
        }

        private string TLogSub       = "";
        private string TLogDirectory => $@"{CurrentDirectory}\{Debug}\{TLogSub}\";

        private EmCxx[] MDR_SetupStaticLibrary(EmptyBuildEngine engine)
        {
            List<EmCxx> resultTasks = new List<EmCxx>();

            if (!Directory.Exists(TLogDirectory))
                Directory.CreateDirectory(TLogDirectory);

            var file1 = new EmCxx {
                BuildEngine            = engine,
                TrackerLogDirectory    = TLogDirectory,
                ObjectFileName         = $@"{CurrentDirectory}\{Debug}\Input1.cpp.o",
                DependencyFileName     = $@"{CurrentDirectory}\{Debug}\Input1.cpp.d",
                GenerateDependencyFile = true,
                Sources                = new ITaskItem[] {
                    new TaskItem($@"{CurrentDirectory}\Tests\MultiDependencyRebuild\Input1.cpp"),
                },
            };
            resultTasks.Add(file1);

            var file2 = new EmCxx {
                BuildEngine            = engine,
                TrackerLogDirectory    = TLogDirectory,
                ObjectFileName         = $@"{CurrentDirectory}\{Debug}\Input2.cpp.o",
                DependencyFileName     = $@"{CurrentDirectory}\{Debug}\Input2.cpp.d",
                GenerateDependencyFile = true,
                Sources                = new ITaskItem[] {
                    new TaskItem($@"{CurrentDirectory}\Tests\MultiDependencyRebuild\Input2.cpp"),
                },
            };
            resultTasks.Add(file2);

            var file3 = new EmCxx {
                BuildEngine            = engine,
                TrackerLogDirectory    = TLogDirectory,
                ObjectFileName         = $@"{CurrentDirectory}\{Debug}\Input3.cpp.o",
                DependencyFileName     = $@"{CurrentDirectory}\{Debug}\Input3.cpp.d",
                GenerateDependencyFile = true,
                Sources                = new ITaskItem[] {
                    new TaskItem($@"{CurrentDirectory}\Tests\MultiDependencyRebuild\Input3.cpp"),
                },
            };
            resultTasks.Add(file3);

            return resultTasks.ToArray();
        }

        private EmCxx[] MDR_SetupExecutable(EmptyBuildEngine engine)
        {
            List<EmCxx> resultTasks = new List<EmCxx>();

            if (!Directory.Exists(TLogDirectory))
                Directory.CreateDirectory(TLogDirectory);

            var file1 = new EmCxx {
                BuildEngine            = engine,
                TrackerLogDirectory    = TLogDirectory,
                ObjectFileName         = $@"{CurrentDirectory}\{Debug}\Main1.cpp.o",
                DependencyFileName     = $@"{CurrentDirectory}\{Debug}\Main1.cpp.d",
                GenerateDependencyFile = true,
                Sources                = new ITaskItem[] {
                    new TaskItem($@"{CurrentDirectory}\Tests\MultiDependencyRebuild\Main1.cpp"),
                },
            };
            resultTasks.Add(file1);

            var file2 = new EmCxx {
                BuildEngine            = engine,
                TrackerLogDirectory    = TLogDirectory,
                ObjectFileName         = $@"{CurrentDirectory}\{Debug}\Main.cpp.o",
                DependencyFileName     = $@"{CurrentDirectory}\{Debug}\Main.cpp.d",
                GenerateDependencyFile = true,
                Sources                = new ITaskItem[] {
                    new TaskItem($@"{CurrentDirectory}\Tests\MultiDependencyRebuild\Main.cpp"),
                },
            };
            resultTasks.Add(file2);

            return resultTasks.ToArray();
        }

        private void MDR_CompileObjectFiles(EmCxx[] objects)
        {
            foreach (var obj in objects)
                Assert.IsTrue(obj.Execute());

            // verify that it skipped it
            foreach (var obj in objects)
            {
                Assert.IsTrue(obj.Execute());
                Assert.IsTrue(obj.SkippedExecution);
            }

            foreach (var obj in objects)
            {
                Assert.IsTrue(File.Exists(obj.ObjectFileName));
                File.Delete(obj.ObjectFileName);
                Assert.IsTrue(!File.Exists(obj.ObjectFileName));
            }

            foreach (var obj in objects)
                Assert.IsTrue(obj.Execute());

            // verify that it actually rebuilt it
            foreach (var obj in objects)
                Assert.IsTrue(File.Exists(obj.ObjectFileName));

            // verify that it skipped it
            foreach (var obj in objects)
            {
                Assert.IsTrue(obj.Execute());
                Assert.IsTrue(obj.SkippedExecution);
            }
        }

        private ITaskItem[] MDR_MergeObjectFiles(EmCxx[] objects, EmAr lib = null)
        {
            var items = new ITaskItem[objects.Length + (lib != null ? 1 : 0)];
            var i     = 0;
            foreach (var obj in objects)
            {
                items[i] = new TaskItem(obj.ObjectFileName);
                ++i;
            }

            if (lib != null)
                items[i] = new TaskItem(lib.OutputFile);

            return items;
        }

        [TestMethod]
        public void MultiDependencyRebuild()
        {
            ClearIfExists(Debug);

            var engine = new EmptyBuildEngine();

            TLogSub     = "StaticObjectLogs";
            var objects = MDR_SetupStaticLibrary(engine);

            MDR_CompileObjectFiles(objects);

            var ar = new EmAr {
                BuildEngine         = engine,
                TrackerLogDirectory = TLogDirectory,
                OutputFile          = new TaskItem($@"{CurrentDirectory}\{Debug}\libInput.a"),
                Sources             = MDR_MergeObjectFiles(objects),
            };
            Assert.IsTrue(ar.Execute());
            Assert.IsTrue(ar.Execute());
            Assert.IsTrue(ar.SkippedExecution);

            Assert.IsTrue(File.Exists(ar.OutputFile.GetMetadata("FullPath")));
            File.Delete(ar.OutputFile.GetMetadata("FullPath"));
            Assert.IsTrue(ar.Execute());
            Assert.IsTrue(File.Exists(ar.OutputFile.GetMetadata("FullPath")));

            TLogSub = "ExecutableLogs";

            var exeObjects = MDR_SetupExecutable(engine);
            MDR_CompileObjectFiles(exeObjects);

            var lnk = new EmLink {
                ConfigurationType   = "Application",
                BuildEngine         = engine,
                TrackerLogDirectory = TLogDirectory,
                OutputFile          = new TaskItem($@"{CurrentDirectory}\{Debug}\mdr.wasm"),
                Sources             = MDR_MergeObjectFiles(exeObjects, ar),
            };

            // build it twice. the first call should
            // build it, and the second call should
            // skip it because it's up to date..
            Assert.IsTrue(lnk.Execute());
            Assert.IsTrue(lnk.Execute());
            Assert.IsTrue(lnk.SkippedExecution);

            Assert.IsTrue(File.Exists(lnk.OutputFile.GetMetadata("FullPath")));
            File.Delete(lnk.OutputFile.GetMetadata("FullPath"));
            Assert.IsTrue(!File.Exists(lnk.OutputFile.GetMetadata("FullPath")));

            Assert.IsTrue(lnk.Execute());
            Assert.IsTrue(!lnk.SkippedExecution);
            Assert.IsTrue(File.Exists(lnk.OutputFile.GetMetadata("FullPath")));

            // delete the archive and attempt to rebuild
            File.Delete(ar.OutputFile.GetMetadata("FullPath"));
            Assert.IsFalse(lnk.Execute());
            Assert.IsTrue(ar.Execute());
            Assert.IsTrue(lnk.Execute());
            Assert.IsTrue(!lnk.SkippedExecution);
            Assert.IsTrue(lnk.Execute());
            Assert.IsTrue(lnk.SkippedExecution);

            var wavm = FindWavm();
            Assert.IsNotNull(wavm);

            var output = "";
            var rc     = Spawn(wavm, $"run {lnk.OutputFile.GetMetadata("FullPath")}", ref output);
            Assert.AreEqual(0, rc);
            Assert.AreEqual("Called SomePrototypeInTheExecutableSource\n" +
                            "Called Input1PrototypeInTheStaticLibrarySource\n" +
                            "Called Input2PrototypeInTheStaticLibrarySource\n" +
                            "Called Input3PrototypeInTheStaticLibrarySource\n",
                            output);
        }

        [TestMethod]
        public void TestInvalidIntSwitchValue()
        {
            var a = new TestSwitch {
                Prop1 = 22
            };

            var builder = new StringWriter();
            Write(builder, a.GetType(), a);
            Assert.AreEqual(string.Empty, builder.ToString());
        }

        [TestMethod]
        public void TestValidIntSwitchValue()
        {
            var a = new TestSwitch();

            for (var i = 0; i < 6; i++)
            {
                a.Prop1     = -3 + i;
                var builder = new StringWriter();
                Write(builder, a.GetType(), a);
                if (i == 0 || i == 6)
                    Assert.AreEqual(string.Empty, builder.ToString());
                else
                    Assert.AreEqual($" -a={a.Prop1}", builder.ToString());
            }
        }

        [TestMethod]
        public void SanitizeTest()
        {
            var result = EmUtils.Sanitize("A//////////B//C//////D/E//////////////F");
            Assert.AreEqual(@"A\B\C\D\E\F", result);
        }

        [TestMethod]
        public void TestFileNameWithoutExtension()
        {
            var strings = new[] {
                "abc.123",
                "abc.def.hij.k",
                "/some/unix/_/path/abc.def.hij.k",
                @"Z:\some\windows\_\path\abc.def.hij.k",
                @"Z:\s o m e\w i n d o w s\_\p a t h\a b c.d e f.h i j.k",
                @"96587658765*&R8 T^RuyTR876R 87^5r8&^8&^%9^$#&%^$^$#%#^$^*&(07",
                @"96587658765*&R8 T^RuyTR876R/87^5r8&^8&^%9^$#&%^$^$#%#^$^*&(07",
                @"96587658765*&R8 T^RuyTR876R/87^5r8&^8&^%9^$#&%^$^$#%#^$^*&(07/file.txt",
            };

            var expected = new[] {
                "abc",
                "abc",
                @"\some\unix\_\path\abc",
                @"Z:\some\windows\_\path\abc",
                @"Z:\s o m e\w i n d o w s\_\p a t h\a b c",
                @"96587658765*&R8 T^RuyTR876R 87^5r8&^8&^%9^$#&%^$^$#%#^$^*&(07",
                @"96587658765*&R8 T^RuyTR876R\87^5r8&^8&^%9^$#&%^$^$#%#^$^*&(07",
                @"96587658765*&R8 T^RuyTR876R\87^5r8&^8&^%9^$#&%^$^$#%#^$^*&(07\file",
            };

            for (var i = 0; i < strings.Length; ++i)
            {
                var a = strings[i];
                var b = expected[i];
                var c = EmUtils.FileNameWithoutExtension(a);
                Assert.AreEqual(b, c);
            }
        }
    }
}
