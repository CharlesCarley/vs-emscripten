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
    public class EmAr : EmTask
    {
        protected override string SenderName    => nameof(EmAr);

        protected override string _BuildFileName => OutputFile;

        /// <summary>
        /// Output file name parameter $(OutDir)%(Filename).o
        /// </summary>
        public string OutputFile { get; set; }

        protected void TaskStarted()
        {
            if (Verbose)
                LogTaskProps(GetType(), this);
        }

        public bool RunAr()
        {
            var tool     = EmccTool;
            tool         = tool.Replace("emcc.bat", "emar.bat");
            IEmTask task = this;
            return !task.Spawn(new ProcessStartInfo(tool) {
                CreateNoWindow         = true,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                WorkingDirectory       = Environment.CurrentDirectory,
                Arguments              = $"qc {OutputFile} {EmUtils.GetSeperatedSource(' ', Sources)}",
            });
        }

        public bool RunRanlib()
        {
            var tool     = EmccTool;
            tool         = tool.Replace("emcc.bat", "emranlib.bat");
            IEmTask task = this;
            return !task.Spawn(new ProcessStartInfo(tool) {
                CreateNoWindow         = true,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                WorkingDirectory       = Environment.CurrentDirectory,
                Arguments              = $"{OutputFile}",
            });
        }

        public override bool Run()
        {
            TaskStarted();

            bool result = RunAr();
            if (result)
            {
                result = RunRanlib();
                if (result)
                    SkippedExecution = false;
            }
            return result;
        }
    }
}
