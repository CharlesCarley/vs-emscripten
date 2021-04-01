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
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using static EmscriptenTask.EmUtils;
using Input  = Microsoft.Build.Utilities.CanonicalTrackedInputFiles;
using Output = Microsoft.Build.Utilities.CanonicalTrackedOutputFiles;

namespace EmscriptenTask
{
    public abstract class EmTask : IEmTask
    {
        private const int TimeOut = 30000;

        protected abstract string SenderName { get; }

        /// <summary>
        /// Internal access to the current input file for logging.
        /// </summary>
        protected abstract string BuildFileName { get; }

        public IBuildEngine BuildEngine { get; set; }

        public ITaskHost HostObject { get; set; }

        // ========================= Tracking ======================================
        private ITaskItem[] _tLogReadFiles;
        private ITaskItem[] _tLogWriteFiles;
        private string _trackerLogDirectory;

        protected string TLogWritePathName   => $@"{TrackerLogDirectory}{SenderName}.write.1.tlog";
        protected string TLogReadPathName    => $@"{TrackerLogDirectory}{SenderName}.read.1.tlog";
        protected string TLogCommandPathName => $@"{TrackerLogDirectory}{SenderName}.command.1.tlog";

        [Required]
        public string TrackerLogDirectory
        {
            get => _trackerLogDirectory;
            set {
                _trackerLogDirectory = value;

                // force them to change
                _tLogReadFiles  = null;
                _tLogWriteFiles = null;

                if (string.IsNullOrEmpty(_trackerLogDirectory))
                    return;

                // Make sure that the directory ends with a \
                // Other areas assume this!
                if (!_trackerLogDirectory.EndsWith("\\"))
                    _trackerLogDirectory += "\\";

                _trackerLogDirectory = Sanitize(_trackerLogDirectory);
            }
        }

        public ITaskItem[] TLogReadFiles
        {
            get {
                if (_tLogReadFiles == null || _tLogReadFiles.Length <= 0)
                {
                    _tLogReadFiles = new ITaskItem[] {
                        new TaskItem(TLogReadPathName)
                    };
                }

                return _tLogReadFiles;
            }
            set => _tLogReadFiles = value;
        }

        public ITaskItem[] TLogWriteFiles
        {
            get {
                if (_tLogWriteFiles == null || _tLogWriteFiles.Length <= 0)
                {
                    _tLogWriteFiles = new ITaskItem[] {
                        new TaskItem(TLogWritePathName)
                    };
                }

                return _tLogWriteFiles;
            }
            set => _tLogWriteFiles = value;
        }
        // clang-format off

        /// <summary>
        /// The input source files.
        /// </summary>
        [Required] 
        public ITaskItem[] Sources { get; set; }

        public bool MinimalRebuildFromTracking { get; set; } = true;

        private Input _inputFiles;
        private Output _outputFiles;
        private ITaskItem[] _currentSources;


        // ========================= Temporary  ====================================

        // Temporary extra debug properties
        public string DebugProp1 { get; set; }
        public string DebugProp2 { get; set; }
        public bool   DebugProp3 { get; set; }

        // ========================== General ======================================

        /// <summary>
        /// Indicator to determine build state. Used in Toolset.targets
        /// </summary>
        [Output] public bool SkippedExecution { get; set; }

        /// <summary>
        /// This should be the upstream/emscripten directory in the emsdk.
        /// </summary>
        public string EmscriptenDirectory => EmUtils.EmscriptenDirectory;

        /// <summary>
        /// This should be the full path to the $(EmscriptenDirectory)\emcc.bat batch file.
        /// </summary>
        public string EmccTool => EmUtils.EmccTool;

        /// <summary>
        /// If this is set to true, this will enable task logging.
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// If this is set to true, the contents of the command line should be
        /// logged to stdout.
        /// </summary>
        public bool EchoCommandLines { get; set; }

        // clang-format on

