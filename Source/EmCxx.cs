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
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Utilities;
using static EmscriptenTask.EmUtils;
using Input  = Microsoft.Build.Utilities.CanonicalTrackedInputFiles;
using Output = Microsoft.Build.Utilities.CanonicalTrackedOutputFiles;

namespace EmscriptenTask
{
    public class EmCxx : EmTask
    {
        protected override string SenderName    => nameof(EmCxx);
        protected override string BuildFileName => BuildFile;

        // clang-format off

        /// <summary>
        /// An internal property that is set per source file right
        /// before calling ProcessFile.
        /// </summary>
        [StringSwitch("-c", BaseSwitch.RequiresValidation | BaseSwitch.QuoteIfWhiteSpace)]
        public string BuildFile { get; set; }
        
        /// <summary>
        /// Provides any extra user supplied include directories.
        /// </summary>
        [SeparatedStringSwitch("-I", BaseSwitch.RequiresValidation|BaseSwitch.QuoteIfWhiteSpace)]
        public string AdditionalIncludeDirectories { get; set; }

        //[SeparatedStringSwitch("-I", BaseSwitch.RequiresValidation | BaseSwitch.QuoteIfWhiteSpace)]
        public string SystemIncludeDirectories { get; set; }

        /// <summary>
        /// Debug info options.
        /// </summary>
        [EnumSwitch("None,FullDebug", "-g0,-g")]
        public string DebugInformationFormat { get; set; }

        /// <summary>
        /// Warning verbosity level.
        /// A value of Default will do nothing and let the compiler emit the default messages.
        /// A value of None will pass -w.
        /// A value of All will pass -Wall.
        /// </summary>
        [EnumSwitch("None,All", "-w,-Wall")]
        public string WarningLevel { get; set; }

        /// <summary>
        /// Generates an error instead of a warning.
        /// </summary>
        [BoolSwitch("-Werror")]
        public bool TreatWarningAsError { get; set; }

        [IntSwitch("-ferror-limit=")] 
        public string ErrorLimit { get; set; }

        [IntSwitch("-ftemplate-backtrace-limit=")] 
        public string TemplateBacktraceLimit { get; set; }

        [EnumSwitch("O0,O1,Os,O2,O3", "-O0,-O1,-Os,-O2,-O3")] 
        public string OptimizationLevel { get; set; }

        // OmitFramePointers

        /// <summary>
        /// Provides any extra user supplied preprocessor definitions.
        /// </summary>
        [SeparatedStringSwitch("-D", BaseSwitch.GlueSwitch)]
        public string PreprocessorDefinitions { get; set; }

        /// <summary>
        /// Extra system definitions.
        /// </summary>
        [SeparatedStringSwitch("-D", BaseSwitch.GlueSwitch)] 
        public string SystemPreprocessorDefinitions { get; set; }

        /// <summary>
        /// If true this will nullify any preprocessor definitions.
        /// </summary>
        public bool UndefineAllPreprocessorDefinitions { get; set; }

     
        
        /// <summary>
        /// Set standard exception handling.
        /// A value of Disabled will disable exceptions.
        /// A value of Enabled will enable exceptions.
        /// </summary>
        [EnumSwitch("Enabled,Disabled", "-fexceptions,-fno-exceptions")]
        public string ExceptionHandling { get; set; }

        /// <summary>
        /// Place each function in its own section.
        /// </summary>
        [BoolSwitch("-ffunction-sections")]
        public bool FunctionLevelLinking { get; set; }

        [BoolSwitch("-fdata-sections")] 
        public bool DataLevelLinking { get; set; }

        [BoolSwitch("-fstack-protector")] 
        public bool BufferSecurityCheck { get; set; }

        [BoolSwitch("-fpic")] 
        public bool PositionIndependentCode { get; set; }

        [BoolSwitch("-fshort-enums")] 
        public bool UseShortEnums { get; set; }

        // EnableMicrosoftExtensions
        // ConstExprLimit
        // TemplateRecursionLimit

