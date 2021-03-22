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

namespace EmscriptenTask
{
    public class EmLink : EmTask
    {
        protected override string SenderName => nameof(EmLink);

        protected override string _BuildFileName => OutputFile;

        public string OutputFile { get; set; }

        public string AdditionalDependencies { get; set; }

        public string AdditionalLibraryDirectories { get; set; }

        public string EmExportName { get; set; }

        public string EmWasmMode { get; set; }

        public string EmSDLVersion { get; set; }

        protected string BuildSwitches()
        {
            if (string.IsNullOrEmpty(OutputFile))
                throw new ArgumentNullException("Missing OutputFile");

            StringWriter builder = new StringWriter();

            if (!string.IsNullOrEmpty(EmWasmMode))
            {
                if (EmWasmMode.Equals("EmWasmOnlyJS"))
                    builder.Write("-s WASM=0");
                else if (EmWasmMode.Equals("EmWasmBoth"))
                    builder.Write("-s WASM=2");
                else
                    builder.Write("-s WASM=1");  // only WebAssembly
            }
            else
                builder.Write("-s WASM=1");

            if (!string.IsNullOrEmpty(EmSDLVersion))
            {
                int SDLVer;
                int.TryParse(EmSDLVersion, out SDLVer);

                switch (SDLVer)
                {
                case 1:
                case 2:
                    builder.Write(' ');
                    builder.Write("-s USE_SDL=");
                    builder.Write(SDLVer);
                    break;
                default:
                    throw new ArgumentException($"Invalid SDL version supplied {SDLVer}");
                }
            }

            // write the input objects as a WS separated list
            var objects = EmUtils.GetSeperatedSource(' ', Sources);
            builder.Write(' ');
            builder.Write(objects);

            if (!string.IsNullOrEmpty(AdditionalLibraryDirectories))
            {
                var dirs = EmUtils.SeperatePaths(AdditionalLibraryDirectories, ';', "-L", true);
                builder.Write(' ');
                builder.Write(dirs);
            }

            if (!string.IsNullOrEmpty(AdditionalDependencies))
            {
                var libraries = EmUtils.SeperatePaths(AdditionalDependencies, ';', " ", true);
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
                LogTaskProps(GetType(), this);
        }

        public override bool Run()
        {
            TaskStarted();

            var tool     = EmccTool;
            tool         = tool.Replace("emcc.bat", "em++.bat");
            IEmTask task = this;
            return !task.Spawn(new ProcessStartInfo(tool) {
                CreateNoWindow         = true,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                WorkingDirectory       = Environment.CurrentDirectory,
                Arguments              = BuildSwitches(),
            });
        }
    }
}
