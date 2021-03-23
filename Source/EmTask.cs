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
using static EmscriptenTask.EmUtils;
using System.Text.RegularExpressions;

namespace EmscriptenTask
{
    public abstract class EmTask : IEmTask
    {
        protected abstract string SenderName { get; }
        protected abstract string _BuildFileName { get; }

        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost    HostObject { get; set; }

        /// <summary>
        /// Indicator to determine build state
        /// </summary>
        [Output] public bool SkippedExecution { get; set; } = true;

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
        /// This should contain the list of source files to build.
        /// </summary>
        public string Sources { get; set; }

        /// <summary>
        /// If this is set to true, the contents of the command line should be
        /// logged to stdout.
        /// </summary>
        public bool EchoCommandLines { get; set; } = false;

        // =====================================================================================

        /// <summary>
        /// When set to 1, will generate more verbose output during compilation.
        /// </summary>
        public bool EmVerbose { get; set; } = false;

        /// <summary>
        /// Add some calls to emscripten tracing APIs.
        /// </summary>
        public bool EmTracing { get; set; } = false;

        // =====================================================================================

        protected void LogTaskProps(Type type, object inst)
        {
            var fields = type.GetProperties();
            int maxLen = 0;
            foreach (var field in fields)
            {
                if (field.CanRead)
                {
                    if (maxLen < field.Name.Length)
                        maxLen = field.Name.Length;
                }
            }

            LogSeperator(SenderName);
            foreach (var field in fields)
            {
                if (field.CanRead)
                {
                    string ws = new string(' ', (maxLen + 1) - field.Name.Length);
                    LogMessage($"{field.Name}{ws}: {field.GetValue(inst)}");
                }
            }
        }

        protected void LogSeperator()
        {
            LogMessage(new string('=', 80));
        }

        protected void LogSeperator(string message)
        {
            string a = new string('=', 35);
            string b = new string('=', 45 - message.Length);
            LogMessage($"{a} {message} {b}");
        }
        protected void LogSeperatorShort(string message)
        {
            string a = new string('=', 35);
            LogMessage($"{a} {message}");
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

        bool IEmTask.Spawn(ProcessStartInfo info)
        {
            if (Verbose || EchoCommandLines)
                LogMessage($"{info.FileName} {info.Arguments}");

            // ensure that the redirect options have been set.
            info.RedirectStandardOutput = true;
            info.RedirectStandardError  = true;

            var process = Process.Start(info);

            process.OutputDataReceived += OnMessageDataRecieved;
            process.ErrorDataReceived += OnErrorDataRecieved;
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            /// Wait at least 30 seconds, and
            /// if the process has not finished
            /// by then, kill it and report a time out.
            if (!process.WaitForExit(30000))
            {
                LogError($"the process {BaseName(info.FileName)} timed out.");
                return false;
            }

            return process.ExitCode != 0;
        }

        private void OnMessageDataRecieved(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                LogMessage(e.Data);
        }

        private void OnErrorDataRecieved(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                int  line    = 0;
                int  column  = 0;
                bool matched = false;

                if (e.Data.Contains(_BuildFileName))
                {
                    // >main.cpp : error : main.cpp:8:5: error: ...
                    Regex re    = new Regex($@"\:([0-9]+)\:([0-9]+)\:", RegexOptions.Compiled);
                    var   match = re.Match(e.Data);
                    if (match.Success && match.Groups.Count == 3)
                    {
                        matched = true;
                        if (int.TryParse(match.Groups[1].Value, out int pline))
                            line = pline;
                        if (int.TryParse(match.Groups[2].Value, out int pcolumn))
                            column = pcolumn;
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
        }

        public abstract bool Run();

        public bool Execute()
        {
            if (!ValidateSDK())
            {
                LogError("Failed to resolve the EMSDK root environment variable");
                LogError("emcc was not found");
                return false;
            }
            try
            {
                return Run();
            }
            catch (Exception ex)
            {
                LogError(ex.Message);
            }
            return false;
        }
    }
}
