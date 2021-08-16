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
import os, sys, shutil, subprocess

TEST_DIR = "Test"

ConfigEnv = {
    "TestDir" : "Test",
    "BuildDir": "__build__",
    "Generator": '"Visual Studio 16 2019" -A Emscripten -T emsdk',
    "GeneratorCLangCL": '"Visual Studio 16 2019" -A x64 -T CLangCL'
}

# directory
def gotoDir(directory):
    try:
        os.chdir(directory)
    except:
        print("Failed to change to directory: ", directory)

def goToHome():
    fname = sys.argv[0]
    try:
        splits = fname.split('\\')
        fname = ""
        for i in range(0, len(splits)-2):
            fname += splits[i] + os.sep

        os.chdir(fname)
    except:
        print("Failed to change to directory: ", fname)


def execString(strToExec):
    os.system(strToExec)

def createBuildDir(direct):

    curDir = os.getcwd()
    buildDir = curDir + os.sep + direct

    if (not os.path.isdir(buildDir)):
        os.mkdir(buildDir)

def removeBuildDir(direct):

    curDir = os.getcwd()
    buildDir = curDir + os.sep + direct

    if (os.path.isdir(buildDir)):
        shutil.rmtree(buildDir)


def testCMake(projDir):
    gotoDir("Test/"+projDir)

    removeBuildDir(ConfigEnv['BuildDir'])
    createBuildDir(ConfigEnv['BuildDir'])
    gotoDir(ConfigEnv['BuildDir'])

    absDir = os.getcwd()


    success = os.system("cmake .. -G " + ConfigEnv['Generator']) == 0
    try:
        #cmd = 'type %s\\CMakeFiles\\CMakeOutput.log'%absDir
        # os.system(cmd)
        cmd = 'type %s\\CMakeFiles\\CMakeError.log'%absDir
        os.system(cmd)
    except:
        pass

    goToHome()
    gotoDir("Test/"+projDir)

    if (success):
        absDir = os.getcwd() + os.sep + ConfigEnv['BuildDir'] + os.sep
        vsDir = os.environ.get("VS2019INSTALLDIR") + "\\Common7\\IDE\\devenv.exe"
        os.system(vsDir + " " + absDir + "TestCMakeSupport.sln")


def test01():
    gotoDir("Test/Test01")
    absDir = os.getcwd()
    success = os.system("msbuild Test1") == 0
    success = success and os.system("msbuild Test2") == 0
    success = success and os.system("msbuild Test3") == 0
    success = success and os.system("msbuild Test4") == 0
    success = success and os.system("msbuild Test5") == 0
    goToHome()


def main(argc, argv):

    # Grab it from the environment
    vsDir = os.environ.get("VS2019INSTALLDIR")
    if vsDir == None:
        print("Unable to locate the Visual Studio install directory")
        return 1

    # looking for the msbuild toolset subdir
    vsDir += "\\MSBuild\\Microsoft\\VC\\v160\\Platforms"


    if not os.path.isdir(vsDir):
        print("Unable to locate a valid directory in :", vsDir)
        return 1

    work = os.getcwd()
    dirToCopy = work + "\\Source\Toolset"
    copiedDir = vsDir + "\\Emscripten"


    try:
        shutil.rmtree(copiedDir)
    except:
        pass
    shutil.copytree(dirToCopy, copiedDir)


    fileToCopy = work + "\\Bin\EmscriptenTask.dll"
    copiedFile = vsDir + "\\Emscripten\\EmscriptenTask.CPP.Tasks.dll"

    shutil.copy(fileToCopy, copiedFile)

    if ("test01" in argv):
        test01()
    elif ("test02" in argv):
        testCMake("Test02")
    elif ("test03" in argv):
        testCMake("Test03")
    return 0


if __name__ == "__main__":
    exit(main(len(sys.argv), sys.argv))
