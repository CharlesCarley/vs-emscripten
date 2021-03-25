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
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using static EmscriptenTask.EmUtils;
using System.Collections;

namespace EmscriptenTask
{
    public class EmSource : ITaskItem
    {
        private List<string>               m_metaDataNames = null;
        private Dictionary<string, string> m_metaData      = new Dictionary<string, string>();

        public EmSource(string inFile, string outFile)
        {
            inFile  = AbsoultePath(inFile);
            outFile = AbsoultePath(outFile);

            SetMetadata("FullPath", inFile);
            SetRootedPath("RootedFullPath", inFile);

            SetMetadata("OutputFile", outFile);
            SetMetadata("DefiningProjectFullPath", inFile);
        }

        public EmSource(string inFile)
        {
            SetMetadata("FullPath", inFile);
            SetRootedPath("RootedFullPath", inFile);
            SetMetadata("DefiningProjectFullPath", inFile);
        }

        private void SetRootedPath(string metaName, string pathToRoot)
        {
            // for saving to the .tlog
            pathToRoot = $@"^{AbsoultePath(pathToRoot)}".ToUpperInvariant();
            SetMetadata(metaName, pathToRoot);
        }

        public string ItemSpec
        {
            get {
                return GetMetadata("FullPath");
            }
            set {
                SetMetadata("FullPath", value);
            }
        }

        public ICollection MetadataNames
        {
            get {
                if (m_metaDataNames == null)
                {
                    m_metaDataNames = new List<string> {
                        "FullPath",
                        "RootedFullPath",
                        "OutputFile",
                        "DefiningProjectFullPath"
                    };
                }
                return m_metaDataNames;
            }
        }

        public int MetadataCount => MetadataNames.Count;

        public IDictionary CloneCustomMetadata()
        {
            return new Dictionary<string, string>(m_metaData);
        }

        public void CopyMetadataTo(ITaskItem destinationItem)
        {
            destinationItem.ItemSpec = ItemSpec;
        }

        public string GetMetadata(string metadataName)
        {
            if (m_metaData.ContainsKey(metadataName))
                return m_metaData[metadataName];
            return null;
        }

        public void RemoveMetadata(string metadataName)
        {
            if (m_metaData.ContainsKey(metadataName))
                m_metaData.Remove(metadataName);
        }

        public void SetMetadata(string metadataName, string metadataValue)
        {
            if (m_metaData.ContainsKey(metadataName))
                m_metaData[metadataName] = metadataValue;
            else
                m_metaData.Add(metadataName, metadataValue);
        }
    }
}
