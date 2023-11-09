using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VisualColliderFilter
{
    public enum FilterMode { layer, tag, mesh, prefab, name, position };
    public FilterMode filterMode;

    public LayerMask layer;

    public string tag;

    public Mesh mesh;

    public GameObject prefab;

    public string name;
    public enum NameMode { includesName, exactName };
    public NameMode nameMode;
    public bool CapitalSensitive;

    public enum PositionMode { LessThan , GreaterThen , EqualTo , GreaterOrEqualTo , LessOrEqualTo , InBetween};
    public PositionMode positionMode;
    public enum PositionAxis { x , y , z};
    public PositionAxis positionAxis;

    public float position;
    public Vector2 positionInbetween;

    public Color materialColor = new Color(0,0,0,0);

    public bool exclude = false;
    public bool isActive = true;
}