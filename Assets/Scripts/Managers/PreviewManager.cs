using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreviewManager : MonoBehaviour
{
    [SerializeField]
    private Button saveButton;
    [SerializeField]
    private Image saveIcon;

    [SerializeField]
    private Button shareButton;

    [SerializeField]
    private Sprite activeIcon;

    [SerializeField]
    private Sprite inactiveIcon;

    [SerializeField]
    private ScreenCapture screenshotManager;

    public void DownloadPreview()
    {
        screenshotManager.Download();
        disableSave();
    }

    public void Share()
    {
        screenshotManager.Share();
    }


    void Start()
    {
        
    }

    public void disableSave()
    {
        saveButton.interactable = false;
        saveIcon.sprite = inactiveIcon;
    }

    private void OnEnable()
    {
        saveButton.interactable = true;
        saveIcon.sprite = activeIcon;
    }

    void Update()
    {
        
    }
}
