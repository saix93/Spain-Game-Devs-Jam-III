using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Trait")]
public class SO_Trait : ScriptableObject
{
    [UniqueIdentifier]
    public int ID;
    public string Name;
    public int Value;
    public Sprite Icon;
}
