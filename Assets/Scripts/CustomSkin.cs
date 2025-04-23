using UnityEngine;
using System.Collections.Generic;

public enum PartType { Body, Eyes, Head, Mouth }

[System.Serializable]
public class CustomPart
{
    public PartType partType;
    public MeshFilter renderer;
    public List<Mesh> options;
    [HideInInspector] public int currentIndex = 0;
}