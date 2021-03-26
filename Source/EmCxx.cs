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
using Microsoft.Build.Utilities;
using System;
using System.IO;
using static EmscriptenTask.EmUtils;

namespace EmscriptenTask
{
    public class EmCxx : EmTask
    {
        protected override string SenderName     => nameof(EmCxx);
        protected override string _BuildFileName => BuildFile;

        /// <summary>
        /// An internal property that is set per source file right
        /// before calling ProcessFile.
        /// </summary>
        protected string BuildFile { get; set; }

        // ================================ General ================================

        /// <summary>
        /// Provides any extra user supplied include directories.
        /// </summary>
        public string AdditionalIncludeDirectories { get; set; }

        public string SystemIncludeDirectories { get; set; }

        /// <summary>
        /// Debug info options.
        /// </summary>
        public string DebugInformationFormat { get; set; }

        /// <summary>
        /// Warning verbosity level.
        /// A value of Default will do nothing and let the compiler emit the default messages.
        /// A value of None will pass -w.
        /// A value of All will pass -Wall.
        ///
        /// </summary>
        public string WarningLevel { get; set; }

        /// <summary>
        /// Generates an error instead of a warning.
        /// </summary>
        public bool TreatWarningAsError { get; set; }

        // ErrorLimit
        // TemplateBacktraceLimit

        // ============================= Optimization ==============================
        // OptimizationLevel
        // OmitFramePointers
        //

        // ============================= Preprocessor ==============================

        /// <summary>
        /// Provides any extra user supplied preprocessor definitions.
        /// </summary>
        public string PreprocessorDefinitions { get; set; }

        /// <summary>
        /// Extra system definitions.
        /// </summary>
        public string SystemPreprocessorDefinitions { get; set; }

        /// <summary>
        /// Do not pass any preprocessor definitions
        /// </summary>
        public bool UndefineAllPreprocessorDefinitions { get; set; } = false;

        // ============================= Code Generation ===========================
        // FunctionLevelLinking
        // DataLevelLinking
        // BufferSecurityCheck
        // PositionIndependentCode
        // UseShortEnums

        /// <summary>
        /// Set standard exception handling.
        /// A value of Disabled will disable exceptions.
        /// A value of Enabled will enable exceptions.
        /// </summary>
        public string ExceptionHandling { get; set; }

        // ============================= Language ==============================----
        // RuntimeTypeInfo
        // LanguageExtensions
        // EnableMicrosoftExtensions
        // ConstExprLimit
        // TemplateRecursionLimit

        /// <summary>
        /// Option to explicitly set the desired C++ standard version.
        /// </summary>
        public string LanguageStandard { get; set; }

        // ============================= Output Files ==============================
        // PreserveTempFiles
        // GenerateDependencyFile
        // DependencyFileName

        /// <summary>
        /// The output object file defined as $(OutDir)%(Filename).o
        /// </summary>
        public string ObjectFileName { get; set; }

        public bool GenerateDependencyFile { get; set; }

        public string DependencyFileName { get; set; }

        // =========================-==== Advanced  ==============================--
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
        public string CompileAs { get; set; }
        private bool  IsInC => CompileAs.Equals("CompileAsC");

        /// <summary>
        /// Output included files.
        /// </summary>
        public bool ShowIncludes { get; set; }

        // ============================= Command Line  =============================
        public string AdditionalOptions { get; set; }

        // ============================= Tracking =============================

        /// <summary>
        /// Test to determine whether or not the supplied source code
        /// should be compiled as c code
        /// </summary>
        /// <returns>
        /// True if the file should be compiled as c. Internally sets the CompileAs
        /// property to CompileAsC or CompileAsCpp
        /// </returns>
        private bool TestCompileAsC()
        {
            // use own test.
            var result = false;

            if (!string.IsNullOrEmpty(CompileAs))
            {
                if (CompileAs.Equals("Default"))
                {
                    if (BuildFile != null)
                    {
                        result = BuildFile.EndsWith(".c");
                        if (result)
                        {
                            CompileAs = "CompileAsC";
                        }
                    }
                    return result;
                }

                if (CompileAs.Equals("CompileAsC"))
                    return true;
                return false;
            }

            if (BuildFile != null)
            {
                result    = BuildFile.EndsWith(".c");
                CompileAs = result ? "CompileAsC" : "Default";
            }
            return result;
        }

