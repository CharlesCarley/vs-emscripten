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
using Microsoft.Build.Utilities;
using static EmscriptenTask.EmUtils;

namespace EmscriptenTask
{
    public class EmLink : EmTask
    {
        protected override string SenderName => nameof(EmLink);

        protected override string _BuildFileName => OutputFile;

        /// <summary>
        /// Output file name parameter $(OutDir)$(TargetName)$(TargetExt)
        /// </summary>
        public string OutputFile { get; set; }

        public string ConfigurationType { get; set; }

        /// <summary>
        /// User supplied libraries.
        /// </summary>
        public string AdditionalDependencies { get; set; }

        /// <summary>
        /// Extra library search paths.
        /// </summary>
        public string AdditionalLibraryDirectories { get; set; }

        /// <summary>
        /// Settings.js conversion, the EXPORT_NAME option
        /// </summary>
        public string EmExportName { get; set; }

        /// <summary>
        /// Settings.js conversion, the argument to WASM=[0,1,2]
        /// </summary>
        public string EmWasmMode { get; set; }

        /// <summary>
        /// Settings.js conversion, the argument to USE_SDL=[1,2]
        /// </summary>
        public string EmSDLVersion { get; set; }

        /// <summary>
        /// Settings.js conversion, the argument to FULL_ES3=[1]
        /// </summary>
        /// <returns></returns>
        ///
        public bool EmUseFullOpenGLES3 { get; set; }

        protected string BuildSwitches()
        {
            if (string.IsNullOrEmpty(OutputFile))
                throw new ArgumentNullException(nameof(OutputFile), "Missing OutputFile");

            var builder = new StringWriter();

            if (!string.IsNullOrEmpty(EmWasmMode))
            {
                switch (EmWasmMode)
                {
                case "EmWasmOnlyJS":
                    builder.Write("-s WASM=0");
                    break;
                case "EmWasmBoth":
                    builder.Write("-s WASM=2");
                    break;
                default:
                    builder.Write("-s WASM=1");  // only WebAssembly
                    break;
                }
            }
            else
                builder.Write("-s WASM=1");

            if (EmUseFullOpenGLES3)
                builder.Write(" -s FULL_ES3=1");

            if (!string.IsNullOrEmpty(EmSDLVersion))
            {
                int.TryParse(EmSDLVersion, out var sdlVersion);

                switch (sdlVersion)
                {
                case 1:
                case 2:
                    builder.Write(" -s USE_SDL=");
                    builder.Write(sdlVersion);
                    break;
                default:
                    throw new ArgumentException($"Invalid SDL version supplied {sdlVersion}");
                }
            }

            // write the input objects as a WS separated list
            var objects = GetSeparatedSource(' ', Sources);

            builder.Write(' ');
            builder.Write(objects);

            if (!string.IsNullOrEmpty(AdditionalLibraryDirectories))
            {
                var dirs = SeparatePaths(AdditionalLibraryDirectories, ';', "-L", true);
                builder.Write(' ');
                builder.Write(dirs);
            }

            if (!string.IsNullOrEmpty(AdditionalDependencies))
            {
                var libraries = SeparatePaths(AdditionalDependencies, ';', " ", true);
                builder.Write(' ');
                builder.Write(libraries);
            }

            builder.Write(' ');
            builder.Write("-o");
            builder.Write(OutputFile);
            SkippedExecution = false;
            return builder.ToString();
        }

        private void SaveTLogRead()
        {
            var sourceFiles = InputFiles.ComputeSourcesNeedingCompilation();
            if (sourceFiles == null || sourceFiles.Length <= 0)
                return;

            var filePath = TLogReadPathName;

            var text = string.Empty;
            if (File.Exists(filePath))
                text = File.ReadAllText(filePath);

            var builder = new StringWriter();
            foreach (var source in sourceFiles)
            {
                var tracked = $"^{AbsolutePath(source.ItemSpec)}".ToUpperInvariant();
                if (text.Contains(tracked))
                    continue;

                builder.Write(tracked);
                builder.Write('\n');
            }

            File.AppendAllText(filePath, builder.ToString());
        }

        protected void TaskStarted()
        {
            if (Verbose)
                LogTaskProps(GetType(), this);
            OutputFiles = new CanonicalTrackedOutputFiles(this, TLogWriteFiles);

            InputFiles = new CanonicalTrackedInputFiles(this,
                                                        TLogWriteFiles,
                                                        Sources,
                                                        null,
                                                        OutputFiles,
                                                        MinimalRebuildFromTracking,
                                                        true);

            OutputFile = AbsolutePath(OutputFile);
        }

        private void TaskFinished(bool succeeded)
        {
            if (!succeeded)
                return;

            SaveTLogRead();

            var input = InputFiles.ComputeSourcesNeedingCompilation();
            foreach (var inputFile in input)
            {
                var fileName   = inputFile.ItemSpec;
                var sourceRoot = OutputFile.ToUpperInvariant();
                OutputFiles.AddComputedOutputForSourceRoot(sourceRoot, fileName);
            }

            switch (ConfigurationType)
            {
            case "Application":
            {
                var swapName = FileNameWithoutExtension(OutputFile);
                OutputFiles.AddComputedOutputForSourceRoot(OutputFile, $"{swapName}.wasm");
                break;
            }
            case "HTMLApplication":
            {
                var swapName = FileNameWithoutExtension(OutputFile);
                OutputFiles.AddComputedOutputForSourceRoot(OutputFile, $"{swapName}.wasm");
                OutputFiles.AddComputedOutputForSourceRoot(OutputFile, $"{swapName}.html");
                OutputFiles.AddComputedOutputForSourceRoot(OutputFile, $"{swapName}.js");
                break;
            }
            }

            OutputFiles.SaveTlog();
        }

        public override bool Run()
        {
            var tool = EmccTool;
            tool     = tool.Replace("emcc.bat", "em++.bat");
            return Call(tool, BuildSwitches());
        }
        public override void OnStart()
        {
            OnTaskStarted += TaskStarted;
            OnTaskStopped += TaskFinished;
        }
    }
}
