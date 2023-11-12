#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Unity.VisualScripting;

[CustomEditor(typeof(ScriptableVisualCollider))]
public class VisualColliderFilter_Inspector : Editor
{
    private VisualElement PropertyContainer;
    [SerializeField] VisualTreeAsset visualTree;

    ScriptableVisualCollider _target;

    public override VisualElement CreateInspectorGUI()
    {
        _target = (ScriptableVisualCollider)target;

        VisualElement myInspector = new VisualElement();

        visualTree.CloneTree(myInspector);


        return myInspector;
    }

}
#endif