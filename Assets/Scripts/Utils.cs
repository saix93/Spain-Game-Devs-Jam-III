using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static SO_SadnessLevel CalculateSadness(List<SO_SadnessLevel> sadnessLevels, Character character)
    {
        SO_SadnessLevel sLevel = null;

        foreach (var level in sadnessLevels)
        {
            if (character.NumberOfUnions >= level.Value)
            {
                sLevel = level;
            }
        }

        return sLevel;
    }
    public static List<string> GetAllAvailableNames()
    {
        var names = new List<string>
        {
            "A",
            "B",
            "C",
            "D",
            "E",
            "F",
            "G",
            "H",
            "I",
            "J",
            "K",
            "L",
            "M",
            "N",
            "O",
            "P",
            "Q",
            "R",
            "S",
            "T",
            "U",
            "V",
            "W",
            "X",
            "Y",
            "Z"
        };

        return names;
    }
    public static List<int> GetDifferentRandomNumbers(int from, int to, int iterations)
    {
        var arr = new List<int>();

        for (var i = 0; i < iterations; i++)
        {
            arr.Add(GetRandomValueExcept(arr, from, to));
        }
        
        return arr;
    }
    public static T GetRandomValueExcept<T>(List<T> list, int min, int max)
    {
        T value;

        do
        {
            value = GetRandomValue<T>(min, max);
        } while (list.Contains(value));

        return value;
    }
    public static T GetRandomValue<T>(int min, int max)
    {
        object value = null;
        
        if (typeof(T) == typeof(int))
        {
            value = Random.Range(min, max);
        }

        return (T) value;
    }
}
