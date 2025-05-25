using UnityEngine;
using System.Collections.Generic;

public enum PartType { Body, Head, Hair, Helmet, LeftArm, RightArm }


[System.Serializable]
public class CustomPart
{
    public PartType partType;
    public List<GameObject> options;
    [HideInInspector] public int currentIndex = 0;
}