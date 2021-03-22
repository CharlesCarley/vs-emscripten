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
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using static EmscriptenTask.EmUtils;

namespace EmscriptenTask
{
    public class EmCxx : EmTask
    {
        protected override string SenderName     => nameof(EmCxx);
        protected override string _BuildFileName => BuildFile;

        public string AdditionalOptions { get; set; }

        /// <summary>
        /// An internal property that is set per source file right
        /// before calling ProcessFile.
        /// </summary>
        protected string BuildFile { get; set; }

        /// <summary>
        /// Provides any extra user supplied include directories.
        /// </summary>
        public string AdditionalIncludeDirectories { get; set; }

        /// <summary>
        /// Provides any extra user supplied preprocessor definitions.
        /// </summary>
        public string PreprocessorDefinitions { get; set; }

        /// <summary>
        /// Do not pass any preprocessor definitions
        /// </summary>
        public bool UndefineAllPreprocessorDefinitions { get; set; } = false;

        /// <summary>
        /// Extra system definitions.
        /// </summary>
        public string SystemPreprocessorDefinitions { get; set; }

        /// <summary>
        /// Debug info options.
        /// </summary>
        public string DebugInformationFormat { get; set; }

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

        private bool _IsInC => CompileAs.Equals("CompileAsC");

        /// <summary>
        /// The output object file defined as $(OutDir)%(Filename).o
        /// </summary>
        public string ObjectFileName { get; set; } = null;

        /// <summary>
        /// Option to explicitly set the desired C++ standard version.
        /// </summary>
        public string LanguageStandard { get; set; }

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

        /// <summary>
        /// Set standard exception handling.
        /// A value of Disabled will disable exceptions.
        /// A value of Enabled will enable exceptions.
        /// </summary>
        public string ExceptionHandling { get; set; }

        /// <summary>
        /// Output included files.
        /// </summary>
        public bool ShowIncludes { get; set; }

        /// <summary>
        /// Test to determine whether or not the supplied source code
        /// should be compiled as c code
        /// </summary>
        /// <returns>
        /// True if the file should be compiled as c. Internally sets the CompileAs
        /// property to CompileAsC or CompileAsCpp
        /// </returns>
        bool TestCompileAsC()
        {
            if (!string.IsNullOrEmpty(CompileAs))
            {
                if (CompileAs.Equals("Default"))
                {
                    // use our own test.
                    bool result = false;
                    if (BuildFile != null)
                    {
                        result = BuildFile.EndsWith(".c");
                        if (result)
                            CompileAs = "CompileAsC";
                    }
                    return result;
                }
                else if (CompileAs.Equals("CompileAsC"))
                    return true;
                else
                    return false;
            }
            else
            {
                // use own test.
                bool result = false;
                if (BuildFile != null)
                {
                    result = BuildFile.EndsWith(".c");
                    if (result)
                        CompileAs = "CompileAsC";
                    else
                        CompileAs = "CompileAsCpp";
                }
                return result;
            }
        }

