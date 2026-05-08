using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowUsername : MonoBehaviour
{
    public TextMeshProUGUI UserNameText;
    public string Name;
    // Start is called before the first frame update
    void LateUpdate()
    {
        ShowUserName();
    }

    public void ShowUserName()
    {
        Name = PlayerPrefs.GetString("UserName", "");
        UserNameText.text = Name;
    }
}
