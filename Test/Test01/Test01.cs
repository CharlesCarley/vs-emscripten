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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using static TestUtils.Utils;

namespace UnitTest
{
    [TestClass]
    public class Test01
    {
        [TestMethod]
        public void CanMsBuildExec()
        {
            Spawn(MsBuild, "/?");
        }

        [TestMethod]
        public void RunTest01()
        {
            ClearIfExists(IntDir);

            Spawn(MsBuild, "Test1");
            Assert.IsTrue(File.Exists($@"{CurrentDirectory}\{IntDir}\main.cpp.o"));
            Assert.IsTrue(File.Exists($@"{CurrentDirectory}\{IntDir}\Test1.wasm"));

            var output = Spawn(Wavm, $@" run {IntDir}\Test1.wasm");
            Assert.AreEqual("Main.cpp Hello WASM World!\n", output);

            ClearIfExists(IntDir);
        }

        [TestMethod]
        public void RunTest02()
        {
            ClearIfExists(IntDir);
            Spawn(MsBuild, "Test2");

            Assert.IsTrue(File.Exists($@"{CurrentDirectory}\{IntDir}\main.abcdefg"));
            Assert.IsTrue(File.Exists($@"{CurrentDirectory}\{IntDir}\Test2.wasm"));

            var output = Spawn(Wavm, $@" run {IntDir}\Test2.wasm");
            Assert.AreEqual("Main.cpp Hello WASM World!\n", output);

            ClearIfExists(IntDir);
        }

        [TestMethod]
        public void RunTest03()
        {
            ClearIfExists(IntDir);

            Spawn(MsBuild, "Test3");
            
            Assert.IsTrue(File.Exists($@"{CurrentDirectory}\{IntDir}\main.c.o"));
            Assert.IsTrue(File.Exists($@"{CurrentDirectory}\{IntDir}\Test3.wasm"));

            var  output = Spawn(Wavm, $@" run {IntDir}\Test3.wasm");

            Assert.AreEqual("main.c Hello WASM World!\n", output);
            ClearIfExists(IntDir);
        }

        [TestMethod]
        public void RunTest04()
        {
            ClearIfExists(IntDir);

            Spawn(MsBuild, "Test4");

            Assert.IsTrue(File.Exists($@"{CurrentDirectory}\{IntDir}\main2.c.o"));
            Assert.IsTrue(File.Exists($@"{CurrentDirectory}\{IntDir}\fn1.c.o"));
            Assert.IsTrue(File.Exists($@"{CurrentDirectory}\{IntDir}\fn2.c.o"));
            Assert.IsTrue(File.Exists($@"{CurrentDirectory}\{IntDir}\fn3.c.o"));
            Assert.IsTrue(File.Exists($@"{CurrentDirectory}\{IntDir}\Test4.wasm"));

            var output = Spawn(Wavm, $@" run {IntDir}\Test4.wasm");
            Assert.AreEqual("Main2--------------------------------Hello WASM World!\n", output);

            ClearIfExists(IntDir);
        }

        [TestMethod]
        public void RunTest05()
        {
            ClearIfExists(IntDir);

            Spawn(MsBuild, "Test5");

            Assert.IsTrue(File.Exists($@"{CurrentDirectory}\{IntDir}\main2.c.o"));
            Assert.IsTrue(File.Exists($@"{CurrentDirectory}\{IntDir}\fn1.c.o"));
            Assert.IsTrue(File.Exists($@"{CurrentDirectory}\{IntDir}\fn2.c.o"));
            Assert.IsTrue(File.Exists($@"{CurrentDirectory}\{IntDir}\fn3.c.o"));
            Assert.IsTrue(File.Exists($@"{CurrentDirectory}\{IntDir}\fnLib.a"));
            Assert.IsTrue(File.Exists($@"{CurrentDirectory}\{IntDir}\Test5.wasm"));

            var output = Spawn(Wavm, $@" run {IntDir}\Test5.wasm");
            Assert.AreEqual("Main2--------------------------------Hello WASM World!\n", output);

            ClearIfExists(IntDir);
        }
    }
}
