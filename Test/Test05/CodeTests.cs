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
using static TestUtils.Utils;

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
            ClearIfExists(IntDir);

            var sourceFile = $@"{CurrentDirectory}\Tests\New Folder\New Text Document.c";
            var objFile    = $@"{CurrentDirectory}\{IntDir}\New Folder\t1.c.o";
            var depFile    = $@"{CurrentDirectory}\{IntDir}\New Folder\t1.c.d";
            var wasmFile   = $@"{CurrentDirectory}\{IntDir}\New Folder\t 1 2.wasm";
            var trackerDir = $@"{CurrentDirectory}\{IntDir}\";

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
        private string TLogDirectory => $@"{CurrentDirectory}\{IntDir}\{TLogSub}\";

        private EmCxx[] SetupStaticLibrary(EmptyBuildEngine engine)
        {
            var resultTasks = new List<EmCxx>();

            if (!Directory.Exists(TLogDirectory))
                Directory.CreateDirectory(TLogDirectory);

            // The source is modified during The test so,
            // make temporary copies

            File.Copy($@"{CurrentDirectory}\Tests\MultiDependencyRebuild\Input1.cpp",
                      $@"{CurrentDirectory}\{IntDir}\Input1.cpp");
            File.Copy($@"{CurrentDirectory}\Tests\MultiDependencyRebuild\Input2.cpp",
                      $@"{CurrentDirectory}\{IntDir}\Input2.cpp");
            File.Copy($@"{CurrentDirectory}\Tests\MultiDependencyRebuild\Input3.cpp",
                      $@"{CurrentDirectory}\{IntDir}\Input3.cpp");

            var file1 = new EmCxx {
                BuildEngine                  = engine,
                TrackerLogDirectory          = TLogDirectory,
                AdditionalIncludeDirectories = $@"{CurrentDirectory}\Tests\MultiDependencyRebuild\",
                ObjectFileName               = $@"{CurrentDirectory}\{IntDir}\Input1.cpp.o",
                DependencyFileName           = $@"{CurrentDirectory}\{IntDir}\Input1.cpp.d",
                GenerateDependencyFile       = true,
                Sources                      = new ITaskItem[] {
                    new TaskItem($@"{CurrentDirectory}\{IntDir}\Input1.cpp"),
                },
            };
            resultTasks.Add(file1);

            var file2 = new EmCxx {
                BuildEngine                  = engine,
                AdditionalIncludeDirectories = $@"{CurrentDirectory}\Tests\MultiDependencyRebuild\",
                TrackerLogDirectory          = TLogDirectory,
                ObjectFileName               = $@"{CurrentDirectory}\{IntDir}\Input2.cpp.o",
                DependencyFileName           = $@"{CurrentDirectory}\{IntDir}\Input2.cpp.d",
                GenerateDependencyFile       = true,
                Sources                      = new ITaskItem[] {
                    new TaskItem($@"{CurrentDirectory}\{IntDir}\Input2.cpp"),
                },
            };
            resultTasks.Add(file2);

            var file3 = new EmCxx {
                BuildEngine                  = engine,
                TrackerLogDirectory          = TLogDirectory,
                AdditionalIncludeDirectories = $@"{CurrentDirectory}\Tests\MultiDependencyRebuild\",
                ObjectFileName               = $@"{CurrentDirectory}\{IntDir}\Input3.cpp.o",
                DependencyFileName           = $@"{CurrentDirectory}\{IntDir}\Input3.cpp.d",
                GenerateDependencyFile       = true,
                Sources                      = new ITaskItem[] {
                    new TaskItem($@"{CurrentDirectory}\{IntDir}\Input3.cpp"),
                },
            };
            resultTasks.Add(file3);

            return resultTasks.ToArray();
        }

        private EmCxx[] SetupExecutable(EmptyBuildEngine engine)
        {
            var resultTasks = new List<EmCxx>();

            if (!Directory.Exists(TLogDirectory))
                Directory.CreateDirectory(TLogDirectory);

            var file1 = new EmCxx {
                BuildEngine            = engine,
                TrackerLogDirectory    = TLogDirectory,
                ObjectFileName         = $@"{CurrentDirectory}\{IntDir}\Main1.cpp.o",
                DependencyFileName     = $@"{CurrentDirectory}\{IntDir}\Main1.cpp.d",
                GenerateDependencyFile = true,
                Sources                = new ITaskItem[] {
                    new TaskItem($@"{CurrentDirectory}\Tests\MultiDependencyRebuild\Main1.cpp"),
                },
            };
            resultTasks.Add(file1);

            var file2 = new EmCxx {
                BuildEngine            = engine,
                TrackerLogDirectory    = TLogDirectory,
                ObjectFileName         = $@"{CurrentDirectory}\{IntDir}\Main.cpp.o",
                DependencyFileName     = $@"{CurrentDirectory}\{IntDir}\Main.cpp.d",
                GenerateDependencyFile = true,
                Sources                = new ITaskItem[] {
                    new TaskItem($@"{CurrentDirectory}\Tests\MultiDependencyRebuild\Main.cpp"),
                },
            };
            resultTasks.Add(file2);

            return resultTasks.ToArray();
        }

        private static void ReSaveFile(ITaskItem item)
        {
            File.WriteAllText(item.ItemSpec, File.ReadAllText(item.ItemSpec));
        }

        private static void SwapStringInFiles(EmCxx[] objects, string searchFor, string replaceWith)
        {
            foreach (var obj in objects)
            {
                var text = File.ReadAllText(obj.Sources[0].ItemSpec);
                File.WriteAllText(obj.Sources[0].ItemSpec, text.Replace(searchFor, replaceWith));
            }
        }

        private static void CompileObjectFiles(EmCxx[] objects)
        {
            // Make sure everything builds initially
            foreach (var obj in objects)
                ExecuteAndAssertUpToDate(obj);

            // Simulate a different time stamp by saving the file.
            foreach (var obj in objects)
                ReSaveFile(obj.Sources[0]);

            // Make sure everything is rebuilt
            foreach (var obj in objects)
                ExecuteAndAssertUpToDate(obj);

            // Simulate an out of date event by removing an object file.
            foreach (var obj in objects)
            {
                Assert.IsTrue(File.Exists(obj.ObjectFileName));
                File.Delete(obj.ObjectFileName);
                Assert.IsTrue(!File.Exists(obj.ObjectFileName));
            }

            // Make sure everything is rebuilt
            foreach (var obj in objects)
                ExecuteAndAssertUpToDate(obj);

            // Verify that it actually rebuilt it
            foreach (var obj in objects)
                Assert.IsTrue(File.Exists(obj.ObjectFileName));
        }

        private static ITaskItem[] MergeObjectFiles(EmCxx[] objects, EmAr lib = null)
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

        private static void ExecuteAndAssertUpToDate(EmTask task)
        {
            // build it twice. the first call should
            // build it, and the second call should
            // skip it because it's up to date..
            Assert.AreEqual(true, task.Execute());
            Assert.AreNotEqual(true, task.SkippedExecution);

            Assert.AreEqual(true, task.Execute());
            Assert.AreEqual(true, task.SkippedExecution);
        }

        [TestMethod]
        public void MultiDependencyRebuild()
        {
            ClearIfExists(IntDir);

            var engine = new EmptyBuildEngine();

            TLogSub     = "StaticObjectLogs";
            var objects = SetupStaticLibrary(engine);

            CompileObjectFiles(objects);

            var ar = new EmAr {
                BuildEngine         = engine,
                TrackerLogDirectory = TLogDirectory,
                OutputFile          = new TaskItem($@"{CurrentDirectory}\{IntDir}\libInput.a"),
                Sources             = MergeObjectFiles(objects),
            };
            ExecuteAndAssertUpToDate(ar);

            Assert.IsTrue(File.Exists(ar.OutputFile.ItemSpec));
            File.Delete(ar.OutputFile.ItemSpec);
            Assert.IsTrue(ar.Execute());
            Assert.IsTrue(File.Exists(ar.OutputFile.ItemSpec));

            TLogSub = "ExecutableLogs";

            var exeObjects = SetupExecutable(engine);
            CompileObjectFiles(exeObjects);

            var lnk = new EmLink {
                ConfigurationType   = "Application",
                BuildEngine         = engine,
                TrackerLogDirectory = TLogDirectory,
                OutputFile          = new TaskItem($@"{CurrentDirectory}\{IntDir}\mdr.wasm"),
                Sources             = MergeObjectFiles(exeObjects, ar),
            };

            ExecuteAndAssertUpToDate(lnk);

            Assert.IsTrue(File.Exists(lnk.OutputFile.ItemSpec));
            File.Delete(lnk.OutputFile.ItemSpec);
            Assert.IsTrue(!File.Exists(lnk.OutputFile.ItemSpec));

            Assert.IsTrue(lnk.Execute());
            Assert.IsTrue(!lnk.SkippedExecution);
            Assert.IsTrue(File.Exists(lnk.OutputFile.ItemSpec));

            // delete the archive and attempt to rebuild
            File.Delete(ar.OutputFile.ItemSpec);

            Assert.AreEqual(false,
                            lnk.Execute(),
                            "EmLink.Execute did not fail with invalid input");

            // regenerate the archive
            ExecuteAndAssertUpToDate(ar);

            // it should link again
            ExecuteAndAssertUpToDate(lnk);

            var output = Spawn(Wavm, $"run {lnk.OutputFile.ItemSpec}");

            Assert.AreEqual("Called SomePrototypeInTheExecutableSource\n" +
                                "Called Input1PrototypeInTheStaticLibrarySource\n" +
                                "Called Input2PrototypeInTheStaticLibrarySource\n" +
                                "Called Input3PrototypeInTheStaticLibrarySource\n",
                            output);

            // modify the source files and assert that every thing rebuilds

            SwapStringInFiles(objects, "PrototypeInTheStaticLibrarySource", "__Swapped_Text__");
            CompileObjectFiles(objects);
            ExecuteAndAssertUpToDate(ar);
            ExecuteAndAssertUpToDate(lnk);

            output = Spawn(Wavm, $"run {lnk.OutputFile.ItemSpec}");

            Assert.AreEqual("Called SomePrototypeInTheExecutableSource\n" +
                                "Called Input1__Swapped_Text__\n" +
                                "Called Input2__Swapped_Text__\n" +
                                "Called Input3__Swapped_Text__\n",
                            output);

            // swap it back to create a cycle
            SwapStringInFiles(objects, "__Swapped_Text__", "PrototypeInTheStaticLibrarySource");

            CompileObjectFiles(objects);
            ExecuteAndAssertUpToDate(ar);
            ExecuteAndAssertUpToDate(lnk);

            output = Spawn(Wavm, $"run {lnk.OutputFile.ItemSpec}");

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
