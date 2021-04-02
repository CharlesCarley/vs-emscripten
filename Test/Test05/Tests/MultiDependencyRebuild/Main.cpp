#include "Main.h"
#include "Input1.h"
#include "Input2.h"
#include "Input3.h"



int main(int argc, char **argv)
{
    // Calls the source from within the
    // main programs object code
    SomePrototypeInTheExecutableSource();
    // Calls the source from within the
    // static library object code
    Input1_InTheStaticLibrarySource();
    Input2_InTheStaticLibrarySource();
    Input3_InTheStaticLibrarySource();
    return 0;
}

