using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class RibbonTextManager : MonoBehaviour
{

    public TMPro.TextMeshPro container;
    public TMPro.TextMeshProUGUI charLimitInput;
    public TMPro.TMP_InputField textInputArea;

    private string defaultText = "";
    private string charLimit;
    const string WHEELTRAILS_TXT = "WheeltrailsText";
    
    // Start is called before the first frame update
    void Start()
    {
    }

    public void Init()
    {
        charLimit = $"{textInputArea.characterLimit}";
        defaultText = PlayerPrefs.GetString(WHEELTRAILS_TXT, "Wheeltrails");
        textInputArea.text = defaultText;
        fillTextSpace();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnEnable()
    {
        charLimitInput.text = $"{textInputArea.text.Length}/{charLimit}";
        textInputArea.ActivateInputField();
        textInputArea.MoveTextEnd(true);
    }

    public void fillTextSpace()
    {  
        int limit = textInputArea.characterLimit;
        string currentText = Regex.Replace(textInputArea.text, @"\t|\n|\r", "").Trim();
        //textInputArea.text = currentText;
        float s = limit / textInputArea.text.Length;
        int dup = Mathf.FloorToInt(s);

        if (dup > 1)
        {
            string paddedText = "";
            for (int i = 0; i < dup; i++) {
                paddedText = $"{paddedText} {currentText}".Trim();
            }
            container.text = paddedText.Trim();
            
        }
        else
        {
            container.text = textInputArea.text.Trim();
        }
        PlayerPrefs.SetString(WHEELTRAILS_TXT, currentText);
    }

    public void onTextChange(string txt)
    {
        charLimitInput.text = $"{txt.Length}/{charLimit}";
    }

}
