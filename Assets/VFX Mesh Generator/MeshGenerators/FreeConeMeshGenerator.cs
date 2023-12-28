using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeConeMeshGenerator: MeshGenerator
{
    [SerializeField] int _sides = 32;
    [SerializeField] float _angle = 360;
    public int Sides { get => _sides; set => _sides = value; }
    public float Angle { get => _angle; set => _angle = value; }
    [System.Serializable] public class HeightData
    {
        public float Height;
        public float Radius;
        public HeightData(float height, float radius)
        {
            Height = height;
            Radius = radius;
        }
    }
    public List<HeightData> HeightList = new List<HeightData>() { 
        new HeightData(0.5f, 0.1f),
        new HeightData(0.38f, 0.3f),
        new HeightData(0.1f, 0.5f),
    };
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
        List<Vector3> vertsList = new List<Vector3>();
        for(int i = 0; i < HeightList.Count; i++)
        {
            vertsList.AddRange(GetCircumferencesVertices(_sides, HeightList[i].Radius, _angle, HeightList[i].Height));
        }
        return vertsList.ToArray();
    }
    protected override int[] CalculateTriangles(Vector3[] verts)
    {
        int sides = verts.Length/HeightList.Count;
        List<int> newTriangles = new List<int>();
        for(int j = 0; j < HeightList.Count-1; j++)
        {
            for (int i = 0; i < sides-1; i++)
            {
                int outerIndex = i;
                int innerIndex = i + sides;

                int nextIndex = (i + 1) % sides;
                int nextOuterIndex = nextIndex;
                int nextInnerIndex = nextIndex + sides;
                
                if(!FlipNormals)
                {
                    newTriangles.AddRange(new int[] { outerIndex+j*sides, nextOuterIndex+j*sides, nextInnerIndex+j*sides, });
                    newTriangles.AddRange(new int[] { outerIndex+j*sides, nextInnerIndex+j*sides, innerIndex+j*sides, });
                }
                else
                {
                    newTriangles.AddRange(new int[] { nextInnerIndex+j*sides, nextOuterIndex+j*sides, outerIndex+j*sides });
                    newTriangles.AddRange(new int[] { innerIndex+j*sides, nextInnerIndex+j*sides, outerIndex+j*sides });
                }
            }
        }
        return newTriangles.ToArray();
    }
    protected override Vector2[] CalculateUVs(Vector3[] verts)
    {
        int sides = verts.Length/HeightList.Count-1;
        List<Vector2> newUVs = new List<Vector2>();

        for(int j = 0; j < HeightList.Count; j++)
        {
            float percentage = (float)(j)/(float)(HeightList.Count-1);
            for (int i = 0; i < sides; i++)
            {
                newUVs.Add(new Vector2(percentage, i/(float)(sides)));
            }
            // Extra uv to handle the seam
            newUVs.Add(new Vector2(percentage, 1));
        }

        return newUVs.ToArray();
    }

    [Header("Equation properties. Not used in auto update.")]
    [SerializeField] float _startHeight = 0;
    [SerializeField] float _endHeight = 1;
    public float StartHeight { get => _startHeight; set => _startHeight = value; }
    public float EndHeight { get => _endHeight; set => _endHeight = value; }
    public void EditVerticesByEquation(Func<float, float> Equation)
    {
        for(int i = 0; i < HeightList.Count; i++)
        {
            HeightList[i].Height = Mathf.Lerp(_startHeight, _endHeight, (float)i/(float)(HeightList.Count-1));
            HeightList[i].Radius = Equation(HeightList[i].Height);
        }
        Draw();
    }

#if UNITY_EDITOR
    int _currentHeightListCount = 0;
    protected override void OnEditorUpdate()
    {
        if(_currentHeightListCount != HeightList.Count)
        {
            _currentHeightListCount = HeightList.Count;
            Draw();
        }
        if(RestrictUnsafeValues)
        {
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
