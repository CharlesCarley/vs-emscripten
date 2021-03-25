#include "MockLibAPI.h"

int main()
{

    Mock1();
    Mock2();
    switch (Mock3(0)) // to generate a warning
    {
    case A:
        break;
    }
    return 0;
}
