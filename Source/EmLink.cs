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
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using static EmscriptenTask.EmSwitchWriter;
using static EmscriptenTask.EmUtils;
using Input  = Microsoft.Build.Utilities.CanonicalTrackedInputFiles;
using Output = Microsoft.Build.Utilities.CanonicalTrackedOutputFiles;

namespace EmscriptenTask
{
    public class EmLink : EmTask
    {
        protected override string SenderName => nameof(EmLink);

        protected override string BuildFileName => OutputFile.GetMetadata(FullPath);
        public string             ConfigurationType { get; set; }

        // clang-format off

        /// <summary>
        /// Output file name parameter $(OutDir)$(TargetName)$(TargetExt)
        /// </summary>
        [Required]
        [StringSwitch("-o", BaseSwitch.QuoteIfWhiteSpace)]
        public ITaskItem OutputFile { get; set; }

        /// <summary>
        /// User supplied libraries.
        /// </summary>
        [SeparatedStringSwitch(" ", BaseSwitch.RequiresValidation | BaseSwitch.QuoteIfWhiteSpace)]
        public string AdditionalDependencies { get; set; }

        /// <summary>
        /// Extra library search paths.
        /// </summary>
        [SeparatedStringSwitch("-L", BaseSwitch.RequiresValidation | BaseSwitch.QuoteIfWhiteSpace)]
        public string AdditionalLibraryDirectories { get; set; }

        /// <summary>
        /// Extra library search paths.
        /// </summary>
        [StringSwitch(" ")]
        public string AdditionalOptions { get; set; }


        /// <summary>
        /// Settings.js conversion, the argument to WASM=[0,1,2]
        /// </summary>
        [EnumSwitch("EmWasmOnlyJS,EmWasmOnlyWasm,EmWasmBoth",
                    "-s WASM=0,-s WASM=1,-s WASM=2", 
                    "-s WASM=1")]
        public string EmWasmMode { get; set; }

        /// <summary>
        /// Sets the SDL version number
        /// </summary>
        [IntSwitch("-s USE_SDL=", new[]{1,2})]
        public string EmSdlVersion { get; set; }

        /// <summary>
        /// Enables OpenGL ES 2
        /// </summary>
        [BoolSwitch ("-s FULL_ES2=1")]
        public bool EmUseFullOpenGles2 { get; set; }

        /// <summary>
        /// Enables OpenGL ES 3
        /// </summary>
        [BoolSwitch("-s FULL_ES3=1")]
        public bool EmUseFullOpenGles3 { get; set; }

        /// <summary>
        /// Sets the minimum WebGL version
        /// </summary>
        [IntSwitch("-s MIN_WEBGL_VERSION=", new[] { 1,2 })]
        public string EmMinWebGlVersion { get; set; }

        /// <summary>
        /// Sets the minimum WebGL version
        /// </summary>
        [IntSwitch("-s MAX_WEBGL_VERSION=", new[] { 1, 2 })]
        public string EmMaxWebGlVersion { get; set; }

        [SeparatedStringSwitch(" ", BaseSwitch.RequiresValidation | BaseSwitch.QuoteIfWhiteSpace, ' ')]
        public ITaskItem[] AllSource => Sources;

        // clang-format on

        protected string BuildSwitches()
        {
            if (OutputFile == null)
                throw new ArgumentNullException(nameof(OutputFile), "no output file");

            var builder = new StringWriter();
            Write(builder, GetType(), this);
            return builder.ToString();
        }

        protected override void OnStart()
        {
            if (Verbose)
                LogTaskProps(GetType(), this);
        }

        private ITaskItem[] MergeInputs()
        {
            var items = new List<ITaskItem>();
            items.AddRange(Sources);

            if (!string.IsNullOrEmpty(AdditionalDependencies))
            {
                var additionalDependencies = StringToTaskItemList(AdditionalDependencies);
                items.AddRange(additionalDependencies);
            }
            return items.ToArray();
        }

        private ITaskItem[] _mergedInputs;
        private ITaskItem[] MergedInputs => _mergedInputs ?? (_mergedInputs = MergeInputs());

        void EmitOutput()
        {
            switch (ConfigurationType)
            {
            case "HTMLApplication":
                var swapName = OutputFile.GetMetadata(Filename);
                EmitOutputForInput(OutputFile, MergedInputs);
                EmitOutputForInput(new TaskItem($"{swapName}.html"), MergedInputs);
                EmitOutputForInput(new TaskItem($"{swapName}.js"), MergedInputs);
                break;
            case "Application":
                EmitOutputForInput(OutputFile, MergedInputs);
                break;
            }
        }

        public override bool Run()
        {
            var currentSource = GetCurrentSource();
            if (currentSource == null || currentSource.Length <= 0)
                return true;

            var result = Call(EmCppTool, BuildSwitches());
            if (result)
            {
                EmitOutput();
                foreach (var inp in MergedInputs)
                    AddDependenciesForInput(inp, null);
            }
            return result;
        }
    }
}
