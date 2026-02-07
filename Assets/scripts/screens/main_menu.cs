using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.IO;
using TMPro;
using System.Xml;

public class MAIN_MENU : MonoBehaviour
{
    [Header("configuration")]
    [SerializeField] GameObject level_button_prefab;
    [SerializeField] GameObject control_listing_prefab;
    [SerializeField] Color warning_color = new Color(1f, 1f, 0f);
    [SerializeField] Color error_color = new Color(1f, 0f, 0f);
    
    [Header("components")]
    [SerializeField] SESSION_DATA session_data;
    [SerializeField] TMP_InputField input_field;
    [SerializeField] TMP_Text input_feedback_text;
    [SerializeField] GameObject naming_menu;
    [SerializeField] GameObject selection_menu;
    [SerializeField] GameObject level_buttons_content_object;
    [SerializeField] GameObject control_listings_content_object;



    void Start()
    {
        this.input_field.onValueChanged.AddListener((text) => this.validate_name(text));

        this.create_control_listings();

        this.create_level_buttons();
    }
    


    public void create_control_listings()
    {
        InputActionMap character_actions = InputSystem.actions.FindActionMap("character");
        foreach (var input_action in character_actions.actions)
        {
            GameObject control_listing_object = Instantiate(this.control_listing_prefab, this.control_listings_content_object.transform);
            control_listing_object.transform.Find("name").GetComponent<TMP_Text>().text = $"{input_action.name}:";
            control_listing_object.transform.Find("value").GetComponent<TMP_Text>().text = input_action.GetBindingDisplayString() switch
            {
                "Delta" => "mouse",
                "W|Up Arrow/A|Left Arrow/S|Down Arrow/D|Right Arrow" => "wasd",
                "LMB" => "left mouse click",
                "E" => "e",
                "RMB" => "right mouse click",
                "Scroll" => "scroll",
                "\t" => "tab",
                _ => input_action.GetBindingDisplayString(),
            };
        }
    }

    public void create_level_buttons()
    {
        string levels_directory_path = Path.Combine(Application.persistentDataPath, GLOBAL.levels_directory_path);
        if (!Directory.Exists(levels_directory_path))
        {
            Debug.Log($"'{levels_directory_path}' directory is missing");
            return;
        }
        string[] file_paths = Directory.GetFiles(levels_directory_path);
        foreach (string file_path in file_paths)
        {
            string file_name = Path.GetFileName(file_path);
            if (file_name.Contains(',') || file_name.Contains('\n'))
            {
                Debug.Log($"'{file_name}' file name contains ',' or '\\n'");
                continue;
            }
            GameObject level_button_object = Instantiate(this.level_button_prefab, this.level_buttons_content_object.transform);
            level_button_object.GetComponent<BUTTON>().set_text(file_name);
            level_button_object.GetComponent<Button>().onClick.AddListener(() => load_level(file_name));
        }
    }

    public void init_level_buttons()
    {
        foreach (Transform button_object in level_buttons_content_object.transform)
        {
            string prerequisite = this.get_prerequisite(button_object.GetComponent<BUTTON>().get_text());
            if (prerequisite != null && !player_meets_prerequisite(prerequisite))
            {
                button_object.gameObject.SetActive(false);
            }
            else
            {
                button_object.gameObject.SetActive(true);
            }
        }
    }
    
    public string get_prerequisite(string level_name)
    {
        string level_path = Path.Combine(Application.persistentDataPath, GLOBAL.levels_directory_path, level_name);
        using XmlReader reader = XmlReader.Create(level_path);
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element && reader.Name == "prerequisite")
            {
                return reader.ReadElementContentAsString();
            }
        }
        return null;
    }

    public bool player_meets_prerequisite(string prerequisite)
    {
        string leaderboard_path = Path.Combine(Application.persistentDataPath, GLOBAL.leaderboard_path);
        if (!File.Exists(leaderboard_path))
        {
            return false;
        }

        using StreamReader reader = new StreamReader(leaderboard_path);
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            string[] fields = line.Split(',');
            if (fields.Length >= 2 && fields[0] == this.session_data.player_name && fields[1] == prerequisite)
            {
                return true;
            }
        }
        return false;
    }

    bool validate_name(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            this.input_feedback_text.text = "name can not be empty.";
            this.input_feedback_text.color = this.error_color;
            return false;
        }
        if (name.Contains(',') || name.Contains('\n'))
        {
            this.input_feedback_text.text = "name can not contain ',' or '\\n'.";
            this.input_feedback_text.color = this.error_color;
            return false;
        }
        if (name_is_in_use(name))
        {
            this.input_feedback_text.text = $"'{name}' is already in use";
            this.input_feedback_text.color = this.warning_color;
            return true;
        }
        this.input_feedback_text.text = "";
        return true;
    }

    bool name_is_in_use(string name)
    {
        string leaderboard_path = Path.Combine(Application.persistentDataPath, GLOBAL.leaderboard_path);
        if (File.Exists(leaderboard_path))
        {
            using StreamReader reader = new StreamReader(leaderboard_path);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] field = line.Split(',');
                if (field.Length >= 1 && field[0] == name)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void save_name()
    {
        string name = this.input_field.text;
        if (this.validate_name(name))
        {
            this.session_data.player_name = name;
            this.init_level_buttons();
            this.naming_menu.SetActive(false);
            this.selection_menu.SetActive(true);
        }
    }
    
    public void load_level(string level_name)
    {
        this.session_data.level_name = level_name;
        SceneManager.LoadScene(GLOBAL.level_scene_number);
    }
    
    public void quit()
    {
        Application.Quit();
    }
}
    