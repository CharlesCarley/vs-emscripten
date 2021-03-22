# -----------------------------------------------------------------------------
#   Copyright (c) Charles Carley.
#
#   This software is provided 'as-is', without any express or implied
# warranty. In no event will the authors be held liable for any damages
# arising from the use of this software.
#
#   Permission is granted to anyone to use this software for any purpose,
# including commercial applications, and to alter it and redistribute it
# freely, subject to the following restrictions:
#
# 1. The origin of this software must not be misrepresented; you must not
#    claim that you wrote the original software. If you use this software
#    in a product, an acknowledgment in the product documentation would be
#    appreciated but is not required.
# 2. Altered source versions must be plainly marked as such, and must not be
#    misrepresented as being the original software.
# 3. This notice may not be removed or altered from any source distribution.
# ------------------------------------------------------------------------------
import glob, string
import os, sys, subprocess, shutil


def clear_nuget():
    os.system("nuget locals all -clear")

def log(st, path):
    path = path.replace('\\', '/')
    path = path.replace('//', '/')

    if st:
        print ("Deleting => %s"%path)
    else:
        print ("Skipped => %s"%path)

def loadIgnoreList(root):

    base = root + os.sep + ".gitignore"
    ignore = []

    if (os.path.isfile(base)):
        file = open(base, encoding='utf8')
        data = file.readlines()
        file.close()

        for line in data:
            line = line.replace('\r', '')
            line = line.replace('\n', '')
            line = line.replace('*', '')

            if (line != 'secrets' and line.find("credentials") == -1):
                ignore.append(line)

    return ignore


def main(argc, argv):

    if argc <= 1:
        print("Usage: " + sys.argv[0] + " root path")
        exit(1)

    rootPath = sys.argv[1]
    ignore = loadIgnoreList(rootPath)

    for root, dirs, files in os.walk(rootPath):
        for directory in dirs:

            if directory in ignore:
                build_path = root + os.sep + directory

                if os.path.isdir(build_path):
                    try:
                        log(1, build_path)
                        shutil.rmtree(build_path)
                    except:
                        log(0, build_path)


        for file in files:

            build_path = root + os.sep + file
            li = os.path.splitext(build_path)

            if len(li) > 1:
                if li[1] in ignore:

                    if os.path.isfile(build_path):
                        try:
                            log(1, build_path)
                            os.remove(build_path)
                        except:
                            log(0, build_path)

if __name__ == "__main__":
    exit(main(len(sys.argv), sys.argv))
