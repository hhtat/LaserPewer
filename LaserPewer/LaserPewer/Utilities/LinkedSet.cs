using System.Collections;
using System.Collections.Generic;

namespace LaserPewer.Utilities
{
    public class LinkedSet<T> : ICollection<T>, IReadOnlyCollection<T>
    {
        #region ICollection

        public int Count { get { return linkedList.Count; } }
        public bool IsReadOnly { get { return false; } }

        #endregion

        #region LinkedList

        public LinkedListNode<T> First { get { return linkedList.First; } }
        public LinkedListNode<T> Last { get { return linkedList.Last; } }

        #endregion

        private readonly LinkedList<T> linkedList;
        private readonly Dictionary<T, LinkedListNode<T>> dictionary;

        public LinkedSet()
        {
            linkedList = new LinkedList<T>();
            dictionary = new Dictionary<T, LinkedListNode<T>>();
        }

        #region ICollection

        public void Clear()
        {
            linkedList.Clear();
            dictionary.Clear();
        }

        public void Add(T item)
        {
            LinkedListNode<T> node = new LinkedListNode<T>(item);
            dictionary.Add(item, node);
            linkedList.AddLast(node);
        }

        public bool Contains(T item)
        {
            return dictionary.ContainsKey(item);
        }

        public bool Remove(T item)
        {
            LinkedListNode<T> node;
            if (dictionary.TryGetValue(item, out node))
            {
                dictionary.Remove(item);
                linkedList.Remove(node);
            }
            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (T item in linkedList)
            {
                array[arrayIndex++] = item;
            }
        }

        #endregion

        #region LinkedList

        public LinkedListNode<T> Find(T value)
        {
            LinkedListNode<T> node;
            if (dictionary.TryGetValue(value, out node))
            {
                return node;
            }
            return null;
        }

        #endregion

        #region IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            return linkedList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return linkedList.GetEnumerator();
        }

        #endregion
    }
}
