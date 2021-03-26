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
            var objects = EmUtils.GetSeparatedSource(' ', Sources);

            builder.Write(' ');
            builder.Write(objects);

            if (!string.IsNullOrEmpty(AdditionalLibraryDirectories))
            {
                var dirs = EmUtils.SeparatePaths(AdditionalLibraryDirectories, ';', "-L", true);
                builder.Write(' ');
                builder.Write(dirs);
            }

            if (!string.IsNullOrEmpty(AdditionalDependencies))
            {
                var libraries = EmUtils.SeparatePaths(AdditionalDependencies, ';', " ", true);
                builder.Write(' ');
                builder.Write(libraries);
            }

            builder.Write(' ');
            builder.Write("-o");
            builder.Write(OutputFile);
            SkippedExecution = false;
            return builder.ToString();
        }

        protected void TaskStarted()
        {
            if (Verbose)
            {
                LogTaskProps(GetType(), this);
            }
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
        }
    }
}
