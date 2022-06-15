using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using System;
using DilmerGames.Core.Singletons;
using EasyCurvedLine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using EasyCurvedLine;


public class ARCurvedLine
{

    public float lineSegmentSize = 0.05f;

    private int positionCount = 0;

    private Vector3 prevPointDistance = Vector3.zero;

    private LineRenderer LineRenderer { get; set; }

    private LineSettings settings;

    public ARCurvedLine(LineSettings settings)
    {
        this.settings = settings;
    }


    private Vector3 smoothAxis(int axis, int windowSize, Vector3 newPos)
    {
       
        int bucket = 0;
        
        float zTotal = 0.0f;
        for (int i = positionCount-2; i >= 0 && i >= (positionCount-2)-windowSize; i--) {
            Vector3 p1 = LineRenderer.GetPosition(i);
            zTotal += p1[axis];
            bucket = bucket+1;
        }
       
        float newAxis = (zTotal / bucket);
        //ARDebugManager.Instance.LogInfo($"oy:{newPos.y}, n{newAxis}");
        newPos[axis] = newAxis;
        return newPos;
    }

    public void AddPoint(Vector3 position)
    {
        bool firstPoint = (prevPointDistance == Vector3.zero);
        if (firstPoint)
        {
            prevPointDistance = position;
        }

        if (prevPointDistance != null &&
            Mathf.Abs(Vector3.Distance(prevPointDistance, position)) >= settings.minDistanceBeforeNewPoint)
        {

            positionCount++;
            LineRenderer.positionCount = positionCount;

            float smoothTime = settings.dampen;
            float xVelocity = 0.0f;
            float yVelocity = 0.0f;
            float zVelocity = 0.0f;

            Vector3 smoothedPos = prevPointDistance;

            if (firstPoint)
            {
                smoothedPos = position;
            }
            else
            {
               smoothedPos.x = Mathf.SmoothDamp(prevPointDistance.x, position.x, ref xVelocity, smoothTime);
               smoothedPos.y = Mathf.SmoothDamp(prevPointDistance.y, position.y, ref yVelocity, smoothTime); //-0.6f
               smoothedPos.z = Mathf.SmoothDamp(prevPointDistance.z, position.z, ref zVelocity, smoothTime);
            }

            LineRenderer.SetPosition(positionCount - 1, smoothedPos);   
            prevPointDistance = smoothedPos;


            if (settings.allowSmoothing)
            {
                Vector3[] linePositions = new Vector3[LineRenderer.positionCount];
                LineRenderer.GetPositions(linePositions);
                ARDebugManager.Instance.LogInfo($"pre-smoothing: [{linePositions.Length}]");
                Vector3[] smoothLines = LineSmoother.SmoothLine(linePositions, settings.minDistanceBeforeNewPoint / 2.0f);
                LineRenderer.positionCount = smoothLines.Length;
                LineRenderer.SetPositions(smoothLines);
                ARDebugManager.Instance.LogInfo($"post-smoothing: [{smoothLines.Length}]");

            }


            // applies simplification if reminder is 0
            if (LineRenderer.positionCount % settings.applySimplifyAfterPoints == 0 &&
                settings.allowSimplification)
            {
                int oldCount = LineRenderer.positionCount;
                ARDebugManager.Instance.LogInfo($"x:{smoothedPos.x},y:{smoothedPos.y},z:{smoothedPos.z}");
                LineRenderer.Simplify(settings.tolerance);
                positionCount = LineRenderer.positionCount;
                ARDebugManager.Instance.LogInfo($"simplfy: [{oldCount-positionCount}]");

                positionCount = LineRenderer.positionCount;
            }
        }
    }


    public void AddCurvedPoint(Vector3 position)
    {

        if (LineRenderer.positionCount > 4)
        {
            //Vector3[] linePositions = new Vector3[LineRenderer.positionCount];
            //Vector3[] smoothedLinePositions = new Vector3[4];
            //Vector3 p1 = linePositions[LineRenderer.positionCount - 1];
            //Vector3 p2 = linePositions[LineRenderer.positionCount - 2];
            //Vector3 p3 = linePositions[LineRenderer.positionCount - 3];
            //Vector3 p4 = linePositions[LineRenderer.positionCount - 3];
            //Vector3[] smoothLines = LineSmoother.SmoothLine(smoothedLinePositions, settings.minDistanceBeforeNewPoint/2.0f);

            //for(int )

            //LineRenderer.

        }
        else
        {
            positionCount++;
            LineRenderer.positionCount = positionCount;
            Vector3 smoothedPos = smoothAxis(1, 7, position);
            LineRenderer.SetPosition(positionCount - 1, smoothedPos);
            prevPointDistance = smoothedPos;
        }
    }

    public void AddNewLineRenderer(Transform parent, GameObject anchorContainer, Vector3 position)
    {
        positionCount = 2;
        if (!anchorContainer)
        {
            anchorContainer = new GameObject($"LineRenderer");
            anchorContainer.transform.parent = parent;
        }

        anchorContainer.tag = settings.lineTagName;

        LineRenderer goLineRenderer = anchorContainer.AddComponent<LineRenderer>();

        goLineRenderer.startWidth = settings.startWidth;
        goLineRenderer.endWidth = settings.endWidth;

        goLineRenderer.startColor = settings.startColor;
        goLineRenderer.endColor = settings.endColor;


        goLineRenderer.material = settings.defaultMaterial;
        goLineRenderer.useWorldSpace = true;
        goLineRenderer.positionCount = positionCount;

        //goLineRenderer.alignment = LineAlignment.TransformZ;
        goLineRenderer.generateLightingData = false;
        goLineRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        goLineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        goLineRenderer.textureMode = LineTextureMode.Tile;

        goLineRenderer.numCornerVertices = settings.cornerVertices;
        goLineRenderer.numCapVertices = settings.endCapVertices;

        goLineRenderer.SetPosition(0, position);
        goLineRenderer.SetPosition(1, position);

        LineRenderer = goLineRenderer;

        ARDebugManager.Instance.LogInfo($"New curved line renderer created");
    }
}