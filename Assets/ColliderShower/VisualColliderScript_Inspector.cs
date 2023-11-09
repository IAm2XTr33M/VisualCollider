using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Unity.VisualScripting;

[CustomEditor(typeof(VisualColliderScript))]
public class VisualColliderScript_Inspector : Editor
{
    private VisualElement PropertyContainer;
    [SerializeField] VisualTreeAsset visualTree;

    VisualColliderScript _target;

    public override VisualElement CreateInspectorGUI()
    {
        _target = (VisualColliderScript)target;

        VisualElement myInspector = new VisualElement();

        visualTree.CloneTree(myInspector);

        HandleButtons(myInspector);

        return myInspector;
    }

    void HandleButtons(VisualElement myInspector)
    {
        Button createNewbut = myInspector.Q<Button>("CreateNew");
        createNewbut.clicked += CreateNew;

        Button forcerefreshBut = myInspector.Q<Button>("ForceRefresh");
        forcerefreshBut.clicked += ForceRefresh;
    }

    void CreateNew()
    {
        _target.createNewScriptable = true;
    }

    void ForceRefresh()
    { 
        _target.ManualForceRefresh = true;
    }
}