        /// <summary>
        /// The main verbose log function.
        /// Outputs the value of all readable properties.
        /// </summary>
        /// <param name="type">The class type to log.</param>
        /// <param name="inst">The current instance of the type. </param>
        protected void LogTaskProps(Type type, object inst)
        {
            var fields = type.GetProperties();
            var maxLen = 0;
            foreach (var field in fields)
            {
                if (!field.CanRead)
                    continue;

                if (maxLen < field.Name.Length)
                    maxLen = field.Name.Length;
            }

            LogSeparator(SenderName);

            foreach (var field in fields)
            {
                if (!field.CanRead)
                    continue;

                var nameLen = maxLen + 1 - field.Name.Length;
                var ws      = string.Empty;
                if (nameLen > 0)
                    ws = new string(' ', nameLen);

                LogMessage($"{field.Name}{ws}: {field.GetValue(inst)}");
            }
        }

        protected void LogSeparator(string message)
        {
            if (message.Length > 45)
                message = message.Substring(0, 20);

            var a = new string('=', 35);
            var b = new string('=', 45 - message.Length);
            LogMessage($"{a} {message} {b}");
        }

        protected void LogMessage(string message)
        {
            BuildEngine.LogMessageEvent(
                new BuildMessageEventArgs(
                    message ?? "null",       // message
                    string.Empty,            // help keyword
                    SenderName,              // sender
                    MessageImportance.High,  // importance
                    DateTime.Now             // date and time
                    ));
        }

        protected void LogWarning(string message, int line = 0, int column = 0)
        {
            BuildEngine.LogWarningEvent(
                new BuildWarningEventArgs(
                    SenderName,         // subcategory
                    string.Empty,       // code
                    BuildFileName,      // file
                    line,               // line number
                    column,             // column number
                    0,                  // end line number
                    0,                  // end column number
                    message ?? "null",  // message
                    string.Empty,       // help keyword
                    SenderName,         // sender
                    DateTime.Now        // date and time
                    ));
        }
        protected void LogError(string message, int line = 0, int column = 0)
        {
            BuildEngine.LogErrorEvent(
                new BuildErrorEventArgs(
                    SenderName,         // subcategory
                    string.Empty,       // code
                    BuildFileName,      // file
                    line,               // line number
                    column,             // column number
                    0,                  // end line number
                    0,                  // end column number
                    message ?? "null",  // message
                    string.Empty,       // help keyword
                    SenderName,         // sender
                    DateTime.Now        // date and time
                    ));
        }

        protected void EmitOutputForInput(ITaskItem outItem, ITaskItem inItem)
        {
            _outputFiles.AddComputedOutputForSourceRoot(
                inItem.GetMetadata(FullPath).ToUpperInvariant(),
                outItem.GetMetadata(FullPath).ToUpperInvariant());
        }

        protected void EmitOutputForInput(ITaskItem outItem, ITaskItem[] inItems)
        {
            foreach (var item in inItems)
                EmitOutputForInput(outItem, item);
        }

        protected void AddDependenciesForInput(ITaskItem inItem, ITaskItem[] depItems)
        {
            var inPath          = inItem.GetMetadata(FullPath).ToUpperInvariant();
            var dependencyTable = _inputFiles.DependencyTable;

            if (!dependencyTable.ContainsKey(inPath))
                dependencyTable.Add(inPath, new Dictionary<string, string>());

            if (depItems != null && depItems.Length > 0)
            {
                var dict = dependencyTable[inPath];
                foreach (var item in depItems)
                {
                    var key = item.GetMetadata(FullPath).ToUpperInvariant();

                    if (!dict.ContainsKey(key))
                        dict.Add(key, inPath);
                    else
                        dict[key] = inPath;
                }
            }
            else
            {
                var key  = inPath.ToUpperInvariant();
                var dict = dependencyTable[inPath];

                if (!dict.ContainsKey(key))
                    dict.Add(key, inPath);
                else
                    dict[key] = inPath;
            }
        }

        private void NotifyTaskStated()
        {
            _outputFiles = new Output(this, TLogWriteFiles, true);
            _inputFiles  = new Input(this, TLogReadFiles, Sources, null, _outputFiles, true, false);

            //_outputFiles.RemoveDependenciesFromEntryIfMissing(Sources);
            //_inputFiles.RemoveDependenciesFromEntryIfMissing(Sources);

            _currentSources = _inputFiles.ComputeSourcesNeedingCompilation();

            SkippedExecution = true;
            OnStart();
        }

