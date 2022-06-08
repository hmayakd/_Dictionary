using System;
using System.Collections;
using System.Collections.Generic;

namespace _Dictionary
{
    public class _Dictionary
    {
        public struct Entry
        {
            public int hashCode;    // Lower 31 bits of hash code, -1 if unused
            public int next;        // Index of next entry, -1 if last
            public string key;           // Key of entry
            public string value;         // Value of entry
        }
        private Entry[] entries;
        private int[] buckets;
        private int count;
        private int version;
        private int freeList;
        private int freeCount;
        private _KeyCollection keys;
        private _ValueCollection values;
        private IEqualityComparer<string> comparer;
        public _Dictionary() : this(0, null) { }
        public _Dictionary(int capacity) : this(capacity, null) { }
        public _Dictionary(IEqualityComparer<string> comparer) : this(0, comparer) { }
        public _Dictionary(int capacity, IEqualityComparer<string> comparer)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException();
            if (capacity > 0) Initialize(capacity);
            this.comparer = comparer ?? EqualityComparer<string>.Default;
        }
        public _Dictionary(IDictionary<string, string> dictionary) : this(dictionary, null) { }
        public _Dictionary(IDictionary<string, string> dictionary, IEqualityComparer<string> comparer) :
            this(dictionary != null ? dictionary.Count : 0, comparer)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException();
            }
            foreach (KeyValuePair<string, string> pair in dictionary)
            {
                Add(pair.Key, pair.Value);
            }
        }
        public string this[string key]
        {
            get
            {
                int i = FindEntry(key);
                if (i >= 0) return entries[i].value;
                throw new KeyNotFoundException();
            }
            set
            {
                Insert(key, value, false);
            }
        }
        public IEqualityComparer<string> Comparer
        {
            get
            {
                return comparer;
            }
        }
        public int Count
        {
            get { return count - freeCount; }
        }
        public int Version
        {
            get { return version; }
        }
        public Entry[] Entries
        {
            get { return entries; }
        }
        public _KeyCollection Keys
        {
            get
            {
                if (keys == null) keys = new _KeyCollection(this);
                return keys;
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }
        public bool ContainsKey(string key)
        {
            return FindEntry(key) >= 0;
        }
        public void Clear()
        {
            if (count > 0)
            {
                for (int i = 0; i < buckets.Length; i++) buckets[i] = -1;
                Array.Clear(entries, 0, count);
                freeList = -1;
                count = 0;
                freeCount = 0;
                version++;
            }
        }
        public _ValueCollection Values
        {
            get
            {
                if (values == null) values = new _ValueCollection(this);
                return values;
            }
        }
        public bool ContainsValue(string value)
        {
            if (value == null)
            {
                for (int i = 0; i < count; i++)
                {
                    if (entries[i].hashCode >= 0 && entries[i].value == null) return true;
                }
            }
            else
            {
                EqualityComparer<string> c = EqualityComparer<string>.Default;
                for (int i = 0; i < count; i++)
                {
                    if (entries[i].hashCode >= 0 && c.Equals(entries[i].value, value)) return true;
                }
            }
            return false;
        }
        public void Add(string key, string value)
        {
            Insert(key, value, true);
        }
        public bool Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(key);
            }

            if (buckets != null)
            {
                int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
                int bucket = hashCode % buckets.Length;
                int last = -1;
                for (int i = buckets[bucket]; i >= 0; last = i, i = entries[i].next)
                {
                    if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key))
                    {
                        if (last < 0)
                        {
                            buckets[bucket] = entries[i].next;
                        }
                        else
                        {
                            entries[last].next = entries[i].next;
                        }
                        entries[i].hashCode = -1;
                        entries[i].next = freeList;
                        entries[i].key = default(string);
                        entries[i].value = default(string);
                        freeList = i;
                        freeCount++;
                        version++;
                        return true;
                    }
                }
            }
            return false;
        }
        private void CopyTo(KeyValuePair<string, string>[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException();
            }

            if (index < 0 || index > array.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (array.Length - index < Count)
            {
                throw new ArgumentException();
            }

            int count = this.count;
            Entry[] entries = this.entries;
            for (int i = 0; i < count; i++)
            {
                if (entries[i].hashCode >= 0)
                {
                    array[index++] = new KeyValuePair<string, string>(entries[i].key, entries[i].value);
                }
            }
        }
        private void Insert(string key, string value, bool add)
        {
            if (key == null)
            {
                throw new ArgumentNullException(key);
            }
            if (buckets == null) Initialize(0);
            int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
            int targetBucket = hashCode % buckets.Length;
            for (int i = buckets[targetBucket]; i >= 0; i = entries[i].next)
            {
                if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key))
                {
                    if (add)
                    {
                        throw new ArgumentException();
                    }
                    entries[i].value = value;
                    version++;
                    return;
                }
            }
            int index;
            if (freeCount > 0)
            {
                index = freeList;
                freeList = entries[index].next;
                freeCount--;
            }
            else
            {
                if (count == entries.Length)
                {
                    Resize();
                    targetBucket = hashCode % buckets.Length;
                }
                index = count;
                count++;
            }
            entries[index].hashCode = hashCode;
            entries[index].next = buckets[targetBucket];
            entries[index].key = key;
            entries[index].value = value;
            buckets[targetBucket] = index;
            version++;
        }
        private int FindEntry(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(key);
            }

            if (buckets != null)
            {
                int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
                for (int i = buckets[hashCode % buckets.Length]; i >= 0; i = entries[i].next)
                {
                    if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key)) return i;
                }
            }
            return -1;
        }
        private void Initialize(int capacity)
        {
            int size = _HashHelpers.GetPrime(capacity);
            buckets = new int[size];
            for (int i = 0; i < buckets.Length; i++) buckets[i] = -1;
            entries = new Entry[size];
            freeList = -1;
        }
        private void Resize()
        {
            Resize(_HashHelpers.ExpandPrime(count), false);
        }
        private void Resize(int newSize, bool forceNewHashCodes)
        {
            int[] newBuckets = new int[newSize];
            for (int i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;
            Entry[] newEntries = new Entry[newSize];
            Array.Copy(entries, 0, newEntries, 0, count);
            if (forceNewHashCodes)
            {
                for (int i = 0; i < count; i++)
                {
                    if (newEntries[i].hashCode != -1)
                    {
                        newEntries[i].hashCode = (comparer.GetHashCode(newEntries[i].key) & 0x7FFFFFFF);
                    }
                }
            }
            for (int i = 0; i < count; i++)
            {
                if (newEntries[i].hashCode >= 0)
                {
                    int bucket = newEntries[i].hashCode % newSize;
                    newEntries[i].next = newBuckets[bucket];
                    newBuckets[bucket] = i;
                }
            }
            buckets = newBuckets;
            entries = newEntries;
        }
    }
    public struct Enumerator : IEnumerator<KeyValuePair<string, string>>, IDictionaryEnumerator
    {
        private _Dictionary dictionary;
        private int version;
        private int index;
        private KeyValuePair<string, string> current;
        private int getEnumeratorRetType;  // What should Enumerator.Current return?
        internal const int DictEntry = 1;
        internal const int KeyValuePair = 2;
        internal Enumerator(_Dictionary dictionary, int getEnumeratorRetType)
        {
            this.dictionary = dictionary;
            version = dictionary.Version;
            index = 0;
            this.getEnumeratorRetType = getEnumeratorRetType;
            current = new KeyValuePair<string, string>();
        }
        public bool MoveNext()
        {
            if (version != dictionary.Version)
            {
                throw new InvalidOperationException();
            }

            // Use unsigned comparison since we set index to dictionary.count+1 when the enumeration ends.
            // dictionary.count+1 could be negative if dictionary.count is Int32.MaxValue
            while ((uint)index < (uint)dictionary.Count)
            {
                if (dictionary.Entries[index].hashCode >= 0)
                {
                    current = new KeyValuePair<string, string>(dictionary.Entries[index].key, dictionary.Entries[index].value);
                    index++;
                    return true;
                }
                index++;
            }

            index = dictionary.Count + 1;
            current = new KeyValuePair<string, string>();
            return false;
        }
        public KeyValuePair<string, string> Current
        {
            get { return current; }
        }
        public void Dispose()
        {
        }
        object IEnumerator.Current
        {
            get
            {
                if (index == 0 || (index == dictionary.Count + 1))
                {
                    throw new InvalidOperationException();
                }

                if (getEnumeratorRetType == DictEntry)
                {
                    return new System.Collections.DictionaryEntry(current.Key, current.Value);
                }
                else
                {
                    return new KeyValuePair<string, string>(current.Key, current.Value);
                }
            }
        }
        void IEnumerator.Reset()
        {
            if (version != dictionary.Version)
            {
                throw new InvalidOperationException();
            }

            index = 0;
            current = new KeyValuePair<string, string>();
        }
        DictionaryEntry IDictionaryEnumerator.Entry
        {
            get
            {
                if (index == 0 || (index == dictionary.Count + 1))
                {
                    throw new InvalidOperationException();
                }

                return new DictionaryEntry(current.Key, current.Value);
            }
        }
        object IDictionaryEnumerator.Key
        {
            get
            {
                if (index == 0 || (index == dictionary.Count + 1))
                {
                    throw new InvalidOperationException();
                }

                return current.Key;
            }
        }
        object IDictionaryEnumerator.Value
        {
            get
            {
                if (index == 0 || (index == dictionary.Count + 1))
                {
                    throw new InvalidOperationException();
                }

                return current.Value;
            }
        }
    }
}
