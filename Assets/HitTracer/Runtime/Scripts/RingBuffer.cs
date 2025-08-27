using System;
using System.Collections.Generic;

namespace HitTrace
{
    public class RingBuffer<T> where T : class, new()
    {
        private T[] buffer;
        private int head = -1;
        private int count = 0;

        public int Count => count;
        public int Capacity => buffer.Length;
    
        public RingBuffer(int capacity)
        {
            buffer = new T[capacity];
        }

        public void Add(T item)
        {
            head = (head + 1) % buffer.Length;
            buffer[head] = item;

            if (count < buffer.Length)
                count++;
        }

        public IEnumerable<T> IterateLatestToEarliest()
        {
            for (int i = 0; i < count; i++)
            {
                int idx = (head - i + buffer.Length) % buffer.Length;
                yield return buffer[idx];
            }
        }

        public void Resize(int newCapacity)
        {
            if (newCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(newCapacity));
            if (Capacity == newCapacity) return;

            T[] newBuffer = new T[newCapacity];
            int itemsToCopy = Math.Min(count, newCapacity);

            // Copy latest items into new buffer
            for (int i = 0; i < itemsToCopy; i++)
            {
                int oldIdx = (head - (itemsToCopy - 1 - i) + buffer.Length) % buffer.Length;
                newBuffer[i] = buffer[oldIdx];
            }

            buffer = newBuffer;
            count = itemsToCopy;
            head = itemsToCopy - 1;
        }

        public T GetBackItem()
        {
            var idx = (head - (count - 1) + buffer.Length) % buffer.Length;
            return Count < Capacity ? new T() : buffer[idx];
        }
    }
}