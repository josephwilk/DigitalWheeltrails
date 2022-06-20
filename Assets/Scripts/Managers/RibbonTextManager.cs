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

    public void fillTextSpace()
    {
        
        int limit = textInputArea.characterLimit;
        string currentText = textInputArea.text;
        float s = limit / textInputArea.text.Length;
        int dup = Mathf.FloorToInt(s);
        ARDebugManager.Instance.LogInfo($"{dup}");
        if (dup > 1)
        {
            string paddedText = "";
            for (int i = 0; i < dup; i++) {
                paddedText = $"{paddedText.Trim()} {currentText.Trim()}".Trim();
            }
            ARDebugManager.Instance.LogInfo($"{paddedText}");
            container.text = paddedText;
            
        }
        else
        {
            container.text = textInputArea.text;
        }
    }

    public void onTextChange(string txt)
    {
        charLimitInput.text = $"{txt.Length}/{textInputArea.characterLimit}";
    }


}
