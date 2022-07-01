using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DilmerGames.UI;

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

    private bool interacted = false;

    public void DownloadPreview()
    {
        screenshotManager.Download();
        disableSave();
        interacted = true;
    }

    public void Share()
    {
        screenshotManager.Share();
        interacted = true;
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
        interacted = false;
    }

    void Update()
    {

        if (interacted)
        {
#if !UNITY_EDITOR
        OnTouch();
#else
            OnMouse();
#endif
        }
    }

    void OnTouch()
    {
        if (Input.touchCount > 0 &&
    Input.GetTouch(0).phase == TouchPhase.Began &&
    EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        {
            gameObject.GetComponent<UIPane>().Hide();
        }

    }

    private void OnMouse()
    {
        if (Input.GetMouseButton(0) &&
            EventSystem.current.IsPointerOverGameObject())
        {
            gameObject.GetComponent<UIPane>().Hide();
        }
    }
}
