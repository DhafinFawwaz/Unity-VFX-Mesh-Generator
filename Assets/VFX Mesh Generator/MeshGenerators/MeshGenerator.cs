using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.EditorTools;


#if UNITY_EDITOR
using UnityEditor;
[ExecuteInEditMode]
#endif
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public abstract class MeshGenerator: MonoBehaviour
{
    MeshRenderer _meshRenderer;
    MeshFilter _meshFilter;

    protected Mesh Mesh;
    protected Vector3[] Vertices;
    protected Vector2[] UVs;
    protected int[] Triangles;
    
    public bool FlipNormals = false;
    
    // Draw the mesh. Careful if called in runtime, it will cause performance issue if the amount of vertices is a lot.
    public abstract void Draw();
    protected abstract Vector3[] CalculateVertices();
    protected abstract int[] CalculateTriangles(Vector3[] verts);
    protected abstract Vector2[] CalculateUVs(Vector3[] verts);


    // Helper
    protected List<Vector3> GetCircumferencesVertices(int sides, float radius, float angle = 360f, float height = 0f)
    {
        List<Vector3> points = new List<Vector3>();
        float circumferenceProgress = 1f / (sides-1);
        float TAU = 2f*Mathf.PI * angle/360f;
        float radianProgress = circumferenceProgress*TAU;
        for (int i = 0; i < sides; i++)
        {
            float radian = radianProgress * i;
            float x = Mathf.Cos(radian) * radius;
            float y = Mathf.Sin(radian) * radius;
            points.Add(new Vector3(x, y, height));
        }
        return points;
    }

#if UNITY_EDITOR
    [Tooltip("Recalculate mesh automatically when editing properties. Might cause performance issue if the amount of vertices is a lot.")]
    [SerializeField] bool _autoUpdate = true;
    [Tooltip("Example: Making too many vertices, negatif amount of vertices.")]
    [SerializeField] bool _restrictUnsafeValues = true;
    protected bool RestrictUnsafeValues => _restrictUnsafeValues;
    void OnEnable()
    {
        EditorApplication.update += OnEditorUpdate;
        Initialize();       
    }
    void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
    }
    void Start()
    {
        // Outside Initialize becase its not required
        if(_meshRenderer.sharedMaterial == null) 
            _meshRenderer.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
    }
    void Initialize()
    {
        if (_meshFilter == null) _meshFilter = GetComponent<MeshFilter>();
        if (_meshRenderer == null) _meshRenderer = GetComponent<MeshRenderer>();
        if(Mesh == null)
        {
            Mesh = new Mesh();
            _meshFilter.sharedMesh = Mesh;
        }
        if(_meshFilter.sharedMesh != Mesh) _meshFilter.sharedMesh = Mesh;
    }

    // override to handle invalid values
    protected virtual void OnEditorUpdate()
    {
        Initialize();
        if(_autoUpdate) Draw();
    }
    
    public void SaveMesh()
    {
        if(Mesh == null)
        {
            Debug.LogError("Mesh is null!");
            return;
        }
        string path = EditorUtility.SaveFilePanel("Save mesh", "Assets", "Mesh", "asset");
        if (path.Length == 0) return;
        path = FileUtil.GetProjectRelativePath(path);
        AssetDatabase.CreateAsset(Mesh, path);
        AssetDatabase.SaveAssets();
        Debug.Log("Mesh saved successfully!");
    }
    
#endif
}
