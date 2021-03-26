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

namespace EmscriptenTask
{
    public static class EmUtils
    {
        public static string EmccTool { get; set; }
        public static string EmscriptenDirectory { get; set; }

        public static string Sanitize(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new NullReferenceException(
                    "The supplied path variable cannot be null or empty");
            }

            path = path.Replace('/', '\\');
            path = path.Replace("\\\\", "\\");
            return path;
        }

        public static bool ValidateSdk()
        {
            if (EmccTool != null)
                return true;

            var emsdk = Sanitize(Environment.GetEnvironmentVariable("EMSDK"));
            if (emsdk is null)
                throw new DirectoryNotFoundException("The environment variable EMSDK was not found");

            if (!Directory.Exists($"{emsdk}\\upstream\\emscripten"))
            {
                throw new DirectoryNotFoundException(
                    $"The upstream\\emscripten directory was not found in ${emsdk}");
            }

            if (!File.Exists($"{emsdk}\\upstream\\emscripten\\emcc.bat"))
            {
                throw new FileNotFoundException(
                    $"The emcc.bat batch file was not found in ${emsdk}\\upstream\\emscripten");
            }

            EmscriptenDirectory = $"{emsdk}\\upstream\\emscripten";

            EmccTool = $"{EmscriptenDirectory}\\emcc.bat";
            return true;
        }

        public static string AbsolutePath(string path)
        {
            return !Path.IsPathRooted(path) ? $@"{Environment.CurrentDirectory}\{path}" : path;
        }

        public static string GetSeparatedSource(char charSeparator, ITaskItem[] input)
        {
            if (input is null)
            {
                throw new NullReferenceException(
                    "The supplied input variable cannot be null or empty");
            }

            var builder = new StringWriter();
            foreach (var inp in input)
            {
                builder.Write(charSeparator);
                builder.Write(inp.ItemSpec);
            }
            return builder.ToString();
        }

        /// <param name="path">The input file system path.</param>
        /// <returns> Returns the text at the end of the last directory separator. </returns>
        public static string BaseName(string path)
        {
            path = Sanitize(path);

            var splits = path.Split('\\');
            return splits.Length > 0 ? splits[splits.Length - 1] : path;
        }

        public static string FileNameWithoutExtension(string path)
        {
            path = Sanitize(path);

            var splits = path.Split('\\');
            // is the last split
            var fileName = splits.Length > 0 ? splits[splits.Length - 1] : path;

            // is the first split
            var dotSplits = fileName.Split('.');
            var baseName  = dotSplits.Length > 0 ? dotSplits[0] : fileName;

            // recombine

            var builder = new StringWriter();

            for (var i = 0; i < splits.Length - 1; ++i)
            {
                builder.Write(splits[i]);
                builder.Write('\\');
            }

            builder.Write(baseName);
            return builder.ToString();
        }

        /// <param name="path">The input file system path.</param>
        /// <returns>Returns true if the supplied path is a file or directory.</returns>
        public static bool IsFileOrDirectory(string path)
        {
            return Directory.Exists(path) || File.Exists(path);
        }

        /// <summary>
        /// Utility to break apart character separated lists.
        /// </summary>
        /// <param name="paths">The original string.</param>
        /// <param name="originalSeparator">The original separator - mainly a semicolon ';' </param>
        /// <param name="tagSeparation">The string to insert in place of the separator</param>
        /// <param name="needsValidation">if this is set to true, the separation will be
        /// skipped if it is not a valid file or directory</param>
        /// <returns>returns the result of the operation.</returns>
        public static string SeparatePaths(string paths,
                                           char   originalSeparator,
                                           string tagSeparation,
                                           bool   needsValidation = false)
        {
            if (string.IsNullOrEmpty(paths) || string.IsNullOrEmpty(tagSeparation))
                return string.Empty;

            var splitPath = paths.Split(originalSeparator);
            var builder   = new StringWriter();
            foreach (var path in splitPath)
            {
                if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
                    continue;

                var sanitizedPath = Sanitize(path);
                if (needsValidation && !IsFileOrDirectory(sanitizedPath))
                    continue;
                builder.Write($" {tagSeparation} {sanitizedPath}");
            }
            return builder.ToString();
        }
    }
}
