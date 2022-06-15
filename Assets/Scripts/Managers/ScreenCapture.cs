using System;
using System.IO;
using System.Collections;
using UnityEngine;

public class ScreenCapture : MonoBehaviour
{
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

    private void CaptureScreenshot()
    {
        isProcessing = true;
        // create screenshot objects
        if (renderTexture == null)
        {
            // creates off-screen render texture to be rendered into
            rect = new Rect(0, 0, captureWidth, captureHeight);
            renderTexture = new RenderTexture(captureWidth, captureHeight, 24);
            screenShot = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
        }
        // get main camera and render its output into the off-screen render texture created above
        Camera camera = Camera.main;
        camera.targetTexture = renderTexture;
        camera.Render();
        // mark the render texture as active and read the current pixel data into the Texture2D
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(rect, 0, 0);
        // reset the textures and remove the render texture from the Camera since were done reading the screen data
        camera.targetTexture = null;
        RenderTexture.active = null;
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

        // create new thread to offload the saving from the main thread
        new System.Threading.Thread(() =>
        {
            var file = System.IO.File.Create(filename);
            if (fileHeader != null)
            {
                file.Write(fileHeader, 0, fileHeader.Length);
            }
            file.Write(fileData, 0, fileData.Length);
            file.Close();
            Debug.Log(string.Format("Screenshot Saved {0}, size {1}", filename, fileData.Length));
            isProcessing = false;
        }).Start();
        //Cleanup
        //Destroy(renderTexture);
        renderTexture.Release();
        renderTexture = null;
        screenShot = null;
    }


    private IEnumerator TakeScreenshotAndSave()
    {
        yield return new WaitForEndOfFrame();

        Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        ss.Apply();

        // Save the screenshot to Gallery/Photos
        NativeGallery.Permission permission = NativeGallery.SaveImageToGallery(ss, "Wheeltrails", "screenshot.png", (success, path) => Debug.Log("Media save result: " + success + " " + path));

        Debug.Log("Permission result: " + permission);

        // To avoid memory leaks
        Destroy(ss);
    }

    public void TakeScreenShot()
    {
        StartCoroutine(TakeScreenshotAndSave());
    }
}
