using System;
using System.Collections;
using System.Collections.Generic;

namespace InitialPrefabs.NimGui.Collections {

    public struct ReadOnlyCollection<T> : IEnumerable<T>, IEnumerable {

        readonly List<T> Collection;

        public int Count => Collection.Count;

        public T this[int index] => Collection[index];

        public ReadOnlyCollection(List<T> source) {
            Collection = source;
        }

        public List<T>.Enumerator GetEnumerator() {
            return Collection.GetEnumerator();
        }

        public bool Contains(T item) {
            return Collection.Contains(item);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            throw new NotSupportedException("To avoid boxing, do not cast NoAllocReadOnlyCollection to IEnumerable<T>.");
        }

        IEnumerator IEnumerable.GetEnumerator() {
            throw new NotSupportedException("To avoid boxing, do not cast NoAllocReadOnlyCollection to IEnumerable.");
        }
    }
}
