using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class ScreenCapture : MonoBehaviour
{

    [SerializeField]
    private Camera screengrab= null;

    [SerializeField]
    private Image previewImage = null;

    [SerializeField]
    private GameObject imageGui = null;


    //Set your screenshot resolutions
    public int captureWidth = 1920;
    public int captureHeight = 1080;
    // configure with raw, jpg, png, or ppm (simple raw format)
    public enum Format { RAW, JPG, PNG, PPM };
    public Format format = Format.JPG;
    // folder to write output (defaults to data path)
    private string outputFolder;
    // private variables needed for screenshot
    private Rect rect;
    private RenderTexture renderTexture;
    private Texture2D screenShot;

    bool isProcessing = false;

    public ScreenCapture()
    {
    }

    public void Share()
    {
        NativeShare sharing = new NativeShare();
        String file = SaveScreenshotToFile();
        sharing.AddFile(file);
        sharing.Share();
    }

    public void Download()
    {
        NativeGallery.Permission permission = NativeGallery.SaveImageToGallery(screenShot, "Wheeltrails", "screenshot.png",
          (success, path) => Debug.Log("Media save result: " + success + " " + path));

        ARDebugManager.Instance.LogInfo("SAVED");
        Debug.Log("Permission result: " + permission);
    }

    public void CleanUp()
    {
        imageGui.SetActive(false);
        previewImage.sprite = null;
        Destroy(screenShot);
    }


    //Initialize Directory
    private void Start()
    {
        outputFolder = Application.persistentDataPath + "/Screenshots/";
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
            Debug.Log("Save Path will be : " + outputFolder);
        }
    }

    private string CreateFileName(int width, int height)
    {
        //timestamp to append to the screenshot filename
        string timestamp = DateTime.Now.ToString("yyyyMMddTHHmmss");
        // use width, height, and timestamp for unique file 
        var filename = string.Format("{0}/screen_{1}x{2}_{3}.{4}", outputFolder, width, height, timestamp, format.ToString().ToLower());
        // return filename
        return filename;
    }


    private String SaveScreenshotToFile() {

        // get our filename
        string filename = CreateFileName((int)rect.width, (int)rect.height);
        // get file header/data bytes for the specified image format
        byte[] fileHeader = null;
        byte[] fileData = null;
        //Set the format and encode based on it
                if (format == Format.RAW)
                    {
            fileData = screenShot.GetRawTextureData();
                    }
                else if (format == Format.PNG)
                    {
            fileData = screenShot.EncodeToPNG();
                    }
                else if (format == Format.JPG)
                    {
            fileData = screenShot.EncodeToJPG();
                    }
                else //For ppm files
                    {
                        // create a file header - ppm files
            string headerStr = string.Format("P6\n{0} {1}\n255\n", rect.width, rect.height);
            fileHeader = System.Text.Encoding.ASCII.GetBytes(headerStr);
            fileData = screenShot.GetRawTextureData();
                    }
        
        //        // create new thread to offload the saving from the main thread
        //new System.Threading.Thread(() =>
        //        {
                    var file = System.IO.File.Create(filename);
                    if (fileHeader != null)
                        {
                            file.Write(fileHeader, 0, fileHeader.Length);
                        }
        file.Write(fileData, 0, fileData.Length);
        file.Close();
        Debug.Log(string.Format("Screenshot Saved {0}, size {1}", filename, fileData.Length));
        //isProcessing = false;
        //        }).Start();
        return filename;
}


    private IEnumerator TakeScreenshotAndSave()
    {

        screengrab.enabled = true;

        if (renderTexture == null)
        {
            rect = new Rect(0, 0, Screen.width, Screen.height);
            renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        }
   
        Camera camera = screengrab;
        camera.targetTexture = renderTexture;
        camera.Render();
    
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(rect, 0, 0, false);
        screenShot.Apply();
     
        camera.targetTexture = null;
        RenderTexture.active = null;
        yield return null;

        ShowScreenshot();
     
        // To avoid memory leaks
        Destroy(renderTexture);

        screengrab.enabled = false;
    }

    public void TakeScreenShot()
    {
        StartCoroutine(TakeScreenshotAndSave());
    }

    public void ShowScreenshot()
    {
        imageGui.SetActive(true);
        Sprite captureSprite = Sprite.Create(screenShot, new Rect(0.0f, 0.0f, screenShot.width, screenShot.height),
    new Vector2(0.5f, 0.0f), 100.0f);
        previewImage.sprite = captureSprite;

    }
}
