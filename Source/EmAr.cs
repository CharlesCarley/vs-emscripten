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
using System.IO;
using Microsoft.Build.Utilities;
using static EmscriptenTask.EmUtils;

namespace EmscriptenTask
{
    public class EmAr : EmTask
    {
        protected override string SenderName    => nameof(EmAr);
        protected override string BuildFileName => OutputFile;

        /// <summary>
        /// Output file name parameter $(OutDir)%(Filename).o
        /// </summary>
        public string OutputFile { get; set; }

        protected void TaskStarted()
        {
            if (Verbose)
                LogTaskProps(GetType(), this);

            OutputFiles = new CanonicalTrackedOutputFiles(this, TLogWriteFiles);

            InputFiles = new CanonicalTrackedInputFiles(this,
                                                        TLogReadFiles,
                                                        Sources,
                                                        null,
                                                        OutputFiles,
                                                        MinimalRebuildFromTracking,
                                                        true);
            OutputFile = AbsolutePath(OutputFile);
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

        private void TaskFinished(bool succeeded)
        {
            if (!succeeded)
                return;

            SaveTLogRead();
            OutputFiles.SaveTlog();
        }

        public bool RunAr()
        {
            var input = InputFiles.ComputeSourcesNeedingCompilation();
            if (input == null || input.Length <= 0)
                return true;

            var tool = EmccTool;
            tool = tool.Replace("emcc.bat", "emar.bat");

            OutputFiles.AddComputedOutputsForSourceRoot(OutputFile.ToUpperInvariant(), input);

            return Call(tool, $"qc {OutputFile} {GetSeparatedSource(' ', input)}");
        }

        public bool RunRanlib()
        {
            var input = InputFiles.ComputeSourcesNeedingCompilation();
            if (input == null || input.Length <= 0)
                return true;

            var tool = EmccTool;
            OutputFiles.AddComputedOutputForSourceRoot(OutputFile.ToUpperInvariant(), OutputFile);

            tool = tool.Replace("emcc.bat", "emranlib.bat");
            return Call(tool, OutputFile);
        }

        public override bool Run()
        {
            var result = RunAr();
            if (result)
                result = RunRanlib();
            return result;
        }

        public override void OnStart()
        {
            OnTaskStarted += TaskStarted;
            OnTaskStopped += TaskFinished;
        }
    }
}
