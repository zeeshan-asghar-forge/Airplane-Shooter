using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BeforeHomeScreen : MonoBehaviour
{
    public string LevelName = "BeforeHomeScreen";
    public int CurrentScreenWidth;
    public int CurrentScreenHeight;

    // Start is called before the first frame update
    void Start()
    {
        GetResolution();
    }


    public void GetResolution()
    {
        CurrentScreenHeight = Screen.height;
        CurrentScreenWidth = Screen.width;
        SaveResolution();
    }
    public void SaveResolution()
    {
        PlayerPrefs.SetInt("Hight", CurrentScreenHeight);
        PlayerPrefs.SetInt("Width", CurrentScreenWidth);
        GoToHomeScreen();
    }

    public void GoToHomeScreen()
    {
        StartCoroutine(LoadLevel(LevelName));
    }
    IEnumerator LoadLevel(string LevelName)
    {
        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(LevelName);
    }
}
