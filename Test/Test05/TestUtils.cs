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
using Logger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;

namespace UnitTest
{
    public static class TestUtils
    {
        public static string CurrentDirectory => $@"{Environment.CurrentDirectory}\..\..\";
        public static string Debug            => "Debug";

        public static string WriteSwitchesToString(object obj)
        {
            var writer = new StringWriter();
            EmscriptenTask.EmSwitchWriter.Write(writer, obj.GetType(), obj);
            return writer.ToString();
        }

        public static void ClearIfExists(string directory)
        {
            var dir = CurrentDirectory + directory;
            if (!Directory.Exists(dir))
                return;
            try
            {
                Directory.Delete(dir, true);
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        public static void Log(string message)
        {
            Logger.LogMessage("{0}", message);
        }
    }
}
