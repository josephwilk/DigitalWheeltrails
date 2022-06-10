using System.Collections.Generic;
using System;
using DilmerGames.Core.Singletons;
using EasyCurvedLine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARAnchorManager))]
public class ARDrawManager : Singleton<ARDrawManager>
{
    [SerializeField]
    private LineSettings lineSettings = null;

    [SerializeField]
    private UnityEvent OnDraw = null;

    [SerializeField]
    private ARAnchorManager anchorManager = null;

    [SerializeField]
    private Camera arCamera = null;

    public static event Action Ontouched;

    private List<ARAnchor> anchors = new List<ARAnchor>();

    private Dictionary<int, ARCurvedLine> Lines = new Dictionary<int, ARCurvedLine>();

    private bool CanDraw { get; set; }

    private bool drawActive { get; set; }
    private bool initDraw = true;

    private int distanceBetweenWheels = 600; //Screen distance.

    void Update()
    {
#if !UNITY_EDITOR
        DrawOnTouch();
#else
        DrawOnMouse();
#endif
    }

    public bool drawing(){
        return drawActive;
    }

    public void flipDraw()
    {
       
        drawActive = !drawActive;
        if (drawActive == false)
        {
            initDraw = true;
        }
    }

    public void AllowDraw(bool isAllow)
    {
        CanDraw = isAllow;
    }


    void DrawOnTouch()
    {
        if(!CanDraw) return;

        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        {
            return;
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if(touch.phase == TouchPhase.Began) { 
                flipDraw();
            }
        }

        if (drawActive)
        {
                Vector3 leftPos = new Vector3((Screen.width / 2) - distanceBetweenWheels,
                                      (Screen.height / 2)        - 400,
                                      lineSettings.distanceFromCamera);
                Vector3 rightPos = new Vector3((Screen.width / 2) + distanceBetweenWheels,
                                          (Screen.height / 2)     - 400,
                                          lineSettings.distanceFromCamera);

                Vector3 leftWorldPosition = arCamera.ScreenToWorldPoint(leftPos);
                Vector3 rightWorldPosition = arCamera.ScreenToWorldPoint(rightPos);


            if (initDraw) {
                OnDraw?.Invoke();
                initDraw = false;




               // Pose leftPose = new Pose(leftWorldPosition, Quaternion.identity);
                //var gameObject = Instantiate(prefab, leftPose.position, leftPose.rotation);
                // Make sure the new GameObject has an ARAnchor component
                //ARAnchor anchor = gameObject.GetComponent<ARAnchor>();
                //if (anchor == null)
                //{
                //    anchor = gameObject.AddComponent<ARAnchor>();
                //}


                //ARAnchor leftAnchor = anchorManager.AddAnchor(new Pose(leftWorldPosition, Quaternion.identity));


                //if (leftAnchor == null)
                //    Debug.LogError("Error creating reference point");
                //else
                //{
                //    anchors.Add(leftAnchor);
                //    ARDebugManager.Instance.LogInfo($"Anchor created & total of {anchors.Count} anchor(s)");
                //}
                ARCurvedLine leftLine = new ARCurvedLine(lineSettings);
                Lines.Add(0, leftLine);
                leftLine.AddNewLineRenderer(transform, leftWorldPosition);

                OnDraw?.Invoke();

                //ARAnchor rightAnchor = anchorManager.AddAnchor(new Pose(rightWorldPosition, Quaternion.identity));
                //if (rightAnchor == null)
                //    Debug.LogError("Error creating reference point");
                //else
                //{
                //    anchors.Add(rightAnchor);
                //    ARDebugManager.Instance.LogInfo($"Anchor created & total of {anchors.Count} anchor(s)");
                //}
                ARCurvedLine rightLine = new ARCurvedLine(lineSettings);
                Lines.Add(1, rightLine);
                rightLine.AddNewLineRenderer(transform, rightWorldPosition);
                ARDebugManager.Instance.LogInfo("first setup");
            }
            else
            {
                Lines[0].AddPoint(leftWorldPosition);
                Lines[1].AddPoint(rightWorldPosition);
            }

        }
        else
        {
            if (Lines.ContainsKey(0))
            {
                ARDebugManager.Instance.LogInfo("cleanup");
                Lines.Remove(0);
                Lines.Remove(1);
            }
        }
    }

    void DrawOnMouse()
    {
        if(!CanDraw) return;

        Vector3 mousePosition = arCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x-distanceBetweenWheels, Input.mousePosition.y-400, lineSettings.distanceFromCamera));
        Vector3 mousePositionOffset = arCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x+distanceBetweenWheels, Input.mousePosition.y-400, lineSettings.distanceFromCamera));

        if (Input.GetMouseButton(0))
        {
            OnDraw?.Invoke();

            if(Lines.Keys.Count == 0)
            {
                ARCurvedLine line1 = new ARCurvedLine(lineSettings);
                ARCurvedLine line2 = new ARCurvedLine(lineSettings);
                Lines.Add(0, line1);
                Lines.Add(1, line2);
                line1.AddNewLineRenderer(transform, mousePosition);
                line2.AddNewLineRenderer(transform, mousePositionOffset);
            }
            else 
            {
                Lines[0].AddPoint(mousePosition);
                Lines[1].AddPoint(mousePositionOffset);
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            Lines.Remove(0);
            Lines.Remove(1);
        }
    }

    GameObject[] GetAllLinesInScene()
    {
        return GameObject.FindGameObjectsWithTag("Line");
    }

    public void ClearLines()
    {
        drawActive = false;
        initDraw = true;
        GameObject[] lines = GetAllLinesInScene();
        foreach (GameObject currentLine in lines)
        {
            CurvedLineRenderer line = currentLine.GetComponent<CurvedLineRenderer>();
            Destroy(currentLine);
        }
    }
}