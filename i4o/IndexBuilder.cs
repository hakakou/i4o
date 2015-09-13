using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace i4o
{
    public static class IndexBuilder
    {
        public static IndexSet<TChild> BuildIndicesFor<TChild>(
            ObservableCollection<TChild> observableCollection,
            IndexSpecification<TChild> specification) where TChild : INotifyPropertyChanged 
        {
            return new ObservingIndexSet<TChild>(observableCollection, specification);
        }

        public static IndexSet<TChild> BuildIndicesFor<TChild>(
            IEnumerable<TChild> enumerable,
            IndexSpecification<TChild> specification)
        {
            return new IndexSet<TChild>(enumerable,specification);
        }

        public static IIndex<TChild> GetIndexFor<TChild>(
            IEnumerable<TChild> enumerable,
            PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType.Supports<IComparable>()
                       ? MakeComparisonIndex(enumerable, propertyInfo)
                       : MakeEqualityIndex(propertyInfo, enumerable);
        }

        private static IIndex<TChild> MakeEqualityIndex<TChild>(PropertyInfo propertyInfo, IEnumerable<TChild> enumerable)
        {
            return new EqualityIndex<TChild>(enumerable, propertyInfo);
        }

        private static IIndex<TChild> MakeComparisonIndex<TChild>(IEnumerable<TChild> enumerable, PropertyInfo propertyInfo)
        {
            return (IIndex<TChild>) 
                   Activator.CreateInstance(
                       Type.GetType("i4o.ComparisonIndex`2").MakeGenericType(new[] {typeof (TChild), propertyInfo.PropertyType}),
                       new object[] { enumerable, propertyInfo }
                       );
        }

        internal static bool Supports<T>(this Type type)
        {
            return type.GetInterfaces().Any(i => i == typeof(T));
        }
    }
}