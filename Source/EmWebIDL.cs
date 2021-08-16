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
    public class EmWebIDL : EmTask
    {
        protected override string SenderName => nameof(EmWebIDL);

        protected override string BuildFileName => OutputFile.GetMetadata(FullPath);

        // clang-format off

        [Required]
        public ITaskItem OutputFile { get; set; }

        [Required] 
        public string Configuration { get; set; }

        // clang-format on

        protected override void OnStart()
        {
            if (Verbose)
                LogTaskProps(GetType(), this);
        }

        public override bool Run()
        {
            var currentSource = GetCurrentSource();
            if (currentSource == null || currentSource.Length <= 0)
                return true;

            // EmWebIDLTool will add the *.cpp, so remove it now.
            // The compiled output should be back one directory as well.
            var outputFile = OutputFile.ItemSpec;
            outputFile     = outputFile.Replace($"{Configuration}\\", "");

            var extension = OutputFile.GetMetadata("Extension");

            if (!string.IsNullOrEmpty((extension)) &&
                outputFile.EndsWith(extension))
                outputFile = outputFile.Substring(0, outputFile.Length - extension.Length);

            outputFile = Sanitize(outputFile);
            return Call(Python, $"{EmWebIDLTool} {currentSource[0].ItemSpec} {outputFile}");
        }
    }
}
