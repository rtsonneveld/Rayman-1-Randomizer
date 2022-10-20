using System;
using System.Collections.Generic;
using System.Linq;

namespace Rayman1Randomizer;

public static class RandomExtensions
{
    public static T NextWeighedItem<T>(this Random random, List<(T Item, float Weight)> items)
    {
        if (items.Count == 0)
            throw new ArgumentException("The items collection can not be empty", nameof(items));

        items.Sort((itemA, itemB) => (itemA.Weight == itemB.Weight) ? 0 : (itemA.Weight > itemB.Weight ? 1 : -1));
        float sumWeight = items.Sum(i => i.Weight);
        float randomWeight = (float)random.NextDouble() * sumWeight;
        float currentSum = 0;

        foreach (var entry in items)
        {
            currentSum += entry.Weight;

            if (currentSum > randomWeight)
                return entry.Item;
        }

        return items[0].Item;
    }
}