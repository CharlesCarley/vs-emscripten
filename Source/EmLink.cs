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

        protected override string BuildFileName => OutputFile;
        public string             ConfigurationType { get; set; }

        // clang-format off

        /// <summary>
        /// Output file name parameter $(OutDir)$(TargetName)$(TargetExt)
        /// </summary>
        [StringSwitch("-o", true)]
        public string OutputFile { get; set; }

        /// <summary>
        /// User supplied libraries.
        /// </summary>
        [SeparatedStringSwitch(" ", true, true)]
        public string AdditionalDependencies { get; set; }

        /// <summary>
        /// Extra library search paths.
        /// </summary>
        [SeparatedStringSwitch("-L", true, true)]
        public string AdditionalLibraryDirectories { get; set; }

        [IntSwitch("-s ASSERTIONS=", new []{0,1,2})]
        public string EmAssertions { get; set; }

        /// <summary>
        /// Settings.js conversion, the EXPORT_NAME option
        /// </summary>
        [StringSwitch("-s EXPORT_NAME=")]
        public string EmExportName { get; set; }

        /// <summary>
        /// Settings.js conversion, the argument to WASM=[0,1,2]
        /// </summary>
        [EnumSwitch("EmWasmOnlyJS,EmWasmOnlyWasm,EmWasmBoth",
                    "-s WASM=0,-s WASM=1,-s WASM=2", 
                    "-s WASM=1")]
        public string EmWasmMode { get; set; }

        /// <summary>
        /// Settings.js conversion, the argument to USE_SDL=[1,2]
        /// </summary>
        [IntSwitch("-s USE_SDL=", new[] { 1, 2 })]
        public string EmSdlVersion { get; set; }

        [IntSwitch("-s MIN_WEBGL_VERSION=", new[] { 1, 2,3 })]
        public string EmMinWebGlVersion { get; set; }


        [IntSwitch("-s MAX_WEBGL_VERSION=", new[] { 1, 2, 3 })]
        public string EmMaxWebGlVersion { get; set; }

        /// <summary>
        /// Settings.js conversion, the argument to FULL_ES2=[1]
        /// </summary>
        /// <returns></returns>
        [BoolSwitch("-s FULL_ES2=1")]
        public bool EmUseFullOpenGles2 { get; set; }


        /// <summary>
        /// Settings.js conversion, the argument to FULL_ES3=[1]
        /// </summary>
        /// <returns></returns>
        [BoolSwitch("-s FULL_ES3=1")]
        public bool EmUseFullOpenGles3 { get; set; }

        // clang-format on

        protected string BuildSwitches()
        {
            if (string.IsNullOrEmpty(OutputFile))
                throw new ArgumentNullException(nameof(OutputFile), "Missing OutputFile");

            var builder = new StringWriter();

            // write the input objects as a WS separated list
            var objects = GetSeparatedSource(' ', Sources, true);
            builder.Write(' ');
            builder.Write(objects);

            EmSwitchWriter.Write(builder, GetType(), this);
            return builder.ToString();
        }
        
        protected override void OnStart()
        {
            OutputFiles = new CanonicalTrackedOutputFiles(this, TLogWriteFiles);

            InputFiles = new CanonicalTrackedInputFiles(this,
                                                        TLogReadFiles,
                                                        Sources,
                                                        null,
                                                        OutputFiles,
                                                        MinimalRebuildFromTracking,
                                                        true);

            OutputFile = AbsolutePathSanitized(OutputFile);
            if (Verbose)
                LogTaskProps(GetType(), this);
        }

        protected override void OnStop(bool succeeded)
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
    }
}
