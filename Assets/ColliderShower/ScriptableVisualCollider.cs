#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ScriptableVisualCollider : ScriptableObject
{
    public Material TransParentMaterial;
    public List<VisualColliderFilter> VisualColliderFilters = new List<VisualColliderFilter>();

    [HideInInspector]
    public bool changed;

    private void OnValidate()
    {
        changed = true;
    }
}
#endif
