using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button playButton, settingButton, quitButton;

    public GameObject SettingMenu;
    // Start is called before the first frame update
    void Start()
    {
        SettingMenu.SetActive(false);
        settingButton.onClick.AddListener(ActiveSettingMenu);
        playButton.onClick.AddListener(Play);
    }

    void ActiveSettingMenu()
    {
        gameObject.SetActive(false);
        SettingMenu.SetActive(true);
    }

    void Play()
    {
        SceneManager.LoadScene("Game");
    }
}
