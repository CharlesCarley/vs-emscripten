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
using Microsoft.Build.Framework;
using static EmscriptenTask.EmSwitchWriter;
using static EmscriptenTask.EmUtils;
using Input  = Microsoft.Build.Utilities.CanonicalTrackedInputFiles;
using Output = Microsoft.Build.Utilities.CanonicalTrackedOutputFiles;

namespace EmscriptenTask
{
    public class EmAr : EmTask
    {
        protected override string SenderName    => nameof(EmAr);
        protected override string BuildFileName => OutputFile.GetMetadata(FullPath);

        /// <summary>
        /// Output file name parameter $(OutDir)%(Filename).o
        /// </summary>
        [Required]
        [StringSwitch("qc", BaseSwitch.QuoteIfWhiteSpace)]
        public ITaskItem OutputFile { get; set; }


        [SeparatedStringSwitch(" ", BaseSwitch.QuoteIfWhiteSpace, ' ')]
        public ITaskItem[] AllSource => Sources;

        protected override void OnStart()
        {
            if (Verbose)
                LogTaskProps(GetType(), this);
        }

        protected string BuildSwitches()
        {
            if (OutputFile == null)
                throw new ArgumentNullException(nameof(OutputFile), "no output file");
            
            var builder = new StringWriter();
            Write(builder, GetType(), this);
            return builder.ToString();
        }

        public bool RunAr()
        {
            var input = GetCurrentSource();
            if (input == null || input.Length <= 0)
                return true;

            return Call(EmArTool, BuildSwitches());
        }

        public bool RunRanlib()
        {
            var input = GetCurrentSource();
            if (input == null || input.Length <= 0)
                return true;

            var result = Call(EmRanLibTool, OutputFile.ItemSpec);
            if (!result)
                return false;

            EmitOutputForInput(OutputFile, Sources);
            foreach (var src in Sources)
                AddDependenciesForInput(src, null);
            return true;
        }

        public override bool Run()
        {
            var result = RunAr();
            if (result)
                result = RunRanlib();
            return result;
        }
    }
}
