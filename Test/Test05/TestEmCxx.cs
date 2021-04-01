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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class TestEmCxx
    {
        [TestMethod]
        public void TestDefaults()
        {
            var obj = new EmscriptenTask.EmCxx();

            // EmTask Settings
            Assert.AreEqual(null, obj.BuildEngine);
            Assert.AreEqual(null, obj.HostObject);
            Assert.AreEqual(null, obj.TrackerLogDirectory);
            Assert.AreNotEqual(null, obj.TLogReadFiles);
            Assert.AreNotEqual(null, obj.TLogWriteFiles);
            Assert.AreEqual(@"EmCxx.read.1.tlog", obj.TLogReadFiles[0].ItemSpec);
            Assert.AreEqual(@"EmCxx.write.1.tlog", obj.TLogWriteFiles[0].ItemSpec);
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

            // EmCxx
            Assert.AreEqual(null, obj.BuildFile);
            Assert.AreEqual(null, obj.AdditionalIncludeDirectories);
            Assert.AreEqual(null, obj.SystemIncludeDirectories);
            Assert.AreEqual(null, obj.DebugInformationFormat);
            Assert.AreEqual(null, obj.WarningLevel);
            Assert.AreEqual(false, obj.TreatWarningAsError);
            Assert.AreEqual(null, obj.ErrorLimit);
            Assert.AreEqual(null, obj.TemplateBacktraceLimit);
            Assert.AreEqual(null, obj.PreprocessorDefinitions);
            Assert.AreEqual(null, obj.SystemPreprocessorDefinitions);
            Assert.AreEqual(false, obj.UndefineAllPreprocessorDefinitions);
            Assert.AreEqual(null, obj.ExceptionHandling);
            Assert.AreEqual(false, obj.FunctionLevelLinking);
            Assert.AreEqual(false, obj.DataLevelLinking);
            Assert.AreEqual(false, obj.BufferSecurityCheck);
            Assert.AreEqual(false, obj.PositionIndependentCode);
            Assert.AreEqual(false, obj.UseShortEnums);
            Assert.AreEqual(false, obj.RuntimeTypeInfo);
            Assert.AreEqual(null, obj.LanguageStandard);
            Assert.AreEqual(null, obj.LanguageExtensions);
            Assert.AreEqual(null, obj.ObjectFileName);
            Assert.AreEqual(false, obj.GenerateDependencyFile);
            Assert.AreEqual(null, obj.DependencyFileName);
            Assert.AreEqual(null, obj.CompileAs);
            Assert.AreEqual(false, obj.ShowIncludes);
            Assert.AreEqual(null, obj.AdditionalOptions);
        }

        [TestMethod]
        public void TestBuildFile()
        {
            var obj = new EmscriptenTask.EmCxx {
                BuildFile = "someFile.cpp"
            };
            var result1 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            obj.BuildFile = $@"{Environment.CurrentDirectory}\..\..\TestEmCxx.cs";
            var result2   = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual($" -c {obj.BuildFile}", result2);
        }


        [TestMethod]
        public void TestAdditionalIncludeDirectories()
        {
            var obj = new EmscriptenTask.EmCxx {
                AdditionalIncludeDirectories = "../foo bar"
            };
            var result1 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            var curDir = $@"{Environment.CurrentDirectory}\..\..\";

            obj.AdditionalIncludeDirectories = $@"{curDir};{curDir}\New Folder";

            var result2 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual($" -I {curDir} -I \"{curDir}New Folder\"", result2);
        }

        [TestMethod]
        public void TestDebugInformationFormat()
        {
            var obj = new EmscriptenTask.EmCxx {
                DebugInformationFormat = "An Invalid Value"
            };
            var result1 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            obj.DebugInformationFormat = "FullDebug";

            var result2 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -g", result2);

            obj.DebugInformationFormat = "None";

            var result3 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -g0", result3);
        }

        [TestMethod]
        public void TestWarningLevel()
        {
            var obj = new EmscriptenTask.EmCxx {
                WarningLevel = "An Invalid Value"
            };
            var result1 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            obj.WarningLevel = "None";

            var result2 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -w", result2);

            obj.WarningLevel = "All";

            var result3 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -Wall", result3);
        }

        [TestMethod]
        public void TestTreatWarningAsError()
        {
            var obj = new EmscriptenTask.EmCxx {
                TreatWarningAsError = false
            };

            // No value is defined for false, so, it should be skipped.
            var result1 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            obj.TreatWarningAsError = true;

            var result2 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -Werror", result2);
        }

        [TestMethod]
        public void TestErrorLimit()
        {
            var obj = new EmscriptenTask.EmCxx {
                ErrorLimit = ""
            };

            // No value is defined for false, so, it should be skipped.
            var result1 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            obj.ErrorLimit = "20";
            var result2    = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -ferror-limit=20", result2);
        }

        [TestMethod]
        public void TestTemplateBacktraceLimit()
        {
            var obj = new EmscriptenTask.EmCxx {
                TemplateBacktraceLimit = ""
            };

            // No value is defined for false, so, it should be skipped.
            var result1 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            obj.TemplateBacktraceLimit = "20";
            var result2                = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -ftemplate-backtrace-limit=20", result2);
        }

        [TestMethod]
        public void TestPreprocessorDefinitions()
        {
            var obj = new EmscriptenTask.EmCxx {
                PreprocessorDefinitions = string.Empty
            };

            // No value is defined for false, so, it should be skipped.
            var result1 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            obj.PreprocessorDefinitions = "A;B;C";
            var result2                 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -DA -DB -DC", result2);
        }

        [TestMethod]
        public void TestSystemPreprocessorDefinitions()
        {
            var obj = new EmscriptenTask.EmCxx {
                SystemPreprocessorDefinitions = string.Empty
            };

            // No value is defined for false, so, it should be skipped.
            var result1 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            obj.SystemPreprocessorDefinitions = "A;B;C";
            var result2                       = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -DA -DB -DC", result2);
        }

        [TestMethod]
        public void TestExceptionHandling()
        {
            var obj = new EmscriptenTask.EmCxx {
                ExceptionHandling = string.Empty
            };

            // No value is defined for false, so, it should be skipped.
            var result1 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            obj.ExceptionHandling = "Enabled";
            var result2           = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -fexceptions", result2);

            obj.ExceptionHandling = "Disabled";
            var result3           = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -fno-exceptions", result3);
        }

        [TestMethod]
        public void TestFunctionLevelLinking()
        {
            var obj = new EmscriptenTask.EmCxx {
                FunctionLevelLinking = false
            };

            // No value is defined for false, so, it should be skipped.
            var result1 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            obj.FunctionLevelLinking = true;

            var result2 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -ffunction-sections", result2);
        }

        [TestMethod]
        public void TestDataLevelLinking()
        {
            var obj = new EmscriptenTask.EmCxx {
                DataLevelLinking = false
            };

            // No value is defined for false, so, it should be skipped.
            var result1 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            obj.DataLevelLinking = true;

            var result2 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -fdata-sections", result2);
        }

        [TestMethod]
        public void TestBufferSecurityCheck()
        {
            var obj = new EmscriptenTask.EmCxx {
                BufferSecurityCheck = false
            };

            // No value is defined for false, so, it should be skipped.
            var result1 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            obj.BufferSecurityCheck = true;

            var result2 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -fstack-protector", result2);
        }

        [TestMethod]
        public void TestPositionIndependentCode()
        {
            var obj = new EmscriptenTask.EmCxx {
                PositionIndependentCode = false
            };

            // No value is defined for false, so, it should be skipped.
            var result1 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            obj.PositionIndependentCode = true;

            var result2 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -fpic", result2);
        }

        [TestMethod]
        public void TestUseShortEnums()
        {
            var obj = new EmscriptenTask.EmCxx {
                UseShortEnums = false
            };

            // No value is defined for false, so, it should be skipped.
            var result1 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            obj.UseShortEnums = true;

            var result2 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -fshort-enums", result2);
        }

        [TestMethod]
        public void TestRuntimeTypeInfo()
        {
            var obj = new EmscriptenTask.EmCxx {
                RuntimeTypeInfo = false
            };

            // No value is defined for false, so, it should be skipped.
            var result1 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            obj.RuntimeTypeInfo = true;

            var result2 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -frtti", result2);
        }

        [TestMethod]
        public void TestLanguageExtensions()
        {
            var obj = new EmscriptenTask.EmCxx {
                LanguageExtensions = null
            };

            var result1 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            obj.LanguageExtensions = "An Unknown Value";

            var result2 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result2);

            obj.LanguageExtensions = "EnableLanguageExtensions";

            // default
            var result3 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result3);

            obj.LanguageExtensions = "WarnLanguageExtensions";
            var result4          = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -pedantic", result4);

            obj.LanguageExtensions = "DisableLanguageExtensions";
            var result5          = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -pedantic-errors", result5);
        }



        [TestMethod]
        public void TestObjectFileName()
        {
            var obj = new EmscriptenTask.EmCxx
            {
                ObjectFileName = "someFile.o"
            };

            // not required to be valid initially
            var result1 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -o someFile.o", result1);

            obj.ObjectFileName = $@"{Environment.CurrentDirectory}\..\..\TestEmCxx.cs.o";
            var result2 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual($" -o {obj.ObjectFileName}", result2);
        }

        [TestMethod]
        public void TestDependencyFileName()
        {
            var obj = new EmscriptenTask.EmCxx
            {
                DependencyFileName = "someFile.d"
            };

            // not required to be valid initially
            var result1 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -MD -MF someFile.d", result1);

            obj.DependencyFileName = $@"{Environment.CurrentDirectory}\..\..\TestEmCxx.cs.d";
            var result2 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual($" -MD -MF {obj.DependencyFileName}", result2);
        }



        [TestMethod]
        public void TestCompileAs()
        {
            var obj = new EmscriptenTask.EmCxx
            {
                CompileAs = null
            };

            var result1 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            obj.CompileAs = "An Unknown Value";

            var result2 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result2);

            obj.CompileAs = "Default";

            // default
            var result3 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result3);

            obj.CompileAs = "CompileAsC";
            var result4 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -x c", result4);

            obj.CompileAs = "CompileAsCpp";
            var result5 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -x c++", result5);
        }


        [TestMethod]
        public void TestShowIncludes()
        {
            var obj = new EmscriptenTask.EmCxx
            {
                ShowIncludes = false
            };

            // No value is defined for false, so, it should be skipped.
            var result1 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(string.Empty, result1);

            obj.ShowIncludes = true;

            var result2 = TestUtils.WriteSwitchesToString(obj);
            Assert.AreEqual(" -H", result2);
        }

    }
}
