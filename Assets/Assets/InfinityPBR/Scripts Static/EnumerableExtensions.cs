using System.Collections.Generic;
using UnityEngine;

namespace InfinityPBR
{
    public static class EnumerableExtensions
    {
        public static T TakeRandom<T>(this IList<T> items) => items[Random.Range(0, items.Count)];

        public static IEnumerable<T> TakeRandomXAmount<T>(this IList<T> items, int amount)
        {
            if (amount >= items.Count) amount = items.Count;
            for (int i = 0; i < amount; i++)
                yield return items.TakeRandom();
        }

    }
}