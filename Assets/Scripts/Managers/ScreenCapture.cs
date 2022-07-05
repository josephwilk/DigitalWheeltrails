using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DilmerGames.UI;


public class ScreenCapture : MonoBehaviour
{

    [SerializeField]
    private Camera screengrab= null;

    [SerializeField]
    private Image previewImage = null;

    [SerializeField]
    private GameObject imageGui = null;

    private Rect rect;
    private RenderTexture renderTexture;
    private Texture2D screenShot;

    public ScreenCapture()
    {
    }

    public void Share()
    {
        NativeShare sharing = new NativeShare();
        sharing.AddFile(screenShot, "wheeltrails.jpg");
        sharing.Share();
    }

    public void Download()
    {
        NativeGallery.Permission permission = NativeGallery.SaveImageToGallery(screenShot, "Wheeltrails", "wheeltrails.jpg",
          (success, path) => Debug.Log("Media save result: " + success + " " + path));

        Debug.Log("Permission result: " + permission);
    }

    public void CleanUp()
    {
        previewImage.sprite = null;
        Destroy(screenShot);
    }

    private void Start()
    {
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
        Sprite captureSprite = Sprite.Create(screenShot, new Rect(0.0f, 0.0f, screenShot.width, screenShot.height),
    new Vector2(0.5f, 0.0f), 100.0f);
        previewImage.sprite = captureSprite;
        imageGui.SetActive(true);
        imageGui.GetComponent<UIPane>().Show();
    }
}
