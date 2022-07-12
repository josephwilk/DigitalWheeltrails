using UnityEngine;

[CreateAssetMenu(fileName = "LineSettings", menuName = "Create Line Settings", order = 0)]
public class LineSettings : ScriptableObject 
{
    public string lineTagName = "Line";

    public bool meshLine = false;
    public bool lineOff = false;

    public Color startColor = Color.white;

    public Color endColor = Color.white;

    public float startWidth = 0.03f;

    public float endWidth = 0.03f;

    public float distanceFromCamera = 0.6f;

    public Material defaultMaterial;
    public Material extraMaterial = null;

    public int cornerVertices = 140;

    public int endCapVertices = 5;

    [Range(0, 1.0f)]
    public float minDistanceBeforeNewPoint = 0.002f;

    public LineTextureMode textureMode = LineTextureMode.Tile;

    [Header("Lights")]

    public bool lightData = false;
    public UnityEngine.Rendering.LightProbeUsage lightProb = UnityEngine.Rendering.LightProbeUsage.Off;
    public UnityEngine.Rendering.ShadowCastingMode shadow = UnityEngine.Rendering.ShadowCastingMode.Off;

    [Header("Smoothing Options")]
    public bool allowSmoothing = true;

    public float dampen = 0.035f;


    [Header("Tolerance Options")]
    public bool allowSimplification = false;

    public float tolerance = 0.002f;
    
    public float applySimplifyAfterPoints = 400.0f;

}