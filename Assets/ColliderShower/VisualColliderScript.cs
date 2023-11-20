#if UNITY_EDITOR
using VC_ConvecHullCalculator;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class VisualColliderScript : MonoBehaviour
{
    Material transparent_material;
    
    public ScriptableVisualCollider scriptableVisualCollider;
    public bool createNewScriptable = false;


    [Header("Settings")]  
    public bool ShowRenders = true;
    public bool ConvexVisualisation = false;
    public bool ManualForceRefresh = false ;
    public bool ExtraRefresh = false;
    public bool SphereClipSizing = false; 
    public enum VisibilityModes { AlwaysVisible, OnGizmos }
    public VisibilityModes visibilityMode = VisibilityModes.AlwaysVisible;
     
    [Min(0.0001f)]
    public Vector3 padding = new Vector3(0.001f, 0.001f, 0.001f);

    [SerializeField] List<Collider> AllCollidersInScene = new List<Collider>();

    [SerializeField] List<VisualColliderFilter> visualColliderFilters = new List<VisualColliderFilter>();

    [SerializeField] List<VisualColliderObject> objectsToRender = new List<VisualColliderObject>();
    [SerializeField] List<Collider> collidersToRender = new List<Collider>();

    [SerializeField] List<MeshCollider> notifiedMeshColliders = new List<MeshCollider>();

    Mesh cubeMesh;
    Mesh sphereMesh;
    Mesh planeMesh;

    bool first = true;
    bool canshow = false;

    string path;

    #region variable changef / On Enable-Disable / hierachy changed

    private void OnEnable()
    {
        string[] guids;
        guids = AssetDatabase.FindAssets("VisualCollider_transparentMat_URP");

        string materialPath = AssetDatabase.GUIDToAssetPath(guids[0]);

        transparent_material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

        EditorApplication.hierarchyChanged += OnHierarchyChanged;

        cubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        sphereMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
        planeMesh = Resources.GetBuiltinResource<Mesh>("Plane.fbx");

    }
    private void OnDisable()
    {
        EditorApplication.hierarchyChanged -= OnHierarchyChanged;
    }
    IEnumerator CheckConvex()
    {
        while (true)
        {
            if (ExtraRefresh)
            {
                Clear();
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
    void CheckScriptableObjectChanged()
    {
        if (scriptableVisualCollider != null && scriptableVisualCollider.changed)
        {
            Clear();
            scriptableVisualCollider.changed = false;
        }
    }
    private void OnValidate()
    {
        if (first && scriptableVisualCollider != null)
        {
            first = false;
            StopAllCoroutines();
            if (!Application.isPlaying)
            {
                StartCoroutine(CheckConvex());
            }
        }

        Clear();
    }
    void OnHierarchyChanged()
    {
        List<Collider> gottenColliders = GetCollidersInScene();
        if (AllCollidersInScene != gottenColliders)
        {
            Clear();
        }
    }

    #endregion
    private void Update()
    {

        if (visibilityMode == VisibilityModes.OnGizmos)
        {
            canshow = SceneView.lastActiveSceneView.drawGizmos;
        }
        else
        {
            canshow = true; 
        }

        CheckScriptableObjectChanged();

        if(!canshow && visibilityMode == VisibilityModes.AlwaysVisible)
        {
            canshow = true;
        }
        if(ManualForceRefresh == true)
        {
            ManualForceRefresh = false;
            objectsToRender.Clear();
            collidersToRender.Clear();
            Clear();
        }

        RenderMeshes();
    }

    public void CreateNewScriptable()
    {
        var guid = AssetDatabase.FindAssets($"t:Script {nameof(VisualColliderScript)}");
        path = AssetDatabase.GUIDToAssetPath(guid[0]);
        path = Path.GetDirectoryName(path);

        if (!AssetDatabase.IsValidFolder(path + "/Filters"))
        { 
            AssetDatabase.CreateFolder(path, "Filters");
        }

        string[] folder = { path + "/Filters" };
        guid = AssetDatabase.FindAssets("ScriptableVCFilter", folder);

        path = path + "/Filters/ScriptableVCFilter" + guid.Length.ToString() + ".asset";

        ScriptableVisualCollider svc = ScriptableVisualCollider.CreateInstance<ScriptableVisualCollider>();

        AssetDatabase.CreateAsset(svc, path);

        scriptableVisualCollider = svc;
         
        EditorUtility.OpenPropertyEditor(scriptableVisualCollider); 
    }
     
    public void EditFilters()  
    {
        if(scriptableVisualCollider != null) 
        {
            EditorUtility.OpenPropertyEditor(scriptableVisualCollider);
        }
    }

    void Clear()
    {
        AllCollidersInScene = GetCollidersInScene();

        if (scriptableVisualCollider != null)
        {
            visualColliderFilters = scriptableVisualCollider.VisualColliderFilters;
            FilterFilters();
        }
    }

    void FilterFilters()
    {
        foreach (var filter in visualColliderFilters)
        {
            if (filter.materialColor == new Color(0, 0, 0, 0))
            {
                filter.materialColor = Color.white;
            }
        }

            List<Collider> colliderSearchList2 = AllCollidersInScene;

        for (int i = 0; i < collidersToRender.Count; i++)
        {
            if (collidersToRender[i] != null && objectsToRender[i] != null)
            {
                if (objectsToRender[i].isMeshCol)
                {
                    if (objectsToRender[i].isConvex != (collidersToRender[i] as MeshCollider).convex)
                    {
                        //ReCreate Mesh if convex changed
                        objectsToRender[i].isConvex = !objectsToRender[i].isConvex;
                        objectsToRender[i].mesh = GetMesh(collidersToRender[i]);
                    }
                }
            }
            else
            {
                //Remove object if collider is gone from scene
                objectsToRender.RemoveAt(i);
                collidersToRender.RemoveAt(i);
            }
        }

        //Check if there is any new colliders in the scene
        foreach (Collider col in colliderSearchList2)
        {
            if (!collidersToRender.Contains(col))
            {

                VisualColliderObject newObject = new VisualColliderObject();
                newObject.gameobject = col.gameObject;
                newObject.mesh = GetMesh(col);

                if (col.GetType() == typeof(MeshCollider))
                {
                    newObject.isMeshCol = true;
                    if ((col as MeshCollider).convex)
                    {
                        newObject.isConvex = true;
                    }
                }

                collidersToRender.Add(col);
                objectsToRender.Add(newObject);
            }
        }

        foreach(var obj in objectsToRender)
        {
            if(obj.filterLayer > visualColliderFilters.Count-1)
            {
                obj.filterLayer = -1;
            }
        }

        int filterIndex = 0;
        List<VisualColliderObject> filteredObjects = new List<VisualColliderObject>();
        foreach (var filter in visualColliderFilters)
        {
            if (filter.isActive)
            {
                switch (filter.filterMode)
                {
                    case VisualColliderFilter.FilterMode.layer:
                        for (int i = 0; i < objectsToRender.Count; i++)
                        {
                            if (!filteredObjects.Contains(objectsToRender[i]))
                            {
                                if(objectsToRender[i].gameobject != null)
                                {
                                    if ((filter.layer.value & 1 << objectsToRender[i].gameobject.layer) > 0)
                                    {
                                        objectsToRender[i].filterLayer = filterIndex;
                                        filteredObjects.Add(objectsToRender[i]);

                                    }
                                    else
                                    {
                                        objectsToRender[i].filterLayer = -1;
                                    }
                                }
                            }
                        }
                        break;
                    case VisualColliderFilter.FilterMode.tag:
                        if (filter.tag != "" && filter.tag != null)
                        {
                            for (int i = 0; i < objectsToRender.Count; i++)
                            {
                                if (!filteredObjects.Contains(objectsToRender[i]))
                                {
                                    string tag = filter.CapitalSensitive ? objectsToRender[i].gameobject.tag : objectsToRender[i].gameobject.tag.ToLower();
                                    string filtertag = filter.CapitalSensitive ? filter.tag : filter.tag.ToLower();

                                    if (filter.nameMode == VisualColliderFilter.NameMode.includesName && tag.Contains(filtertag) ||
                                        filter.nameMode == VisualColliderFilter.NameMode.exactName && tag == filtertag)
                                    {
                                        objectsToRender[i].filterLayer = filterIndex;
                                        filteredObjects.Add(objectsToRender[i]);
                                    }
                                    else
                                    {
                                        objectsToRender[i].filterLayer = -1;
                                    }
                                }
                            }
                        }
                        break;
                    case VisualColliderFilter.FilterMode.mesh:
                        if (filter.mesh != null)
                        {
                            for (int i = 0; i < objectsToRender.Count; i++)
                            {
                                if (!filteredObjects.Contains(objectsToRender[i]) && objectsToRender[i] != null)
                                {
                                    if (collidersToRender[i].GetType() == typeof(MeshCollider))
                                    {
                                        if (filter.mesh == (collidersToRender[i] as MeshCollider).sharedMesh)
                                        {
                                            objectsToRender[i].filterLayer = filterIndex;
                                            filteredObjects.Add(objectsToRender[i]);
                                        }
                                        else
                                        {
                                            objectsToRender[i].filterLayer = -1;
                                        }
                                    }
                                    else
                                    {
                                        if (objectsToRender[i].gameobject.GetComponent<MeshFilter>() != null)
                                        {
                                            if (filter.mesh == objectsToRender[i].gameobject.GetComponent<MeshFilter>().sharedMesh)
                                            {
                                                objectsToRender[i].filterLayer = filterIndex;
                                                filteredObjects.Add(objectsToRender[i]);
                                            }
                                            else
                                            {
                                                objectsToRender[i].filterLayer = -1;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case VisualColliderFilter.FilterMode.name:
                        if (filter.name != "" && filter.name != null)
                        {
                            for (int i = 0; i < objectsToRender.Count; i++)
                            {
                                if (!filteredObjects.Contains(objectsToRender[i]) && objectsToRender[i] != null)
                                {
                                    string name = filter.CapitalSensitive ? objectsToRender[i].gameobject.name : objectsToRender[i].gameobject.name.ToLower();
                                    string filtername = filter.CapitalSensitive ? filter.name : filter.name.ToLower();

                                    if (filter.nameMode == VisualColliderFilter.NameMode.includesName && name.Contains(filtername) ||
                                        filter.nameMode == VisualColliderFilter.NameMode.exactName && name == filtername)
                                    {
                                        objectsToRender[i].filterLayer = filterIndex;
                                        filteredObjects.Add(objectsToRender[i]);
                                    }
                                    else
                                    {
                                        objectsToRender[i].filterLayer = -1;
                                    }
                                }
                            }
                        }
                        break;
                    case VisualColliderFilter.FilterMode.prefab:
                        if (filter.prefab != null)
                        {
                            List<GameObject> allPrefabs = new List<GameObject>(PrefabUtility.FindAllInstancesOfPrefab(filter.prefab));
                            for (int i = 0; i < objectsToRender.Count; i++)
                            {
                                if (!filteredObjects.Contains(objectsToRender[i]) && objectsToRender[i] != null)
                                {
                                    if (allPrefabs.Contains(objectsToRender[i].gameobject))
                                    {
                                        //foreachobj
                                        for (int j = 0; j < objectsToRender.Count; j++)
                                        {
                                            if (!filteredObjects.Contains(objectsToRender[j]) && collidersToRender[j] != null && collidersToRender[i] != null)
                                            {
                                                if (collidersToRender[j].transform.IsChildOf(collidersToRender[i].transform))
                                                {
                                                    objectsToRender[j].filterLayer = filterIndex;
                                                    filteredObjects.Add(objectsToRender[j]);
                                                }
                                            }
                                        }


                                        objectsToRender[i].filterLayer = filterIndex;
                                        filteredObjects.Add(objectsToRender[i]);
                                    }
                                    else
                                    {
                                        objectsToRender[i].filterLayer = -1;
                                    }
                                }
                            }
                        }
                        break;
                    case VisualColliderFilter.FilterMode.position:
                        float pos = 0;
                        for (int i = 0; i < objectsToRender.Count; i++)
                        {
                            if (!filteredObjects.Contains(objectsToRender[i]) && objectsToRender[i] != null)
                            {
                                switch (filter.positionAxis)
                                {
                                    case VisualColliderFilter.PositionAxis.x: pos = objectsToRender[i].gameobject.transform.position.x; break;
                                    case VisualColliderFilter.PositionAxis.y: pos = objectsToRender[i].gameobject.transform.position.y; break;
                                    case VisualColliderFilter.PositionAxis.z: pos = objectsToRender[i].gameobject.transform.position.z; break;
                                }
                                bool isCorrect = false;
                                switch (filter.positionMode)
                                {
                                    case VisualColliderFilter.PositionMode.GreaterThen: isCorrect = pos > filter.position; break;
                                    case VisualColliderFilter.PositionMode.GreaterOrEqualTo: isCorrect = pos >= filter.position; break;
                                    case VisualColliderFilter.PositionMode.EqualTo: isCorrect = pos == filter.position; break;
                                    case VisualColliderFilter.PositionMode.LessOrEqualTo: isCorrect = pos <= filter.position; break;
                                    case VisualColliderFilter.PositionMode.LessThan: isCorrect = pos < filter.position; break;
                                    case VisualColliderFilter.PositionMode.InBetween:
                                        isCorrect = pos > filter.positionInbetween.x && pos < filter.positionInbetween.y; break;
                                }
                                if (isCorrect)
                                {
                                    objectsToRender[i].filterLayer = filterIndex;
                                    filteredObjects.Add(objectsToRender[i]);
                                }
                                else
                                {
                                    objectsToRender[i].filterLayer = -1;
                                }
                            }
                        }
                        break;
                }
            }
            filterIndex++; 
        }
         
    }  
     
    void RenderMeshes()
    {
        for (int i = 0; i < objectsToRender.Count; i++)
        {
            if (objectsToRender[i].filterLayer != -1 && canshow && ShowRenders )
            {
                if (visualColliderFilters[objectsToRender[i].filterLayer].isActive && !visualColliderFilters[objectsToRender[i].filterLayer].exclude)
                {
                    Material mat = Material.Instantiate(transparent_material);
                    mat.color = visualColliderFilters[objectsToRender[i].filterLayer].materialColor;

                    RenderParams rp = new RenderParams(mat);

                    if (i < collidersToRender.Count)
                    {
                        Collider col = collidersToRender[i];
                        if (col.GetType() == typeof(MeshCollider))
                        {
                            if (objectsToRender[i].gameobject != null && objectsToRender[i].mesh != null)
                            {
                                Transform objTransform = objectsToRender[i].gameobject.transform;
                                Graphics.RenderMesh(rp, objectsToRender[i].mesh, 0, Matrix4x4.TRS(objTransform.position, objTransform.rotation, objTransform.localScale));
                            }
                        }
                        else if (col.GetType() == typeof(BoxCollider))
                        {
                            if (objectsToRender[i].gameobject != null && col != null)
                            {
                                Transform objTransform = objectsToRender[i].gameobject.transform;
                                Graphics.RenderMesh(rp, objectsToRender[i].mesh, 0, Matrix4x4.TRS(col.bounds.center, objTransform.rotation, col.bounds.size + padding));
                            }
                        }
                        else if (col.GetType() == typeof(SphereCollider))
                        {
                            if (objectsToRender[i].gameobject != null)
                            {
                                Transform objTransform = objectsToRender[i].gameobject.transform;

                                if (SphereClipSizing)
                                {
                                    Graphics.RenderMesh(rp, objectsToRender[i].mesh, 0, Matrix4x4.TRS(
                                        col.bounds.center, objTransform.rotation, col.bounds.size / 2f + col.bounds.size * 0.0065f + padding));
                                }
                                else
                                {
                                    Graphics.RenderMesh(rp, objectsToRender[i].mesh, 0, Matrix4x4.TRS(
                                        col.bounds.center, objTransform.rotation, col.bounds.size / 2f + padding));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public List<Vector3> GetMeshVertices(Mesh mesh)
    {
        List<Vector3> verticesList = new List<Vector3>();

        if (mesh != null)
        {
            verticesList.AddRange(mesh.vertices);
        }

        return verticesList;
    }
    List<Collider> GetCollidersInScene()
    {
        return new List<Collider>(FindObjectsOfType<Collider>());
    }

    Mesh GetMesh(Collider col)
    {
        if (col.GetType() == typeof(MeshCollider))
        {
            MeshCollider meshCol = col.gameObject.GetComponent<MeshCollider>();
            if (meshCol.convex && ConvexVisualisation)
            {
                if (!meshCol.sharedMesh.isReadable)
                {
                    var guid = AssetDatabase.FindAssets(meshCol.sharedMesh.name);
                    SetFBXIsReadable(AssetDatabase.GUIDToAssetPath(guid[0]));
                }

                var calc = new VisualCollider_ConvexHullCalculator();
                var verts = new List<Vector3>();
                var tris = new List<int>();
                var normals = new List<Vector3>();
                var points = GetMeshVertices(meshCol.sharedMesh);

                calc.GenerateHull(points, true, ref verts, ref tris, ref normals);

                var mesh = new Mesh();
                mesh.SetVertices(verts);
                mesh.SetTriangles(tris, 0);
                mesh.SetNormals(normals);

                return mesh;
            }
            else
            {
                if(meshCol.convex && !meshCol.sharedMesh.isReadable && ConvexVisualisation && !notifiedMeshColliders.Contains(meshCol))
                {

                    Debug.Log("Sorry but the Mesh " + meshCol.sharedMesh.name + " on " + meshCol.gameObject.name + " is not readable, " +
                        "Its impossible to visualise convex meshes if the mesh.isReadable is false on import, Please turn off the convex " +
                        "visualisation or use a readable mesh!   (Instead I visualised the collider as if it werent convex)");
                    notifiedMeshColliders.Add(meshCol);
                }
                return meshCol.sharedMesh;
            }
        }
        else if (col.GetType() == typeof(BoxCollider))
        {
            return cubeMesh;
        }
        else if (col.GetType() == typeof(SphereCollider))
        {
            return sphereMesh;
        }
        else if (col.GetType() == typeof(CapsuleCollider))
        {
            return cubeMesh;
        }
        return cubeMesh; 
    }

    public static void SetFBXIsReadable(string filePath)
    {
        ModelImporter modelImporter = AssetImporter.GetAtPath(filePath) as ModelImporter;

        if (modelImporter != null)
        {
            modelImporter.isReadable = true;

            AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
        }
    }
}
#endif
