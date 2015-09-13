using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace i4o
{
    public class EqualityIndex<TChild> : IIndex<TChild>
    {
        private readonly Dictionary<int, List<TChild>> _index = new Dictionary<int, List<TChild>>();
        private readonly PropertyReader<TChild> _propertyReader;

        public EqualityIndex(
            IEnumerable<TChild> collectionToIndex,
            PropertyInfo property)
        {
            _propertyReader = new PropertyReader<TChild>(property.Name);
            collectionToIndex.ForEach(Add);
        }

        public IEnumerator<TChild> GetEnumerator()
        {
            return _index.Values.SelectMany(list => list).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(TChild item)
        {
            int propValue = _propertyReader.GetItemHashCode(item);

            List<TChild> list;
            if (_index.TryGetValue(propValue, out list))
                list.Add(item);
            else
                _index.Add(propValue, new List<TChild> { item });
        }

        public void Clear()
        {
            _index.Clear();
        }

        public bool Contains(TChild item)
        {
            int propValue = _propertyReader.GetItemHashCode(item);
            return _index.ContainsKey(propValue) && _index[propValue].Contains(item);
        }

        public void CopyTo(TChild[] array, int arrayIndex)
        {
            var listOfAll = this.ToList();
            listOfAll.CopyTo(array, arrayIndex);
        }

        public bool Remove(TChild item)
        {
            int propValue = _propertyReader.GetItemHashCode(item);

            List<TChild> list;
            if (_index.TryGetValue(propValue, out list))
                return list.Remove(item);

            return false;
        }

        public int Count
        {
            get
            {
                return _index.Count();
                // return this.Count(); 
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public IEnumerable<TChild> WhereThroughIndex(Expression<Func<TChild, bool>> predicate)
        {
            var equalityExpression = predicate.Body as BinaryExpression;
            if (equalityExpression == null)
                throw new NotSupportedException();

            if (equalityExpression.NodeType != ExpressionType.Equal)
                throw new NotImplementedException("Equality Indexes do not work with non equality binary expressions");

            var rightSide = Expression.Lambda(equalityExpression.Right);
            var valueToCheck = rightSide.Compile().DynamicInvoke(null).GetHashCode();
            if (_index.ContainsKey(valueToCheck))
                foreach (var item in _index[valueToCheck])
                {
                    var matchingFromBucket = _index[valueToCheck].Where(predicate.Compile());
                    foreach (var bucketItem in matchingFromBucket) yield return bucketItem;
                }
            else
                yield break;
        }

        public void Reset(TChild changedObject)
        {
            // This is buggy, it tries to remove by looking for the index, that we don't know it.
            // Remove(changedObject);

            foreach (var i in _index.ToArray())
            {
                foreach (var c in i.Value.ToArray())
                    if (object.Equals(c, changedObject))
                        i.Value.Remove(c);

                if (i.Value.Count() == 0)
                    _index.Remove(i.Key);
            }

            Add(changedObject);
        }
    }
}