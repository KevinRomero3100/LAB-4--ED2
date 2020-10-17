using System;
using System.Collections.Generic;
using System.Text;

namespace LAB_3___Compressor.DataStructures
{
    public class PriorityQueue<T> : Interfaces.IPriorityQueue<T>
    {
        private List<T> Queue = new List<T>();
        public Delegate PriorityComparison;
        public int Count;
        public bool IsEmpty()
        {
            if (Queue.Count == 0) return true;
            return false;
        }

        public T Pop()
        {
            int postition_min = GetPosition();
            T value = Queue[postition_min];
            Queue[postition_min] = Queue[Queue.Count - 1];
            Queue[Queue.Count - 1] = value;

            Queue.RemoveAt(Queue.Count-1);
            Count--;

            return value;
        }

        public void Push(T value)
        {
            Queue.Add(value);
            Count++;
        }

        private int GetPosition()
        {
            int position = 0;

            T min = Queue[0];
            for (int i = 0; i < Queue.Count; i++)
            {
                if ((int)PriorityComparison.DynamicInvoke(Queue[i], min)==-1)
                {
                    min = Queue[i];
                    position = i;
                }
            }
            return position;
        }
    }
}
