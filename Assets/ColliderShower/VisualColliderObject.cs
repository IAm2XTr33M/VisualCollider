using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VisualColliderObject
{
    public Mesh mesh;
    public Material mat;
    public GameObject gameobject;

    public int filterLayer = -1;

    public bool isMeshCol = false;
    public bool isConvex = false;
}
