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
            Assert.AreEqual(@"emcc.read.1.tlog", obj.TLogReadFiles[0].ItemSpec);
            Assert.AreEqual(@"emcc.write.1.tlog", obj.TLogWriteFiles[0].ItemSpec);
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
    }
}
