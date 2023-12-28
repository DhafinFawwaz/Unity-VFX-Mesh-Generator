using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalfRingMeshGenerator: MeshGenerator
{
    [SerializeField] int _sides = 32;
    [SerializeField] float _innerRadius = 0.3f;
    [SerializeField] float _outerRadius = 1f;
    [SerializeField] float _angle = 315f;
    [SerializeField] float _height = 0.3f;

    public int Sides { get => _sides; set => _sides = value; }
    public float InnerRadius { get => _innerRadius; set => _innerRadius = value; }
    public float OuterRadius { get => _outerRadius; set => _outerRadius = value; }
    public float Angle { get => _angle; set => _angle = value; }
    public override void Draw()
    {
        Vertices = CalculateVertices();
        Triangles = CalculateTriangles(Vertices);
        UVs = CalculateUVs(Vertices);

        Mesh.Clear();
        Mesh.vertices = Vertices;
        Mesh.triangles = Triangles;
        Mesh.uv = UVs;
        Mesh.RecalculateNormals();
    }
    protected override Vector3[] CalculateVertices()
    {
        List<Vector3> outerVerts = GetCircumferencesVertices(_sides, _outerRadius, _angle);
        List<Vector3> innerVerts = GetCircumferencesVertices(_sides, _innerRadius, _angle);

        List<Vector3> innerBottomVerts = new List<Vector3>();
        List<Vector3> innerTopVerts = new List<Vector3>();
        for(int i = 0; i < innerVerts.Count; i++)
        {
            innerBottomVerts.Add(new Vector3(innerVerts[i].x, innerVerts[i].y, -_height/2));
            innerTopVerts.Add(new Vector3(innerVerts[i].x, innerVerts[i].y, +_height/2));
        }
        
        List<Vector3> vertsList = new List<Vector3>();
        vertsList.AddRange(outerVerts);
        vertsList.AddRange(innerBottomVerts);
        vertsList.AddRange(innerTopVerts);

        return vertsList.ToArray();
    }
    protected override int[] CalculateTriangles(Vector3[] verts)
    {
        int sides = verts.Length/3;
        List<int> newTriangles = new List<int>();
        for (int i = 0; i < sides-1; i++)
        {
            int outerIndex = i;
            int innerIndex = i + sides;

            int nextIndex = (i + 1) % sides;
            int nextOuterIndex = nextIndex;
            int nextInnerIndex = nextIndex + sides;

            int innerBottomIndex = i + sides + sides;
            int nextInnerBottomIndex = nextInnerIndex + sides;
            
            if(!FlipNormals)
            {
                newTriangles.AddRange(new int[] { nextInnerIndex, nextOuterIndex, outerIndex, });
                newTriangles.AddRange(new int[] { innerIndex, nextInnerIndex, outerIndex, });

                newTriangles.AddRange(new int[] { outerIndex, nextOuterIndex, nextInnerBottomIndex, });
                newTriangles.AddRange(new int[] { outerIndex, nextInnerBottomIndex, innerBottomIndex, });
            }
            else
            {
                newTriangles.AddRange(new int[] { outerIndex, nextOuterIndex, nextInnerIndex });
                newTriangles.AddRange(new int[] { outerIndex, nextInnerIndex, innerIndex });
                
                newTriangles.AddRange(new int[] { nextInnerBottomIndex, nextOuterIndex, outerIndex });
                newTriangles.AddRange(new int[] { innerBottomIndex, nextInnerBottomIndex, outerIndex });
            }
        }

        return newTriangles.ToArray();
    }
    protected override Vector2[] CalculateUVs(Vector3[] verts)
    {
        int sides = verts.Length/3;
        sides -= 1; // -1 to handle the seam

        List<Vector2> newUVs = new List<Vector2>();
        for (int i = 0; i < sides; i++)
        {
            newUVs.Add(new Vector2(1, i/(float)(sides)));
        }
        // Extra uv to handle the seam
        newUVs.Add(new Vector2(1, 1));
        for (int i = 0; i < sides; i++)
        {
            newUVs.Add(new Vector2(0, i/(float)(sides)));
        }
        // Extra uv to handle the seam
        newUVs.Add(new Vector2(0, 1));

        for (int i = 0; i < sides; i++)
        {
            newUVs.Add(new Vector2(0, i/(float)(sides)));
        }
        // Extra uv to handle the seam
        newUVs.Add(new Vector2(0, 1));


        return newUVs.ToArray();
    }

#if UNITY_EDITOR
    protected override void OnEditorUpdate()
    {
        if(RestrictUnsafeValues)
        {
            if(_innerRadius > _outerRadius) _innerRadius = _outerRadius;
            if(_innerRadius < 0)
            {
                _innerRadius = 0;
                Debug.LogWarning("Inner radius cannot be less than 0!");
            }
            if (_outerRadius < 0)
            {
                _outerRadius = 0;
                Debug.LogWarning("Outer radius cannot be less than 0!");
            }
            if(_outerRadius < _innerRadius) _outerRadius = _innerRadius;

            if(_angle < 0 || _angle > 360)
            {
                _angle = Mathf.Clamp(_angle, 0, 360);
                Debug.LogWarning("Angle cannot be less than 0 or more than 360!");
            }

            if (_sides < 3)
            {
                _sides = 3;
                Debug.LogWarning("Sides cannot be less than 3!");
            }
            if (_sides > 4096)
            {
                _sides = 4096;
                Debug.LogWarning("Too many sides might cause performance issue! Disable \"Restrict Unsafe Values\" to bypass this warning.");
            }
        }

        base.OnEditorUpdate();
    }
#endif
}
