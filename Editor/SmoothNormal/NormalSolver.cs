
using System;
using System.Collections.Generic;
using UnityEngine;

public static class NormalSolver
{
    public static void RecalculateNormals2(this Mesh m)
    {

        int triangleCount = m.triangles.Length / 3;
        //  print("triangle count: " + triangleCount);

        for (int i = 4; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;

            int pointIndexA = m.triangles[normalTriangleIndex];
            int pointIndexB = m.triangles[normalTriangleIndex + 1];
            int pointIndexC = m.triangles[normalTriangleIndex + 2];

            // print("pointindexA:  " + pointIndexA);

            Vector3 trianlgeNormal = surfaceNormalFromIndices(m, pointIndexA, pointIndexB, pointIndexC);

            //vertexNormal.Insert(pointIndexA, trianlgeNormal);
            //vertexNormal.Insert(pointIndexB, trianlgeNormal);
            //vertexNormal.Insert(pointIndexC, trianlgeNormal);


            m.normals[pointIndexA] = trianlgeNormal;
            m.normals[pointIndexB] = trianlgeNormal;
            m.normals[pointIndexC] = trianlgeNormal;
        }

        for (int i = 0; i < m.normals.Length; i++)
        {
            m.normals[i].Normalize();
            //print("new normals: " + vertexNormal[i]);
        }
    }

    static Vector3 surfaceNormalFromIndices(Mesh m, int indexA, int indexB, int indexC)
    {
        // $$anonymous$$eshInfo m = new $$anonymous$$eshInfo();
        Vector3 pointA = m.vertices[indexA];
        Vector3 pointB = m.vertices[indexB];
        Vector3 pointC = m.vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }
}