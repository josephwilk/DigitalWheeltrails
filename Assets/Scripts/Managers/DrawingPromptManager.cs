using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using DilmerGames.UI;

public class DrawingPromptManager : MonoBehaviour
{

    [SerializeField]
    private ARDrawManager drawManager = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
#if !UNITY_EDITOR
        OnTouch();
#else
        OnMouse();
#endif
    }

    private void OnTouch()
    {
        if (Input.touchCount > 0 &&
            Input.GetTouch(0).phase == TouchPhase.Began &&
            EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        {
            gameObject.GetComponent<UIPane>().Hide();
            drawManager.setDrawMode(true);
        }
    }

    private void OnMouse()
    {
        if (Input.GetMouseButton(0) &&
            EventSystem.current.IsPointerOverGameObject())
        {
            gameObject.GetComponent<UIPane>().Hide();
            drawManager.setDrawMode(true);
        }
    }

}
