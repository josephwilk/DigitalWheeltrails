using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DilmerGames.UI;

public class RibbonModeManager : MonoBehaviour
{

    [SerializeField]
    private LineSettings[] ribbonModes = null;

    private UIPane gui;

    [SerializeField]
    private ARDrawManager drawManager = null;

    // Start is called before the first frame update
    void Start()
    {
        gui = gameObject.GetComponent<UIPane>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setMode(LineSettings mode) {
        drawManager.setLineMaterial(mode);
        gui.Hide();
    }
}