        [BoolSwitch("-frtti")] 
        public bool RuntimeTypeInfo { get; set; }

        /// <summary>
        /// Option to explicitly set the desired C++ standard version.
        /// </summary>
        public string LanguageStandard { get; set; }

        /// <summary>
        ///
        /// </summary>
        [EnumSwitch("EnableLanguageExtensions,WarnLanguageExtensions,DisableLanguageExtensions",
            ",-pedantic,-pedantic-errors")]
        public string LanguageExtensions { get; set; }

        // PreserveTempFiles

        /// <summary>
        /// The output object file defined as $(OutDir)%(Filename).o
        /// </summary>
        [Required] 
        [StringSwitch("-o", BaseSwitch.QuoteIfWhiteSpace)] 
        public string ObjectFileName { get; set; }

        /// <summary>
        /// Set to true to output a dependency file.
        /// If this is set to false the DependencyFileName property
        /// will be set to null.
        /// </summary>
        public bool GenerateDependencyFile { get; set; }

        /// <summary>
        /// Specify the output file path for the generated dependency file.
        /// </summary>
        [StringSwitch("-MD -MF", BaseSwitch.QuoteIfWhiteSpace)]
        public string DependencyFileName { get; set; }

        // ForcedIncludeFiles
        // EnableSpecificWarnings
        // DisableSpecificWarnings
        // TreatSpecificWarningsAsErrors

        /// <summary>
        /// Explicit statement for setting how the source file should be compiled.
        ///
        /// if this is set to Default:
        ///     EmCxx will test the extension for a .c.
        ///     if that test passes the tool will be emcc.bat and
        ///     CompileAs will be set to 'CompileAsC' and add the -x c arguments.
        ///     If the extension is not '.c', the tool will be em++.bat
        /// if this is set to CompileAsC:
        ///     The tool will be emcc.bat with the -x c switch.
        /// if this is set to CompileAsCpp:
        ///     The tool will be em++.bat with the switch -x c++.
        ///
        /// </summary>
        [EnumSwitch("Default,CompileAsC,CompileAsCpp", ",-x c,-x c++")]
        public string CompileAs { get; set; }

        private bool IsInC => CompileAs.Equals("CompileAsC");

        /// <summary>
        /// Output included files.
        /// </summary>
        [BoolSwitch("-H")]
        public bool ShowIncludes { get; set; }

        [StringSwitch] 
        public string AdditionalOptions { get; set; }

        /// <summary>
        /// When set to 1, will generate more verbose output during compilation.
        /// </summary>
        [BoolSwitch("-s VERBOSE=1")]
        public bool EmVerbose { get; set; }
        /// <summary>
        /// Use Clang's undefined behavior sanitizer.
        /// </summary>
        [BoolSwitch("-fsanitize=undefined")]
        public bool EmUseUBSan { get; set; }

        /// <summary>
        /// Use Clang's address sanitizer.
        /// </summary>
        [BoolSwitch("-fsanitize=address")]
        public bool EmUseASan { get; set; }

        // clang-format on

        private bool TestCompileAsC()
        {
            // use own test.
            bool result;

            var testBuildFile = BaseName(BuildFile);

            if (!string.IsNullOrEmpty(CompileAs))
            {
                if (!CompileAs.Equals("Default"))
                    return CompileAs.Equals("CompileAsC");
                if (testBuildFile == null)
                    return false;
                result = testBuildFile.EndsWith(".c");
                if (result)
                    CompileAs = "CompileAsC";
                return result;
            }

            if (testBuildFile == null)
                return false;
            result    = testBuildFile.EndsWith(".c");
            CompileAs = result ? "CompileAsC" : "Default";
            return result;
        }

