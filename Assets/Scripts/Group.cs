using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Group
{
    public int Value;
    public List<Character> Characters;

    public Group()
    {
        Characters = new List<Character>();
        Value = 0;
    }

    public Group(List<Character> newCharacters)
    {
        Characters = new List<Character>(newCharacters);
        Value = EvaluateGroupValue();
    }

    private int EvaluateGroupValue()
    {
        var finalValue = 0;
        var traits = new List<SO_Trait>();

        if (Characters.Count < 2) return 0;

        foreach (var ch in Characters)
        {
            traits.AddRange(ch.Traits);
        }

        // traits = traits.OrderBy(x => x.ID).ToList();

        foreach (var t in traits.Distinct())
        {
            var n = traits.Count(x => x.ID == t.ID);
            if (n > 1) finalValue += t.Value * n;
        }

        return finalValue;
    }
}
