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
    public interface IEmTask : ITask
    {

        /// <summary>
        /// This should be the upstream/emscripten directory in the emsdk.
        /// </summary>
        string EmscriptenDirectory { get; }

        /// <summary>
        /// This should be the full path to the $(EmscriptenDirectory)\emcc.bat batch file.
        /// </summary>
        string EmccTool { get; }

        /// <summary>
        /// If this is set to true, this will enable task logging.
        /// </summary>
        bool Verbose { get; set; }

        /// <summary>
        /// This should contain the list of source files to build.
        /// </summary>
        string Sources { get; set; }

        /// <summary>
        /// If this is set to true, the contents of the command line should be
        /// logged to stdout.
        /// </summary>
        bool EchoCommandLines { get; set; }

        /// <summary>
        /// Spawns the process described in info
        /// </summary>
        /// <param name="info">Details about the process that will be executed.</param>
        /// <returns>
        /// The inverse of a typical return from a main function.
        /// That is, A non zero return code should yield a true return. 
        /// </returns>
        bool Spawn(ProcessStartInfo info);
    }
}