        protected string BuildSwitches()
        {
            var builder = new StringWriter();

            if (!string.IsNullOrEmpty(LanguageStandard))
            {
                switch (LanguageStandard)
                {
                case "stdc89" when IsInC:
                    builder.Write(" -std=c89");
                    break;
                case "stdc99" when IsInC:
                    builder.Write(" -std=c99");
                    break;
                case "stdc11" when IsInC:
                    builder.Write(" -std=c11");
                    break;
                case "gnuc99" when IsInC:
                    builder.Write(" -std=gnu99");
                    break;
                case "gnuc11" when IsInC:
                    builder.Write(" -std=gnu11");
                    break;
                case "stdcpp98" when !IsInC:
                    builder.Write(" -std=c++98");
                    break;
                case "stdcpp03" when !IsInC:
                    builder.Write(" -std=c++03");
                    break;
                case "stdcpp11" when !IsInC:
                    builder.Write(" -std=c++11");
                    break;
                case "stdcpp14" when !IsInC:
                    builder.Write(" -std=c++14");
                    break;
                case "stdcpp17" when !IsInC:
                    builder.Write(" -std=c++1z");
                    break;
                case "gnucpp98" when !IsInC:
                    builder.Write(" -std=gnu++98");
                    break;
                case "gnucpp11" when !IsInC:
                    builder.Write(" -std=gnu++11");
                    break;
                }
            }

            EmSwitchWriter.Write(builder, GetType(), this);
            return builder.ToString();
        }

        protected override void OnStart()
        {
            if (UndefineAllPreprocessorDefinitions)
                PreprocessorDefinitions = null;

            if (!GenerateDependencyFile)
                DependencyFileName = null;

            if (Verbose)
                LogTaskProps(GetType(), this);
        }

        public void ValidateOutputFile()
        {
            var baseName = BaseName(ObjectFileName);
            var basePath = Sanitize(ObjectFileName).Replace(baseName, string.Empty);

            ObjectFileName = Sources.Length > 1
                                 ? $"{basePath}{BaseName(BuildFile)}.o"
                                 : $"{basePath}{baseName}";

            if (string.IsNullOrEmpty(basePath))
                return;

            try
            {
                if (!Directory.Exists(basePath))
                    Directory.CreateDirectory(basePath);
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message);
            }
        }

        protected bool ProcessFile(ITaskItem file)
        {
            BuildFile = file.ItemSpec;
            ValidateOutputFile();

            LogMessage(BaseName(BuildFile));
            EmitOutputForInput(new TaskItem(ObjectFileName), file);

            var isC    = TestCompileAsC();
            var result = Call(isC ? EmccTool : EmCppTool, BuildSwitches());
            if (result)
                MergeDependencies(file);
            return result;
        }

        /// <summary>
        /// Makes use of the dependency file output from -MD -MF - if it is available.
        /// </summary>
        private void MergeDependencies(ITaskItem file)
        {
            var hasDep = string.IsNullOrEmpty(DependencyFileName);
            if (hasDep && !File.Exists(AbsolutePath(DependencyFileName)))
            {
                AddDependenciesForInput(file, null);
                return;
            }

            var depFile = AbsolutePath(DependencyFileName);
            var items   = new List<ITaskItem>();

            var depFileLines = File.ReadAllLines(depFile);
            foreach (var depFileLine in depFileLines)
            {
                var cleanLine = depFileLine.TrimEnd("\\".ToCharArray()).Trim();

                // skip the object that the
                // dependencies belong to.
                if (cleanLine.EndsWith(".o:"))
                    continue;

                if (File.Exists(cleanLine))
                    items.Add(new TaskItem(cleanLine));
            }

            AddDependenciesForInput(file, items.ToArray());
        }

        public override bool Run()
        {
            var list = GetCurrentSource();
            if (list == null || list.Length <= 0)
                return true;

            foreach (var file in list)
            {
                var filename = file.GetMetadata(FullPath);
                if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
                {
                    throw new FileNotFoundException(
                        $"{SenderName}: the file '{filename}' could not be found.");
                }
                if (!ProcessFile(file))
                    return false;
            }
            return true;
        }
    }
}
