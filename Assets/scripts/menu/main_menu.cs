using NUnit.Framework;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class MAIN_MENU : MonoBehaviour
{
    //constants
    public const string leaderboard_path = "leaderboard.txt";
    
	[Header("components/children")]
    [SerializeField] TMP_InputField input_field;
    [SerializeField] TMP_Text error;

    
    
    public void LoadLevel1()
    {
        if (!this.save_name())
        {
            return;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }



    private bool save_name()
    {
        string name = this.input_field.text.Trim();
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
        StreamReader reader = new StreamReader(Path.Combine(Application.persistentDataPath, leaderboard_path));
        using (reader)
        {
            if (reader.ReadLine().Split(',').First() == name)
            {
                error.text = $"Името {name} е заето!";
                return false;
            }
        }
        PLAYER_INFO.name = name;
        return true;
    }

    public void quit()
    {
        Application.Quit();
    }
}
    