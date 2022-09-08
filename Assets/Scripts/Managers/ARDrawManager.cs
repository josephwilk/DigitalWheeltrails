using System.Collections.Generic;
using System;
using DilmerGames.Core.Singletons;
using DilmerGames.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;


//using Autodesk.Fbx;
using UnityFBXExporter;
using UnityEditor;
using System.IO;


[RequireComponent(typeof(ARAnchorManager))]
public class ARDrawManager : Singleton<ARDrawManager>
{
    [SerializeField]
    private LineSettings lineSettings = null;

    [SerializeField]
    private UIPane drawStatus = null;

    [SerializeField]
    private UIPane tapToDrawPrompt = null;

    [SerializeField]
    private UnityEvent OnDraw = null;

    [SerializeField]
    private Camera arCamera = null;


    [SerializeField]
    private TMPro.TMP_InputField ribbonText = null;
    [SerializeField]
    private RenderTexture ribbonTexture = null;

    [SerializeField]
    private Material templateToFindForExport;

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
    private Dictionary<int, ARMeshLine> MeshLines = new Dictionary<int, ARMeshLine>();

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
        tapToDrawPrompt.ShowFast();
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
            tapToDrawPrompt.HideFast();
            drawStatus.Show();
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
        else
        {
            drawStatus.Hide();
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
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
                    ARMeshLine l = new ARMeshLine(lineSettings);
                    ARMeshLine r = new ARMeshLine(lineSettings);
                    MeshLines.Add(0, l);
                    MeshLines.Add(1, r);
                    l.AddNewMeshRenderer(transform, leftAnchor, leftWorldPosition);
                    r.AddNewMeshRenderer(transform, rightAnchor, rightWorldPosition);
                }

            }
            else
            {
                if (lineSettings.meshLine)
                {
                    MeshLines[0].AddPoint(leftWorldPosition);
                    MeshLines[1].AddPoint(rightWorldPosition);
                }

                if (!lineSettings.lineOff)
                {
                    Lines[0].AddPoint(leftWorldPosition);
                    Lines[1].AddPoint(rightWorldPosition);
                }
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
                    MeshLines.Remove(0);
                    MeshLines.Remove(1);
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
                    ARMeshLine l = new ARMeshLine(lineSettings);
                    ARMeshLine r = new ARMeshLine(lineSettings);
                    MeshLines.Add(0, l);
                    MeshLines.Add(1, r);
                    l.AddNewMeshRenderer(transform, null, leftPos);
                    r.AddNewMeshRenderer(transform, null, rightPos);
                }

            }
            else 
            {
                if (lineSettings.meshLine)
                {
                    MeshLines[0].AddPoint(leftPos);
                    MeshLines[1].AddPoint(rightPos);
                }


                if (!lineSettings.lineOff)
                {
                    Lines[0].AddPoint(leftPos);
                    Lines[1].AddPoint(rightPos);
                }
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            flipDraw();
            Lines.Remove(0);
            Lines.Remove(1);

            if (lineSettings.meshLine)
            {
                MeshLines.Remove(0);
                MeshLines.Remove(1);

            }
        }
    }


    public Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height-200, TextureFormat.RGB24, false);
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height-200), 0, 0);
        tex.Apply();
        tex.wrapModeU = TextureWrapMode.Repeat;
        tex.wrapModeV = TextureWrapMode.Clamp;
        tex.name = "ribbonStaticText";

        return tex;
    }

    public void ExportTrail()
    {
        Texture2D tx = toTexture2D(ribbonTexture);

        DateTime theTime = DateTime.Now;
        string ts = theTime.ToString("yyyy-MM-dd\\THH_mm_ss\\Z");
        int mesh_idx = 0;
        int line_idx = 0;
        GameObject[] lines = GetAllLinesInScene();

     
        string fileName = Application.persistentDataPath + "/" + "southbank.log";

        if (ribbonText)
        {
            File.AppendAllText(fileName, "{'ts': '"+ ts +"', 'text': '" +ribbonText.text+"', 'settings':'"+ lineSettings.name +"'}\n");
        }

        foreach (GameObject currentLine in lines){
            LineRenderer line = currentLine.GetComponent<LineRenderer>();
            MeshFilter filter = currentLine.GetComponent<MeshFilter>();

             if (line){

                string filename = ts + "_" + line_idx + "_Line.fbx";
                line_idx++;


                Mesh lineMesh = new Mesh();
                line.BakeMesh(lineMesh);
                lineMesh.Optimize();
                lineMesh.RecalculateNormals();
                lineMesh.RecalculateBounds();


                GameObject export = new GameObject($"LineExport");

                //export.transform.position = currentLine.transform.position;
                export.transform.rotation = currentLine.transform.rotation;
                //export.transform.localScale = currentLine.transform.localScale;
                //export.transform.localPosition = currentLine.transform.localPosition;
                //export.transform.localRotation = currentLine.transform.localRotation;

                MeshRenderer _meshRender = export.AddComponent<MeshRenderer>();
                MeshFilter _meshFilter = export.AddComponent<MeshFilter>();
                _meshFilter.mesh = lineMesh;

                Material m = new Material(Shader.Find(templateToFindForExport.shader.name));
                m.SetTexture("_MainTex", tx);
                _meshRender.material = m;
                //_meshRender.material.SetTexture("_MainTex", tx);

                try
                {
                    ExportGameObject(export, "/"+ts, filename, "/Textures/");
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }

                Destroy(lineMesh);
                Destroy(export);
                
            }
            if (filter)
            {
                string filename = ts + "_" + mesh_idx + "_Mesh.fbx";
                mesh_idx++;

                GameObject export = new GameObject($"MeshExport");
                //export.transform.parent = currentLine.transform.parent;
                export.transform.position = currentLine.transform.position;
                export.transform.rotation = currentLine.transform.rotation;
                //export.transform.localScale = currentLine.transform.localScale;
                //export.transform.localPosition = currentLine.transform.localPosition;
                //export.transform.localRotation = currentLine.transform.localRotation;

                MeshRenderer rend = currentLine.GetComponent<MeshRenderer>();

                MeshRenderer _meshRender = export.AddComponent<MeshRenderer>();
                MeshFilter _meshFilter = export.AddComponent<MeshFilter>();
                _meshFilter.mesh = filter.mesh;
                _meshRender.material = new Material(Shader.Find(templateToFindForExport.shader.name));

                try
                {
                    ExportGameObject(export, "/" + ts, filename, "/Textures/");
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
                Destroy(export);

            }

        }
        byte[] bytes = tx.EncodeToPNG();
        File.WriteAllBytes(Application.persistentDataPath + "/" + ts + "/" + ts + ".png", bytes);


    }



    public static bool ExportGameObject(GameObject rootGameObject, string folderPath, string fileName, string textureFolderName)
    {
        if (rootGameObject == null)
        {
            Debug.Log("Root game object is null, please assign it");
            return false;
        }

        // forces use of forward slash for directory names
        folderPath = folderPath.Replace('\\', '/');
        textureFolderName = textureFolderName.Replace('\\', '/');

        folderPath = Application.persistentDataPath + folderPath;

        if (System.IO.Directory.Exists(folderPath) == false)
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }

        if (System.IO.Path.GetExtension(fileName).ToLower() != ".fbx")
        {
            Debug.LogError(fileName + " does not end in .fbx, please save a file with the extension .fbx");
            return false;
        }

        if (folderPath[folderPath.Length - 1] != '/')
            folderPath += "/";

        if (System.IO.File.Exists(folderPath + fileName))
            System.IO.File.Delete(folderPath + fileName);

        bool exported = FBXExporter.ExportGameObjAtRuntime(rootGameObject, folderPath, fileName, textureFolderName, false);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
        return exported;
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
            LineRenderer line = currentLine.GetComponent<LineRenderer>();
            MeshFilter filter = currentLine.GetComponent<MeshFilter>();
            if (filter)
            {
                Destroy(filter.mesh);
            }
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