/*
 * BSD 2-Clause License
  * Copyright (c) 2018, Andrey Sidorov
 * All rights reserved.
 * https://github.com/AndreySkyFoxSidorov/UI_Deformation_of_images_shapes_and_contours
 * https://forum.unity.com/threads/ui-deformation-of-images-shapes-and-contours.544542/
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasRenderer))]
[ExecuteInEditMode]
public class UIImageCurve : MaskableGraphic
{
    public Texture texture;

    public override Texture mainTexture
    {
        get { return texture; }
    }

    public int xSize, ySize;
    public bool isEnableTime = false;
    public float TimeScale = 10.0f;
    public bool isEnablePerlinNoise = false;
    public float ForcePerlinNoise = 1.0f;

    public AnimationCurve CurveY = new AnimationCurve();
    public AnimationCurve CurveX = new AnimationCurve();
    public float ForceCurve = 1.0f;


    protected Vector2[] vertices;
    protected Vector3[] verticesRun;
    protected bool isInitVertices = false;
    float AddedTime = 0.0f;


    protected override void OnPopulateMesh(VertexHelper vh)
    {
        float minX = (0f - rectTransform.pivot.x) * rectTransform.rect.width;
        float minY = (0f - rectTransform.pivot.y) * rectTransform.rect.height;
        float maxX = (1f - rectTransform.pivot.x) * rectTransform.rect.width;
        float maxY = (1f - rectTransform.pivot.y) * rectTransform.rect.height;

        var color32 = (Color32)color;

        if (xSize < 1) xSize = 1;
        if (ySize < 1) ySize = 1;

        vh.Clear();

        if (isInitVertices && vertices.Length != (xSize + 1) * (ySize + 1))
        {
            isInitVertices = false;
        }

        if (!isInitVertices)
        {
            verticesRun = new Vector3[(xSize + 1) * (ySize + 1)];
            vertices = new Vector2[(xSize + 1) * (ySize + 1)];
        }

        Vector2[] uv = new Vector2[vertices.Length];
        Vector4[] tangents = new Vector4[vertices.Length];
        Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
        for (int i = 0, y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {


                float xTime = CurveX.Evaluate(AddedTime + (1.0f / (float)xSize) * (float)x) - 1.0f;
                float yTime = CurveY.Evaluate(AddedTime + (1.0f / (float)ySize) * (float)y) - 1.0f;

                float yTimeNoise = 0.0f;
                float xTimeNoise = 0.0f;
                if (isEnablePerlinNoise)
                {
                    yTimeNoise = Mathf.PerlinNoise(Time.time * (float)x, 0) * ForcePerlinNoise;
                    xTimeNoise = Mathf.PerlinNoise(0, Time.time * (float)y) * ForcePerlinNoise;
                }

                xTime = xTime * ForceCurve;
                yTime = yTime * ForceCurve;

                if (!isInitVertices)
                {

                    vertices[i] = new Vector2((float)x, (float)y);
                }
                uv[i] = new Vector2((float)x / xSize, (float)y / ySize);
                tangents[i] = tangent;


                verticesRun[i] = new Vector2((minX + ((vertices[i].x + yTime + yTimeNoise) * ((maxX - minX) / xSize))), (minY + ((vertices[i].y + xTime + xTimeNoise) * ((maxY - minY) / ySize))));
                vh.AddVert(new Vector3((minX + ((vertices[i].x + yTime + yTimeNoise) * ((maxX - minX) / xSize))), (minY + ((vertices[i].y + xTime + xTimeNoise) * ((maxY - minY) / ySize)))), color32, new Vector2(uv[i].x, uv[i].y));
            }
        }

        int[] triangles = new int[xSize * ySize * 6];
        for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
        {
            for (int x = 0; x < xSize; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                triangles[ti + 5] = vi + xSize + 2;
                vh.AddTriangle(triangles[ti], triangles[ti + 1], triangles[ti + 2]);
                vh.AddTriangle(triangles[ti + 3], triangles[ti + 4], triangles[ti + 5]);
            }
        }
        isInitVertices = true;


    }


    private void OnDrawGizmosSelected()
    {
        float minX = (0f - rectTransform.pivot.x) * rectTransform.rect.width;
        float minY = (0f - rectTransform.pivot.y) * rectTransform.rect.height;
        float maxX = (1f - rectTransform.pivot.x) * rectTransform.rect.width;
        float maxY = (1f - rectTransform.pivot.y) * rectTransform.rect.height;
        float disX = ((maxX - minX) / xSize) * 0.1f;
        float disY = ((maxY - minY) / ySize) * 0.1f;
        float dis = disX > disY ? disY : disX;


        int[] triangles = new int[xSize * ySize * 6];
        for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
        {
            for (int x = 0; x < xSize; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                triangles[ti + 5] = vi + xSize + 2;
                Gizmos.color = new Color(0.0f, 0.0f, 0.0f, 0.6f);
                Vector3 p1 = verticesRun[triangles[ti]] + transform.position;
                Vector3 p2 = verticesRun[triangles[ti + 1]] + transform.position;
                Vector3 p3 = verticesRun[triangles[ti + 2]] + transform.position;
                Gizmos.DrawLine(p1, p2);
                Gizmos.DrawLine(p1, p3);
                Gizmos.DrawLine(p3, p2);
                Gizmos.DrawSphere(p1, dis);
                Gizmos.DrawSphere(p2, dis);
                Gizmos.DrawSphere(p3, dis);
                p1 = verticesRun[triangles[ti + 3]] + transform.position;
                p2 = verticesRun[triangles[ti + 4]] + transform.position;
                p3 = verticesRun[triangles[ti + 5]] + transform.position;
                Gizmos.DrawLine(p1, p2);
                Gizmos.DrawLine(p1, p3);
                Gizmos.DrawLine(p3, p2);
                Gizmos.DrawSphere(p1, dis);
                Gizmos.DrawSphere(p2, dis);
                Gizmos.DrawSphere(p3, dis);
            }
        }
    }

    private void Update()
    {
        if (isEnableTime)
        {
            AddedTime += Time.deltaTime * TimeScale;
            CurveX.postWrapMode = WrapMode.Loop;
            CurveX.preWrapMode = WrapMode.Loop;
            CurveY.postWrapMode = WrapMode.Loop;
            CurveY.preWrapMode = WrapMode.Loop;

        }
        else
        {
            AddedTime = 0.0f;
        }

        if (isEnableTime || isEnablePerlinNoise)
        {
            SetAllDirty();
        }
    }

}
