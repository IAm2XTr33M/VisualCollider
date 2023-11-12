#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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

        Vector3Field vector3Field = myInspector.Q<Vector3Field>("padding");
         
        vector3Field.RegisterValueChangedCallback(evt =>
        {
            Vector3 newValue = evt.newValue;
            newValue.x = Mathf.Max(newValue.x, 0.001f);
            newValue.y = Mathf.Max(newValue.y, 0.001f);
            newValue.z = Mathf.Max(newValue.z, 0.001f);

            vector3Field.value = newValue;
            _target.padding = newValue;
        });

        HandleButtons(myInspector);

        return myInspector;
    }

    void HandleButtons(VisualElement myInspector)
    {
        Button createNewbut = myInspector.Q<Button>("CreateNew");
        createNewbut.clicked += CreateNew;

        Button editfiltersBut = myInspector.Q<Button>("EditFilters");
        editfiltersBut.clicked += EditFilters;

        Button forcerefreshBut = myInspector.Q<Button>("ForceRefresh");
        forcerefreshBut.clicked += ForceRefresh;
    }

    void CreateNew()
    {
        _target.CreateNewScriptable();
    }

    void EditFilters()
    {
        _target.EditFilters();
    }

    void ForceRefresh()
    { 
        _target.ManualForceRefresh = true;
    }
}
#endif