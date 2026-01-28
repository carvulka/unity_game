using NUnit.Framework;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class main_menu : MonoBehaviour
{
    [SerializeField]
    TMP_InputField textbox;
    [SerializeField]
    TMP_Text error;

    public const string leaderboardFileName = "leaderboard.txt";

    public void LoadLevel1()
    {
        if (!SaveText(textbox))
        {
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private bool SaveText(TMP_InputField textbox)
    {
        string name = textbox.text.Trim();
        if (string.IsNullOrEmpty(name)) 
        {
            error.text = "Името не може да е празно!";
            return false;
        }
        if (name.Contains(','))
        {
            error.text = "Името не може да съдържа символа ,";
            return false;
        }
        // leaderboard contains name and score as csv
        StreamReader sr = new StreamReader(Path.Combine(Application.persistentDataPath, leaderboardFileName));
        using(sr)
        {
            if (sr.ReadLine().Split(',').First() == name)
            {
                error.text = $"Името {name} е заето!";
                return false;
            }
        }
        PLAYER_INFO.NAME = name;
        return true;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
    