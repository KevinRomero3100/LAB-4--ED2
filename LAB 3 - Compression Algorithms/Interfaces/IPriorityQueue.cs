using System;
using System.Collections.Generic;
using System.Text;

namespace LAB_3___Compressor.Interfaces
{
    interface IPriorityQueue<T>
    {
        void Push(T value);
        T Pop();
        bool IsEmpty();
    }
}