        /// <summary>
        /// Builds a a space separated string of all command line parameters.
        /// </summary>
        /// <returns>The space separated string that should be passed to the process.</returns>
        protected string BuildSwitches()
        {
            var builder = new StringWriter();

            if (!string.IsNullOrEmpty(CompileAs))
            {
                if (CompileAs.Equals("CompileAsC"))
                    builder.Write(" -x c");
                else if (!CompileAs.Equals("CompileAsCpp"))
                    builder.Write(" -x c++");
            }

            if (!string.IsNullOrEmpty(ExceptionHandling))
            {
                switch (ExceptionHandling)
                {
                case "Disabled":
                    builder.Write(" -fno-exceptions");
                    break;
                case "Enabled":
                    builder.Write(" -fexceptions");
                    break;
                }
            }

            if (GenerateDependencyFile && !string.IsNullOrEmpty(DependencyFileName))
                builder.Write($" -MD -MF {DependencyFileName}");

            if (ShowIncludes)
                builder.Write(" -H");

            if (TreatWarningAsError)
                builder.Write(" -Werror");

            if (!string.IsNullOrEmpty(WarningLevel))
            {
                switch (WarningLevel)
                {
                case "None":
                    builder.Write(" -w");
                    break;
                case "All":
                    builder.Write(" -Wall");
                    break;
                }
            }

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

            if (!string.IsNullOrEmpty(DebugInformationFormat))
            {
                switch (DebugInformationFormat)
                {
                case "FullDebug":
                    builder.Write(" -g");
                    break;
                case "None":
                    builder.Write(" -g0");
                    break;
                }
            }

            // EmVerbose
            if (EmVerbose)
                builder.Write(" -s VERBOSE=1");

            // EmTracing
            if (EmTracing)
                builder.Write(" -s EMSCRIPTEN_TRACING=1");

            if (!string.IsNullOrEmpty(AdditionalOptions))
            {
                builder.Write(' ');
                builder.Write(AdditionalOptions);
            }

            if (!string.IsNullOrEmpty(SystemIncludeDirectories))
            {
                builder.Write(' ');
                builder.Write(SystemIncludeDirectories);
            }

            if (!string.IsNullOrEmpty(AdditionalIncludeDirectories))
            {
                builder.Write(' ');
                builder.Write(AdditionalIncludeDirectories);
            }

            if (!string.IsNullOrEmpty(SystemPreprocessorDefinitions))
            {
                builder.Write(' ');
                builder.Write(SystemPreprocessorDefinitions);
            }

            if (!string.IsNullOrEmpty(PreprocessorDefinitions))
            {
                builder.Write(' ');
                builder.Write(PreprocessorDefinitions);
            }

            if (!string.IsNullOrEmpty(AdditionalOptions))
            {
                builder.Write(' ');
                builder.Write(AdditionalOptions);
            }

            // BuildFile
            builder.Write($" -c {BuildFile}");
            builder.Write($" -o {ObjectFileName}");
            return builder.ToString();
        }

        private void TaskStarted()
        {
            // enabled by default if not set.
            if (string.IsNullOrEmpty(ExceptionHandling))
                ExceptionHandling = "Enabled";

            if (Verbose)
                LogTaskProps(GetType(), this);

            AdditionalIncludeDirectories  = SeparatePaths(AdditionalIncludeDirectories, ';', "-I", true);
            SystemIncludeDirectories      = SeparatePaths(SystemIncludeDirectories, ';', "-I");
            SystemPreprocessorDefinitions = SeparatePaths(SystemPreprocessorDefinitions, ';', "-D");

            if (!UndefineAllPreprocessorDefinitions)
                PreprocessorDefinitions = SeparatePaths(PreprocessorDefinitions, ';', "-D");
            else
                PreprocessorDefinitions = "";

            OutputFiles = new CanonicalTrackedOutputFiles(this, TLogWriteFiles);

            InputFiles = new CanonicalTrackedInputFiles(this,
                                                        TLogReadFiles,
                                                        Sources,
                                                        null,
                                                        OutputFiles,
                                                        MinimalRebuildFromTracking,
                                                        true);
        }