        private void NotifyTaskFinished(bool succeeded)
        {
            OnStop(succeeded);
            SkippedExecution = !succeeded;

            if (succeeded && _currentSources.Length <= 0)
                SkippedExecution = true;

            if (!SkippedExecution)
            {
                _inputFiles.SaveTlog();
                _outputFiles.SaveTlog();
            }

        }

        /// <summary>
        /// A convenience function for spawning a process.
        /// </summary>
        /// <param name="tool">The process to start</param>
        /// <param name="arguments">Supplies any parameters for the process.</param>
        /// <returns>true if the call is successful.</returns>
        protected bool Call(string tool, string arguments)
        {
            if (string.IsNullOrEmpty(tool))
                throw new ArgumentNullException(nameof(tool), "the tool argument cannot be null");

            if (arguments == null)
                arguments = string.Empty;

            IEmTask task = this;
            return !task.Spawn(new ProcessStartInfo(tool) {
                CreateNoWindow         = true,
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                WorkingDirectory       = Environment.CurrentDirectory,
                Arguments              = arguments,
            });
        }

        bool IEmTask.Spawn(ProcessStartInfo info)
        {
            if (Verbose || EchoCommandLines)
                LogMessage($"{info.FileName} {info.Arguments}");

            // ensure that the redirect options have been set.
            info.RedirectStandardOutput = true;
            info.RedirectStandardError  = true;

            var process = Process.Start(info);
            if (process == null)
            {
                LogError($"{info.FileName} failed to start");
                return true;
            }

            process.OutputDataReceived += OnMessageDataReceived;
            process.ErrorDataReceived += OnErrorDataReceived;
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // Wait at least the specified TimeOut.
            // If the process has not finished
            // by then, kill it and report a time out.
            if (process.WaitForExit(TimeOut))
                return process.ExitCode != 0;

            LogError($"the process {BaseName(info.FileName)} timed out.");
            return false;
        }

        private void OnMessageDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                LogMessage(e.Data);
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            var line    = 0;
            var column  = 0;
            var matched = false;

            if (e.Data.Contains(BuildFileName))
            {
                // >main.cpp : error : main.cpp:8:5: error: ...
                //                             ^   ^
                var re    = new Regex(@"\:([0-9]+)\:([0-9]+)\:", RegexOptions.Compiled);
                var match = re.Match(e.Data);
                if (match.Success && match.Groups.Count == 3)
                {
                    matched = true;
                    if (int.TryParse(match.Groups[1].Value, out var pLine))
                        line = pLine;

                    if (int.TryParse(match.Groups[2].Value, out var pColumn))
                        column = pColumn;
                }
            }

            if (e.Data.Contains("error:"))
                LogError(e.Data, line, column);
            else
            {
                if (matched)
                    LogWarning(e.Data, line, column);
                else
                    LogMessage(e.Data);
            }
        }

        protected ITaskItem[] GetCurrentSource()
        {
            return _currentSources;
        }

        /// <summary>
        /// The main task function for tasks that derive from
        /// the EmTask base class.
        /// </summary>
        /// <returns>
        /// A true return indicates a successful run.
        /// </returns>
        public abstract bool Run();

        /// <summary>
        /// Defines a callback for code that needs executed right as the main
        /// ITask.Execute function is invoked.
        /// </summary>
        protected abstract void OnStart();

        protected virtual void OnStop(bool succeeded)
        {
        }

        public bool Execute()
        {
            if (DebugProp3)
            {
                if (!Debugger.IsAttached)
                    Debugger.Launch();
            }

            if (!ValidateSdk())
            {
                LogError("Failed to resolve the EMSDK root environment variable");
                LogError("emcc was not found");
                return false;
            }

            bool result;
            try
            {
                NotifyTaskStated();
                result = Run();
                NotifyTaskFinished(result);
            }
            catch (Exception ex)
            {
                result = false;
                LogError(ex.Message);
            }
            return result;
        }
    }
}
