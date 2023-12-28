using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingMeshGenerator: MeshGenerator
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
    public float Height { get => _height; set => _height = value; }
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

        List<Vector3> middleBottomVerts = new List<Vector3>();
        List<Vector3> middleTopVerts = new List<Vector3>();
        for(int i = 0; i < innerVerts.Count; i++)
        {
            float xAvg = (innerVerts[i].x+outerVerts[i].x)/2;
            float yAvg = (innerVerts[i].y+outerVerts[i].y)/2;
            middleBottomVerts.Add(new Vector3(xAvg, yAvg, -_height/2));
            middleTopVerts.Add(new Vector3(xAvg, yAvg, +_height/2));
        }
        
        List<Vector3> vertsList = new List<Vector3>();
        vertsList.AddRange(outerVerts);
        vertsList.AddRange(middleBottomVerts);
        vertsList.AddRange(middleTopVerts);
        vertsList.AddRange(innerVerts);

        return vertsList.ToArray();
    }
    protected override int[] CalculateTriangles(Vector3[] verts)
    {
        int sides = verts.Length/4;
        List<int> newTriangles = new List<int>();
        for (int i = 0; i < sides-1; i++)
        {
            int outerIndex = i;
            int middleIndex = i + sides;

            int nextIndex = (i + 1) % sides;
            int nextOuterIndex = nextIndex;
            int nextMiddleIndex = nextIndex + sides;

            int middleBottomIndex = i + sides + sides;
            int nextMiddleBottomIndex = nextMiddleIndex + sides;

            int innerIndex = middleBottomIndex + sides;
            int nextInnerIndex = nextMiddleBottomIndex + sides;

            if(!FlipNormals)
            {
                newTriangles.AddRange(new int[] { nextMiddleIndex, nextOuterIndex, outerIndex, });
                newTriangles.AddRange(new int[] { middleIndex, nextMiddleIndex, outerIndex, });

                newTriangles.AddRange(new int[] { outerIndex, nextOuterIndex, nextMiddleBottomIndex, });
                newTriangles.AddRange(new int[] { outerIndex, nextMiddleBottomIndex, middleBottomIndex, });

                newTriangles.AddRange(new int[] { nextInnerIndex, nextMiddleIndex, middleIndex, });
                newTriangles.AddRange(new int[] { innerIndex, nextInnerIndex, middleIndex, });

                newTriangles.AddRange(new int[] { middleBottomIndex, nextMiddleBottomIndex, nextInnerIndex, });
                newTriangles.AddRange(new int[] { middleBottomIndex, nextInnerIndex, innerIndex, });
            }
            else
            {
                newTriangles.AddRange(new int[] { outerIndex, nextOuterIndex, nextMiddleIndex });
                newTriangles.AddRange(new int[] { outerIndex, nextMiddleIndex, middleIndex });
                
                newTriangles.AddRange(new int[] { nextMiddleBottomIndex, nextOuterIndex, outerIndex });
                newTriangles.AddRange(new int[] { middleBottomIndex, nextMiddleBottomIndex, outerIndex });
                
                newTriangles.AddRange(new int[] { middleIndex, nextMiddleIndex, nextInnerIndex });
                newTriangles.AddRange(new int[] { middleIndex, nextInnerIndex, innerIndex });
                
                newTriangles.AddRange(new int[] { nextInnerIndex, nextMiddleBottomIndex, middleBottomIndex });
                newTriangles.AddRange(new int[] { innerIndex, nextInnerIndex, middleBottomIndex });
            }
            
        }

        return newTriangles.ToArray();
    }
    protected override Vector2[] CalculateUVs(Vector3[] verts)
    {
        int sides = verts.Length/4;
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
            newUVs.Add(new Vector2(0.5f, i/(float)(sides)));
        }
        // Extra uv to handle the seam
        newUVs.Add(new Vector2(0.5f, 1));

        for (int i = 0; i < sides; i++)
        {
            newUVs.Add(new Vector2(0.5f, i/(float)(sides)));
        }
        // Extra uv to handle the seam
        newUVs.Add(new Vector2(0.5f, 1));

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
