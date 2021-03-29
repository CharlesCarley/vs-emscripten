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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using EmscriptenTask;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Logger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;

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
        private static string CurrentDirectory => $@"{Environment.CurrentDirectory}\..\..\";

        [TestMethod]
        public void SanitizeTest()
        {
            var result = EmUtils.Sanitize("A//////////B//C//////D/E//////////////F");
            Assert.AreEqual(@"A\B\C\D\E\F", result);
        }


        [TestMethod]
        public void TestFileNameWithoutExtension()
        {
            var strings = new[]
            {
                "abc.123",
                "abc.def.hij.k",
                "/some/unix/_/path/abc.def.hij.k",
                @"Z:\some\windows\_\path\abc.def.hij.k",
                @"Z:\s o m e\w i n d o w s\_\p a t h\a b c.d e f.h i j.k",
                @"96587658765*&R8 T^RuyTR876R 87^5r8&^8&^%9^$#&%^$^$#%#^$^*&(07",
                @"96587658765*&R8 T^RuyTR876R/87^5r8&^8&^%9^$#&%^$^$#%#^$^*&(07",
                @"96587658765*&R8 T^RuyTR876R/87^5r8&^8&^%9^$#&%^$^$#%#^$^*&(07/file.txt",
            };

            var expected = new[]
            {
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



        private static void ClearIfExists()
        {
            var dir = CurrentDirectory + "Debug";
            if (!Directory.Exists(dir))
                return;

            try
            {
                Directory.Delete(dir, true);
            }
            catch (Exception ex)
            {
                Logger.LogMessage("{0}", ex.Message);
            }
        }

        [TestMethod]
        public void Create()
        {
            ClearIfExists();

            var sourceFile = $@"{CurrentDirectory}\Tests\New Folder\New Text Document.c";
            var objFile    = $@"{CurrentDirectory}\Debug\New Folder\t1.c.o";
            var wasmFile   = $@"{CurrentDirectory}\Debug\New Folder\t 1 2.wasm";
            var trackerDir = $@"{CurrentDirectory}\Debug\";

            if (!Directory.Exists(trackerDir))
                Directory.CreateDirectory(trackerDir);

            var be = new EmptyBuildEngine();

            var task = new EmCxx {
                BuildEngine         = be,
                TrackerLogDirectory = trackerDir,
                ObjectFileName      = objFile,
                Sources             = new ITaskItem[] { new TaskItem(sourceFile) },
            };
            Assert.IsTrue(task.Execute());

            var link = new EmLink {
                BuildEngine         = be,
                TrackerLogDirectory = trackerDir,
                OutputFile          = wasmFile,
                Sources             = new ITaskItem[] { new TaskItem(objFile) },
            };
            Assert.IsTrue(link.Execute());
        }

        [TestMethod]
        public void TestInvalidIntSwitchValue()
        {
            var a = new TestSwitch {
                Prop1 = 22
            };

            var builder = new StringWriter();
            EmSwitchWriter.Write(builder, a.GetType(), a);
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
                EmSwitchWriter.Write(builder, a.GetType(), a);
                if (i == 0 || i == 6)
                    Assert.AreEqual(string.Empty, builder.ToString());
                else
                    Assert.AreEqual($" -a={a.Prop1}", builder.ToString());
            }
        }





    }
}
