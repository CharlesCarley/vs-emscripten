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

using System.Collections;
using Microsoft.Build.Framework;
using Logger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;

namespace UnitTest
{
    public class EmptyBuildEngine : IBuildEngine
    {
        public void LogErrorEvent(BuildErrorEventArgs e)
        {
            Logger.LogMessage("error : {0}", e.Message);
        }

        public void LogWarningEvent(BuildWarningEventArgs e)
        {
            Logger.LogMessage("warning : {0}", e.Message);
        }

        public void LogMessageEvent(BuildMessageEventArgs e)
        {
            Logger.LogMessage("{0}", e.Message);
        }

        public void LogCustomEvent(CustomBuildEventArgs e)
        {
            Logger.LogMessage("{0}", e.Message);
        }

        public bool BuildProjectFile(string projectFileName,
                                     string[] targetNames,
                                     IDictionary globalProperties,
                                     IDictionary targetOutputs)
        {
            return true;
        }

        public bool   ContinueOnError { get; }        = true;
        public int    LineNumberOfTaskNode { get; }   = 0;
        public int    ColumnNumberOfTaskNode { get; } = 0;
        public string ProjectFileOfTaskNode { get; }  = string.Empty;
    }
}
