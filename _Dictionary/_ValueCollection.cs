using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Dictionary
{
    public sealed class _ValueCollection : ICollection<string>, ICollection, IReadOnlyCollection<string>
    {
        private _Dictionary dictionary;
        public _ValueCollection(_Dictionary dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException();
            }
            this.dictionary = dictionary;
        }
        public int Count
        {
            get { return dictionary.Count; }
        }
        bool ICollection<string>.IsReadOnly
        {
            get { return true; }
        }
        bool ICollection.IsSynchronized
        {
            get { return false; }
        }
        public object SyncRoot => throw new NotImplementedException();
        public void Add(string item)
        {
            throw new NotImplementedException();
        }
        public void Clear()
        {
            throw new NotImplementedException();
        }
        public bool Contains(string item)
        {
            throw new NotImplementedException();
        }
        public void CopyTo(string[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }
        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }
        public IEnumerator<string> GetEnumerator()
        {
            throw new NotImplementedException();
        }
        public bool Remove(string item)
        {
            throw new NotImplementedException();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
