using System.Collections.Generic;

namespace UncomplicatedSettingsFramework.Api.Features.Extensions
{
    public static class ListExtensions
    {
        /// <summary>
        /// Adds items from the other list to this list, but removes items from the other list that already exist in this list.
        /// </summary>
        public static void AddDistinctRange<T>(this List<T> original, List<T> other)
        {
            foreach (T item in other)
            {
                if (!original.Contains(item))
                {
                    original.Add(item);
                }
            }
        }

        /// <summary>
        /// Adds all from second list, but removes the ones already in the first list before adding (modifies second list).
        /// </summary>
        public static void AddFromFiltered<T>(this List<T> original, List<T> other)
        {
            other.RemoveAll(item => original.Contains(item));
            original.AddRange(other);
        }
    }
}