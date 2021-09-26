using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Sadness level")]
public class SO_SadnessLevel : ScriptableObject
{
    public int Value;
    public SadnessLevel SadnessLevel;
}

public enum SadnessLevel
{
    Low,
    Medium,
    High,
    Extreme
}
