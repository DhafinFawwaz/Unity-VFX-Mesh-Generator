using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshGenerator), true)]
public class MeshGeneratorInspector: Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MeshGenerator meshGenerator = (MeshGenerator)target;
        if (GUILayout.Button("Save"))
        {
            meshGenerator.SaveMesh();
        }
    }
}

[CustomEditor(typeof(FreeConeMeshGenerator), true)]
public class FreeConeMeshGeneratorInspector: Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        FreeConeMeshGenerator meshGenerator = (FreeConeMeshGenerator)target;

        GUILayout.BeginHorizontal();
        
        if(GUILayout.Button("Linearize"))
            meshGenerator.EditVerticesByEquation(x => x);
        else if(GUILayout.Button("Circulize"))
            meshGenerator.EditVerticesByEquation(x => Mathf.Pow(1-4*(x-0.5f)*(x-0.5f), 0.5f));
        else if(GUILayout.Button("Parabolize"))
            meshGenerator.EditVerticesByEquation(x => Mathf.Pow(x, 0.5f));
        
        GUILayout.EndHorizontal();
        
        if(GUILayout.Button("Save"))
        {
            meshGenerator.SaveMesh();
        }
    }
}