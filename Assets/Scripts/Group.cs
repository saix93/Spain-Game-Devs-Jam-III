using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Group
{
    public int Value;
    public bool RandomlyGenerated;
    public List<Character> Characters;

    public Group()
    {
        Characters = new List<Character>();
        RandomlyGenerated = false;
        Value = 0;
    }

    public Group(List<Character> newCharacters)
    {
        Characters = new List<Character>(newCharacters);
        RandomlyGenerated = Characters.Exists(c => c.PlacedRandomly);
        Value = EvaluateGroupValue();
    }

    public void ReEvaluateGroupValue()
    {
        Value = EvaluateGroupValue();
    }

    private int EvaluateGroupValue()
    {
        var finalValue = 0;
        var traits = new List<SO_Trait>();

        if (Characters.Count < 2) return 0;

        foreach (var ch in Characters)
        {
            if (ch.IsPriest) return -100; // TODO: Quizá se pueda hacer que si el número de traits en común es muy grande, el cura se case igualmente
            traits.AddRange(ch.Traits);
        }

        foreach (var t in traits.Distinct())
        {
            var n = traits.Count(x => x.ID == t.ID);
            if (n > 1) finalValue += t.Value;
        }

        return finalValue;
    }
}
