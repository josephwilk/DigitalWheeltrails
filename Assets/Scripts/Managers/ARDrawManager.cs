using System.Collections.Generic;
using System;
using DilmerGames.Core.Singletons;
using DilmerGames.UI;
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
    private UIPane drawStatus = null;

    [SerializeField]
    private UnityEvent OnDraw = null;

    [SerializeField]
    private ARAnchorManager anchorManager = null;

    [SerializeField]
    private Camera arCamera = null;

    [SerializeField]
    public bool meshMode { get; set; } = false;


    [SerializeField]
    GameObject m_Prefab;
    public GameObject prefab
    {
        get => m_Prefab;
        set => m_Prefab = value;
    }

    public static event Action Ontouched;

    private List<ARAnchor> anchors = new List<ARAnchor>();

    private Dictionary<int, ARCurvedLine> Lines = new Dictionary<int, ARCurvedLine>();

    private WheeltrailsManager wheeltrailsManager = null;

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

    public void setLineMaterial(LineSettings l)
    {
        lineSettings = l;
    }

    public bool drawing(){
        return drawActive;
    }

    public void flipDraw()
    {
        setDrawMode(!drawActive);
    }

    public void setDrawMode(bool mode)
    {
        drawActive = mode;

        if (drawActive == false)
        {
            initDraw = true;
        }
        if (drawActive)
        {
            drawStatus.Show();
        }
        else
        {
            drawStatus.Hide();
        }
    }



    public void AllowDraw(bool isAllow)
    {
        CanDraw = isAllow;
    }


    void DrawOnTouch()
    {
        if(!CanDraw) return;

        if (Input.touchCount > 0 &&
            Input.GetTouch(0).phase == TouchPhase.Began &&
            EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
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

                ARCurvedLine leftLine = new ARCurvedLine(lineSettings);
                GameObject leftAnchor = createAnchor(leftWorldPosition);

                Lines.Add(0, leftLine);
                leftLine.AddNewLineRenderer(transform, leftAnchor, leftWorldPosition);

                ARCurvedLine rightLine = new ARCurvedLine(lineSettings);
                GameObject rightAnchor = createAnchor(rightWorldPosition);

                Lines.Add(1, rightLine);
                rightLine.AddNewLineRenderer(transform, rightAnchor, rightWorldPosition);

                if (lineSettings.meshLine)
                {
                    GameObject wheeltrails = new GameObject($"Wheeltrail");
                    wheeltrails.transform.parent = transform;
                    wheeltrails.tag = lineSettings.lineTagName;
                    wheeltrailsManager = wheeltrails.AddComponent<WheeltrailsManager>();
                    wheeltrailsManager.settings = lineSettings;
                    wheeltrailsManager.parent = wheeltrails.transform;
                    wheeltrailsManager.leftContainer = leftAnchor;
                    wheeltrailsManager.rightContainer = rightAnchor;


                    //ARMeshLine l = new ARMeshLine(lineSettings);
                    //ARMeshLine r = new ARMeshLine(lineSettings);
                    //MeshLines.Add(0, l);
                    //MeshLines.Add(1, r);
                    //l.AddNewMeshRenderer(transform, leftAnchor, leftWorldPosition);
                    //r.AddNewMeshRenderer(transform, rightAnchor, rightWorldPosition);
                }

            }
            else
            {
                if (lineSettings.meshLine)
                {
                    wheeltrailsManager.AddPoint(leftWorldPosition);
                }
              
                Lines[0].AddPoint(leftWorldPosition);
                Lines[1].AddPoint(rightWorldPosition);
            }

        }
        else
        {
            if (Lines.ContainsKey(0))
            {
                Lines.Remove(0);
                Lines.Remove(1);

                if (lineSettings.meshLine)
                {
                    wheeltrailsManager = null;
                }
            }
        }
    }

    void DrawOnMouse()
    {
        if(!CanDraw) return;

        Vector3 leftPos = arCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x-distanceBetweenWheels,
                                                                  Input.mousePosition.y-400,
                                                                  lineSettings.distanceFromCamera));

        Vector3 rightPos = arCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x+distanceBetweenWheels,
                                                                        Input.mousePosition.y-400, lineSettings.distanceFromCamera));

        if (Input.GetMouseButton(0) && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        

        if (Input.GetMouseButton(0))
        {
            OnDraw?.Invoke();
           

            if (Lines.Keys.Count == 0)
            {
                flipDraw();
                ARCurvedLine line1 = new ARCurvedLine(lineSettings);
                ARCurvedLine line2 = new ARCurvedLine(lineSettings);
                Lines.Add(0, line1);
                Lines.Add(1, line2);
                line1.AddNewLineRenderer(transform, null, leftPos);
                line2.AddNewLineRenderer(transform, null, rightPos);


                if (lineSettings.meshLine)
                {
                    GameObject wheeltrails = new GameObject($"Wheeltrail");
                    wheeltrails.transform.parent = transform;
                    wheeltrails.tag = lineSettings.lineTagName;
                    wheeltrailsManager = wheeltrails.AddComponent<WheeltrailsManager>();
                    wheeltrailsManager.settings = lineSettings;
                    wheeltrailsManager.parent = wheeltrails.transform;
                }

            }
            else 
            {
                if (lineSettings.meshLine)
                {
                    wheeltrailsManager.AddPoint(leftPos);
                }
              
                Lines[0].AddPoint(leftPos);
                Lines[1].AddPoint(rightPos);
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            flipDraw();
            Lines.Remove(0);
            Lines.Remove(1);

            if (lineSettings.meshLine)
            {
                wheeltrailsManager = null;
            }
        }
    }

    GameObject[] GetAllLinesInScene()
    {
        return GameObject.FindGameObjectsWithTag("Line");
    }

    public void ClearLines()
    {
        setDrawMode(false);
        GameObject[] lines = GetAllLinesInScene();
        foreach (GameObject currentLine in lines)
        {
            Destroy(currentLine);
        }
    }

    private GameObject createAnchor(Vector3 hit)
    {

        //m_AnchorManager.anchorPrefab
        Pose p = new Pose(hit, Quaternion.identity);
        prefab = new GameObject();
        GameObject anchorContainer = Instantiate(prefab, p.position, p.rotation);
        ARAnchor anchor = null;
        anchor = anchorContainer.GetComponent<ARAnchor>();
        if (anchor == null)
        {
            anchor = anchorContainer.AddComponent<ARAnchor>();
        }
        //anchors.Add(anchor);
        return anchorContainer;
    }
}