        private void TaskStopped(bool succeeded)
        {
            if (!succeeded)
                return;

            SaveTLogCommand();
            SaveTLogRead();
            OutputFiles.SaveTlog();
        }
        

        /// <summary>
        /// Makes sure that the output file name is absolute
        /// and that the intermediate directory exists.
        /// </summary>
        public void ValidateOutputFile()
        {
            var baseName = BaseName(ObjectFileName);
            var basePath = ObjectFileName.Replace(baseName, "");

            if (Sources.Length > 1)
                ObjectFileName = AbsolutePath($"{basePath}{BaseName(BuildFile)}.o");
            else
                ObjectFileName = AbsolutePath($"{basePath}{baseName}");

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
            BuildFile = AbsolutePath(file.ItemSpec);
            ValidateOutputFile();

            // Reflect the BaseName of the file currently being compiled.
            LogMessage(BaseName(BuildFile));

            OutputFiles.AddComputedOutputForSourceRoot(BuildFile.ToUpperInvariant(), ObjectFileName);

            var tool = EmccTool;
            if (!TestCompileAsC())
                tool = tool.Replace("emcc.bat", "em++.bat");

            return Call(tool, BuildSwitches());
        }

        /// <summary>
        /// Makes use of the dependency file output from -MD -MF - if it is available.
        /// </summary>
        /// <returns></returns>
        private string GetDependencyOutput()
        {
            if (!GenerateDependencyFile || string.IsNullOrEmpty(DependencyFileName))
                return null;

            var depFile = AbsolutePath(DependencyFileName);
            if (!File.Exists(depFile))
                return null;

            var builder = new StringWriter();

            var depFileLines = File.ReadAllLines(depFile);
            foreach (var depFileLine in depFileLines)
            {
                var cleanLine = depFileLine.TrimEnd("\\".ToCharArray()).Trim();

                if (!cleanLine.EndsWith(".o:"))
                    builder.WriteLine(cleanLine.ToUpperInvariant());
            }
            return builder.ToString();
        }

        private void SaveTLogRead()
        {
            var sourceFiles = InputFiles.ComputeSourcesNeedingCompilation();
            if (sourceFiles == null || sourceFiles.Length <= 0)
                return;

            var builder = new StringWriter();
            foreach (var source in sourceFiles)
            {
                builder.Write($"^{AbsolutePath(source.ItemSpec)}".ToUpperInvariant());
                builder.Write('\n');

                var extraDependencies = GetDependencyOutput();
                if (!string.IsNullOrEmpty(extraDependencies))
                    builder.Write(extraDependencies);
            }
            File.AppendAllText(TLogReadPathName, builder.ToString());
        }

        private void SaveTLogCommand()
        {
            var sourceFiles = InputFiles.ComputeSourcesNeedingCompilation();
            if (sourceFiles == null || sourceFiles.Length <= 0)
                return;

            var builder = new StringWriter();
            foreach (var source in sourceFiles)
            {
                builder.Write($"^{AbsolutePath(source.ItemSpec)}".ToUpperInvariant());
                builder.Write('\n');

                BuildFile = AbsolutePath(source.ItemSpec);
                ValidateOutputFile();
                builder.WriteLine(BuildSwitches());
            }

            File.AppendAllText(TLogCommandPathName, builder.ToString());
        }

        public override bool Run()
        {
            var list = InputFiles.ComputeSourcesNeedingCompilation();
            foreach (var file in list)
            {
                if (!string.IsNullOrEmpty(file.ItemSpec))
                {
                    if (!ProcessFile(file))
                        return false;
                }
                else
                {
                    LogError($"{SenderName}: no input files");
                    return false;
                }
            }
            return true;
        }

        public override void OnStart()
        {
            OnTaskStarted += TaskStarted;
            OnTaskStopped += TaskStopped;
        }
    }
}