        /// <summary>
        /// Builds a a space separated string of all command line parameters.
        /// </summary>
        /// <returns>The space separated string that should be passed to the process.</returns>
        protected string BuildSwitches()
        {
            StringWriter builder = new StringWriter();

            if (!string.IsNullOrEmpty(CompileAs))
            {
                if (CompileAs.Equals("CompileAsC"))
                {
                    builder.Write(' ');
                    builder.Write("-x c");
                }
                else
                {
                    builder.Write(' ');
                    builder.Write("-x c++");
                }
            }

            if (!string.IsNullOrEmpty(ExceptionHandling))
            {
                if (ExceptionHandling.Equals("Disabled"))
                {
                    builder.Write(' ');
                    builder.Write("-fno-exceptions");
                }
                else
                {
                    builder.Write(' ');
                    builder.Write("-fexceptions");
                }
            }

            if (ShowIncludes)
            {
                builder.Write(' ');
                builder.Write("-H");
            }

            if (TreatWarningAsError)
            {
                builder.Write(' ');
                builder.Write("-Werror");
            }

            if (!string.IsNullOrEmpty(WarningLevel))
            {
                if (WarningLevel.Equals("None"))
                {
                    builder.Write(' ');
                    builder.Write("-w");
                }
                else if (WarningLevel.Equals("All"))
                {
                    builder.Write(' ');
                    builder.Write("-Wall");
                }
            }

            if (!string.IsNullOrEmpty(LanguageStandard))
            {
                builder.Write(' ');
                if (LanguageStandard.Equals("stdc89") && _IsInC)
                    builder.Write("-std=c89");
                else if (LanguageStandard.Equals("stdc99") && _IsInC)
                    builder.Write("-std=c99");
                else if (LanguageStandard.Equals("stdc11") && _IsInC)
                    builder.Write("-std=c11");
                else if (LanguageStandard.Equals("gnuc99") && _IsInC)
                    builder.Write("-std=gnu99");
                else if (LanguageStandard.Equals("gnuc11") && _IsInC)
                    builder.Write("-std=gnu11");
                else if (LanguageStandard.Equals("stdcpp98") && !_IsInC)
                    builder.Write("-std=c++98");
                else if (LanguageStandard.Equals("stdcpp03") && !_IsInC)
                    builder.Write("-std=c++03");
                else if (LanguageStandard.Equals("stdcpp11") && !_IsInC)
                    builder.Write("-std=c++11");
                else if (LanguageStandard.Equals("stdcpp14") && !_IsInC)
                    builder.Write("-std=c++14");
                else if (LanguageStandard.Equals("stdcpp17") && !_IsInC)
                    builder.Write("-std=c++1z");
                else if (LanguageStandard.Equals("gnucpp98") && !_IsInC)
                    builder.Write("-std=gnu++98");
                else if (LanguageStandard.Equals("gnucpp11") && !_IsInC)
                    builder.Write("-std=gnu++11");
            }

            if (!string.IsNullOrEmpty(DebugInformationFormat))
            {
                builder.Write(' ');
                if (DebugInformationFormat.Equals("FullDebug"))
                    builder.Write("-g");
                else if (DebugInformationFormat.Equals("None"))
                    builder.Write("-g0");
            }

            // EmVerbose
            if (EmVerbose)
            {
                builder.Write(' ');
                builder.Write("-s VERBOSE=1");
            }

            // EmTracing
            if (EmTracing)
            {
                builder.Write(' ');
                builder.Write("-s EMSCRIPTEN_TRACING=1");
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
            builder.Write(' ');
            builder.Write($"-c {BuildFile}");

            builder.Write(' ');
            builder.Write($"-o {ObjectFileName}");
            return builder.ToString();
        }

        protected void TaskStarted()
        {
            if (Verbose)
                LogTaskProps(GetType(), this);

            AdditionalIncludeDirectories  = SeperatePaths(AdditionalIncludeDirectories, ';', "-I", true);
            SystemPreprocessorDefinitions = SeperatePaths(SystemPreprocessorDefinitions, ';', "-D");

            if (!UndefineAllPreprocessorDefinitions)
                PreprocessorDefinitions = SeperatePaths(PreprocessorDefinitions, ';', "-D");
            else
                PreprocessorDefinitions = "";
        }

        protected List<string> GetFileList()
        {
            if (string.IsNullOrEmpty(Sources))
                throw new NullReferenceException($"{SenderName}: no input files");
            return new List<string>(Sources.Split(';'));
        }

        public void ValidateOutputFile()
        {
            var baseName = BaseName(ObjectFileName);
            var basePath = ObjectFileName.Replace(baseName, "");

            if (Sources.Contains(";"))
                ObjectFileName = basePath + BaseName(BuildFile) + ".o";

            if (!string.IsNullOrEmpty(basePath))
            {
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
        }

        protected bool ProcessFile(string file)
        {
            BuildFile = file;

            LogMessage(BaseName(BuildFile));
            ValidateOutputFile();

            var tool = EmccTool;
            if (!TestCompileAsC())
                tool = tool.Replace("emcc.bat", "em++.bat");

            IEmTask task = this;
            return task.Spawn(
                new ProcessStartInfo(tool) {
                    CreateNoWindow         = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute        = false,
                    WorkingDirectory       = Environment.CurrentDirectory,
                    Arguments              = BuildSwitches(),
                });
        }

        public override bool Run()
        {
            TaskStarted();
            SkippedExecution = false;

            var list = GetFileList();
            foreach (string file in list)
            {
                if (!string.IsNullOrEmpty(file))
                {
                    if (ProcessFile(file))
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
    }
}
