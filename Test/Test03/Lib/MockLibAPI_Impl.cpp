#include <stdio.h>
#include "MockLibAPI.h"

void Mock2()
{
    printf("Mock symbol 2 called\n");
}


MockEnum Mock3(int x)
{
    switch (x)
    {
    case 0:
        return A;
    case 1:
        return B;
    case 2:
        return C;
    default:
        return D;
    }
}
