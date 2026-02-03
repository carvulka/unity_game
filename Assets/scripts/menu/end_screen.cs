using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class end_screen : MonoBehaviour
{
    [Header("components/children")]
    [SerializeField] TMP_Text result_field;
        
    public void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;
        result_field.text = $"РЕЗУЛТАТ: {Math.Round(PLAYER_INFO.score)} Т. ЗА ВРЕМЕ: {PLAYER_INFO.minutes}:{PLAYER_INFO.seconds}";
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }


    public void play_again()
    {
        PLAYER_INFO.score = 0;
        PLAYER_INFO.minutes = 0;
        PLAYER_INFO.seconds = 0;
        LoadMainMenu();
    }

    public void quit()
    {
        Application.Quit();
    }
}
