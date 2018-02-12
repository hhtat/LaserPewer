using System;
using System.Collections.Generic;

namespace LaserPewer.Utilities
{
    public class PriorityQueue<T>
    {
        public int Count { get { return list.Count; } }

        private readonly List<T> list;
        private readonly Comparison<T> comparison;

        public PriorityQueue(Comparison<T> comparison)
        {
            list = new List<T>();
            this.comparison = comparison;
        }

        public void Clear()
        {
            list.Clear();
        }

        public T Dequeue()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException();
            }

            T head = list[0];
            list[0] = list[Count - 1];
            list.RemoveAt(Count - 1);
            if (Count > 0) percolate(0);

            return head;
        }

        public void Enqueue(T item)
        {
            list.Add(item);
            bubble(Count - 1);
        }

        public T Peek()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException();
            }

            return list[0];
        }

        private void bubble(int index)
        {
            T value = list[index];

            while (index > 0)
            {
                int parentIndex = index / 2;
                if (comparison(value, list[parentIndex]) < 0)
                {
                    list[index] = list[parentIndex];
                    index = parentIndex;
                    continue;
                }

                break;
            }

            list[index] = value;
        }

        private void percolate(int index)
        {
            T value = list[index];

            while (true)
            {
                int leftIndex = 2 * index + 1;
                if (leftIndex < list.Count)
                {
                    int rightIndex = 2 * index + 2;
                    if (rightIndex < list.Count)
                    {
                        if (comparison(list[leftIndex], value) < 0 && comparison(list[leftIndex], list[rightIndex]) <= 0)
                        {
                            list[index] = list[leftIndex];
                            index = leftIndex;
                            continue;
                        }

                        if (comparison(list[rightIndex], value) < 0)
                        {
                            list[index] = list[rightIndex];
                            index = rightIndex;
                            continue;
                        }
                    }
                    else
                    {
                        if (comparison(list[leftIndex], value) < 0)
                        {
                            list[index] = list[leftIndex];
                            index = leftIndex;
                            continue;
                        }
                    }
                }

                break;
            }

            list[index] = value;
        }
    }
}
