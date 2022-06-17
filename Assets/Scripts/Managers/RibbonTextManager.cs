using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RibbonTextManager : MonoBehaviour
{

    public TMPro.TextMeshPro container;
    public TMPro.TextMeshProUGUI charLimitInput;
    public TMPro.TMP_InputField textInputArea;

    
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        charLimitInput.text = $"{textInputArea.text.Length}/{textInputArea.characterLimit}";
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void onTextChange(string txt)
    {
        container.text = txt;
        charLimitInput.text = $"{txt.Length}/{textInputArea.characterLimit}";
    }


}
