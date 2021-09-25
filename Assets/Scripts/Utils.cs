using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public static class Utils
{
    public static SO_SadnessLevel CalculateSadness(Character character)
    {
        SO_SadnessLevel sLevel = null;

        foreach (var level in GameManager._.AllSadnessLevels.List)
        {
            if (character.SadnessPoints >= level.Value)
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
    /// <summary>
    /// Destroys all children of the given transform
    /// </summary>
    /// <param name="transform"></param>
    public static void DestroyChildren(Transform transform)
    {
        foreach (Transform child in transform)
        {
            Object.Destroy(child.gameObject);
        }
    }
}

[Serializable]
public class MinMaxInt
{
    public int Min;
    public int Max;
    
    public MinMaxInt(int min, int max)
    {
        Min = min;
        Max = max;
    }
}
[Serializable]
public class MinMaxFloat
{
    public float Min;
    public float Max;
    
    public MinMaxFloat(float min, float max)
    {
        Min = min;
        Max = max;
    }
}
