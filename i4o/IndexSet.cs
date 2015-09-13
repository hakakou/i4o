using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace i4o
{
    public class IndexSet<T> : IEnumerable<T>
    {
        protected readonly IndexSpecification<T> IndexSpecification;
        protected readonly Dictionary<string, IIndex<T>> IndexDictionary
            = new Dictionary<string, IIndex<T>>();

        public IndexSet(IndexSpecification<T> indexSpecification)
        {
            IndexSpecification = indexSpecification;
        }

        public IndexSet(IEnumerable<T> source, IndexSpecification<T> indexSpecification)
        {
            IndexSpecification = indexSpecification;
            SetupIndices(source);
        }

        protected void SetupIndices(IEnumerable<T> source)
        {
            IndexSpecification.IndexedProperties.ForEach(
                propName =>
                  IndexDictionary.Add(propName, IndexBuilder.GetIndexFor(source, typeof(T).GetProperty(propName)))
            );
        }

        protected virtual void BeforeIndexAccess()
        {
        }

        public IEnumerator<T> GetEnumerator()
        {
            BeforeIndexAccess();

            if (IndexSpecification.IndexedProperties.Count > 0)
                return IndexDictionary[IndexSpecification.IndexedProperties[0]].GetEnumerator();

            throw new InvalidOperationException("Can't enumerate without at least one index");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal IEnumerable<T> WhereUsingIndex(Expression<Func<T, bool>> predicate)
        {
            BeforeIndexAccess();

            if (
                BodyIsBinary(predicate) &&

                (predicate.Body.NodeType == ExpressionType.Equal
                  || predicate.Body.NodeType == ExpressionType.LessThan
                  || predicate.Body.NodeType == ExpressionType.LessThanOrEqual
                  || predicate.Body.NodeType == ExpressionType.GreaterThan
                  || predicate.Body.NodeType == ExpressionType.GreaterThanOrEqual) &&
                
                LeftSideIsMemberExpression(predicate) &&
                LeftSideMemberIsIndexed(predicate)
               )
                return IndexDictionary[LeftSide(predicate).Member.Name].WhereThroughIndex(predicate);

            throw new Exception("No index for " + LeftSide(predicate).Member.Name);
        }

        private static MemberExpression LeftSide(Expression<Func<T, bool>> predicate)
        {
            return ((MemberExpression)((BinaryExpression)predicate.Body).Left);
        }

        private bool LeftSideMemberIsIndexed(Expression<Func<T, bool>> predicate)
        {
            return (IndexSpecification.IndexedProperties.Contains(
                ((MemberExpression)((BinaryExpression)predicate.Body).Left
                ).Member.Name));
        }

        private static bool LeftSideIsMemberExpression(Expression<Func<T, bool>> predicate)
        {
            return ((((BinaryExpression)predicate.Body)).Left is MemberExpression);
        }

        private static bool BodyIsBinary(Expression<Func<T, bool>> predicate)
        {
            return (predicate.Body is BinaryExpression);
        }
    }
}