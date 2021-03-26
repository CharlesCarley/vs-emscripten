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
using Microsoft.Build.Utilities;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static EmscriptenTask.EmUtils;

namespace EmscriptenTask
{
    public abstract class EmTask : IEmTask
    {
        private const int TimeOut = 30000;

        protected abstract string SenderName { get; }

        /// <summary>
        /// Internal access to the current input file for logging.
        /// </summary>
        protected abstract string _BuildFileName { get; }

        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost    HostObject { get; set; }

        // ========================= Tracking ======================================
        private ITaskItem[] _tLogReadFiles;
        private ITaskItem[] _tLogWriteFiles;

        protected string TLogWritePathName => $@"{TrackerLogDirectory}\{SenderName}.write.1.tlog";
        protected string TLogReadPathName => $@"{TrackerLogDirectory}\{SenderName}.read.1.tlog";
        protected string TLogCommandPathName => $@"{TrackerLogDirectory}\{SenderName}.command.1.tlog";


        [Required] public string TrackerLogDirectory { get; set; }
        public ITaskItem[] TLogReadFiles {
            get
            {
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
            get
            {
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

        [Required] public ITaskItem[] Sources { get; set; }
        public bool MinimalRebuildFromTracking { get; set; }

        protected CanonicalTrackedInputFiles  InputFiles { get; set; }
        protected CanonicalTrackedOutputFiles OutputFiles { get; set; }

        // ========================= Temporary  ====================================

        // Temporary extra debug properties
        public string DebugProp1 { get; set; }
        public string DebugProp2 { get; set; }
        public string DebugProp3 { get; set; }

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
        public bool EchoCommandLines { get; set; } = false;

        // ========================= Settings.js ==================================

        /// <summary>
        /// When set to 1, will generate more verbose output during compilation.
        /// </summary>
        public bool EmVerbose { get; set; } = false;

        /// <summary>
        /// Add some calls to emscripten tracing APIs.
        /// </summary>
        public bool EmTracing { get; set; } = false;

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
                if (field.CanRead)
                {
                    if (maxLen < field.Name.Length)
                    {
                        maxLen = field.Name.Length;
                    }
                }
            }

            LogSeparator(SenderName);
            foreach (var field in fields)
            {
                if (field.CanRead)
                {
                    var nameLen = maxLen + 1 - field.Name.Length;
                    var ws      = string.Empty;
                    if (nameLen > 0)
                        ws = new string(' ', nameLen);

                    LogMessage($"{field.Name}{ws}: {field.GetValue(inst)}");
                }
            }
        }

        protected void LogSeparator(string message)
        {
            string a = new string('=', 35);
            string b = new string('=', 45 - message.Length);
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
                    string.Empty,       // subcategory
                    string.Empty,       // code
                    _BuildFileName,     // file
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
                    string.Empty,       // subcategory
                    string.Empty,       // code
                    _BuildFileName,     // file
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

        protected delegate void TaskStartEventHandler();
        protected delegate void TaskStopEventHandler(bool succeeded);

        /// <summary>
        /// An Event that is called after the SDK paths have been validated and before
        /// The Run method is invoked.
        /// </summary>
        protected event TaskStartEventHandler OnTaskStarted;

        /// <summary>
        /// An Event that is called after Run is finished.
        /// </summary>
        protected event TaskStopEventHandler OnTaskStopped;

        private void NotifyTaskStated()
        {
            SkippedExecution = true;
            OnTaskStarted?.Invoke();
        }

        private void NotifyTaskFinished(bool succeeded)
        {
            SkippedExecution = !succeeded;
            OnTaskStopped?.Invoke(succeeded);
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
            if (!process.WaitForExit(TimeOut))
            {
                LogError($"the process {BaseName(info.FileName)} timed out.");
                return false;
            }
            return process.ExitCode != 0;
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

            if (e.Data.Contains(_BuildFileName))
            {
                // >main.cpp : error : main.cpp:8:5: error: ...
                //                             ^   ^
                var re    = new Regex($@"\:([0-9]+)\:([0-9]+)\:", RegexOptions.Compiled);
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
        public abstract void OnStart();

        public bool Execute()
        {
            OnStart();
            if (!ValidateSdk())
            {
                LogError("Failed to resolve the EMSDK root environment variable");
                LogError("emcc was not found");
                return false;
            }
            try
            {
                NotifyTaskStated();

                if (Run())
                {
                    NotifyTaskFinished(true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }

            NotifyTaskFinished(false);
            return false;
        }
    }
}
