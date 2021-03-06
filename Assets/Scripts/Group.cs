using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Group
{
    public int Value;
    public bool HasPriest;
    public bool RandomlyGenerated;
    public bool SadnessAddedThisRound;
    public List<Character> Characters;

    public Group()
    {
        Characters = new List<Character>();
        RandomlyGenerated = false;
        SadnessAddedThisRound = false;
        Value = 0;
    }

    public Group(List<Character> newCharacters)
    {
        Characters = new List<Character>(newCharacters);
        RandomlyGenerated = Characters.Exists(c => c.PlacedRandomly);
        HasPriest = Characters.Exists(c => c.IsPriest);
        Value = EvaluateGroupValue();
    }

    public Vector3 GetMiddlePosition()
    {
        var chairs = new List<Chair>();
        chairs.Add(Characters[0].AssignedChair);
        chairs.AddRange(Characters[0].AssignedChair.LinkedChairs);
        
        return Vector3.Lerp(chairs[0].GetCharacterPosition(), chairs[1].GetCharacterPosition(), .5f);
    }

    public void ReEvaluateGroupValue()
    {
        Value = EvaluateGroupValue();
    }
    private int EvaluateGroupValue()
    {
        var finalValue = 0;
        var traits = new List<SO_Trait>();

        foreach (var ch in Characters)
        {
            if (ch.IsPriest) {
                HasPriest = true;
                return -100;
            }
            
            traits.AddRange(ch.Traits);
        }

        if (Characters.Count < 2) return 0;

        foreach (var t in traits.Distinct())
        {
            var n = traits.Count(x => x.ID == t.ID);
            if (n > 1) finalValue += t.Value;
        }

        // TODO: Quizá se pueda hacer que si el número de traits en común es muy grande, el cura se case igualmente
        return finalValue;
    }
}
