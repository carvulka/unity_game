using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class SPAWNER : MonoBehaviour
{
    //constants
    public const string image_directory_path = "images";
    public const string configuration_path = "configuration.xml";
    
	[Header("configuration")]
    [SerializeField] List<GameObject> prefabs;

    //state
    Dictionary<int, ITEM.TYPE> map;

    
    
    void Start()
    {
        string path = Path.Combine(Application.persistentDataPath, configuration_path);
        if (!File.Exists(path))
        {
            Debug.LogError($"configuration not found at '{path}'");
            return;
        }

        XmlSerializer serializer = new XmlSerializer(typeof(XML_CONFIGURATION));

        using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
        {
            XML_CONFIGURATION configuration = (XML_CONFIGURATION)serializer.Deserialize(stream);

            foreach (var xml_item_type in configuration.item_types)
            {
                GameObject prefab = this.prefabs.Find(p => p.name == xml_item_type.prefab.name);
                if (prefab == null)
                {
                    Debug.LogWarning($"prefab with name '{xml_item_type.prefab.name}' was not found");
                    continue;
                }

                ITEM.TYPE type = ScriptableObject.CreateInstance<ITEM.TYPE>();
                type.prefab = prefab;
                //type.sprite = load_sprite();
                type.description = xml_item_type.description;
                foreach (var bin in xml_item_type.bins)
                {
                    type.bins.Add(new ITEM.TYPE.BIN { id = bin.id, score = bin.score });
                }
                map.Add(xml_item_type.id, type);
            }

            foreach (var xml_item in configuration.items)
            {
                ITEM.TYPE type = map[xml_item.id];
                GameObject game_object = Instantiate(type.prefab, xml_item.transform.position, Quaternion.Euler(xml_item.transform.rotation));

                ITEM item = game_object.AddComponent<ITEM>();
                item.type = type;
                item.state = new ITEM.STATE { multiplier = 1f };
            }

            foreach (var xml_bin in configuration.bins)
            {
                GameObject prefab = this.prefabs.Find(p => p.name == xml_bin.prefab.name);
                if (prefab == null)
                {
                    Debug.LogWarning($"prefab with name '{xml_bin.prefab.name}' was not found");
                    continue;
                }
                GameObject game_object = Instantiate(prefab, xml_bin.transform.position, Quaternion.Euler(xml_bin.transform.rotation));

                BIN bin = game_object.AddComponent<BIN>();
                bin.id = xml_bin.id;
            }

            foreach (var xml_prop in configuration.props)
            {
                GameObject prefab = this.prefabs.Find(p => p.name == xml_prop.prefab.name);
                if (prefab == null)
                {
                    Debug.LogWarning($"prefab with name '{xml_prop.prefab.name}' was not found");
                    continue;
                }
                GameObject game_object = Instantiate(prefab, xml_prop.transform.position, Quaternion.Euler(xml_prop.transform.rotation));
            }
        }
    }

    public Sprite load_sprite(string relative_path)
    {
        string path = Path.Combine(Application.persistentDataPath, image_directory_path, relative_path);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"image not found at '{path}'");
            return null;
        }
        byte[] data = File.ReadAllBytes(path);
        //fix (apparently not garbage collected)
        Texture2D texture = new Texture2D(1, 1);
        if (!texture.LoadImage(data))
        {
            Debug.LogWarning($"failed to load image at '{path}'");
            return null;
        }
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}

[XmlRoot("configuration")]
public class XML_CONFIGURATION
{
    [XmlArray("shared_data_array")]
    [XmlArrayItem("shared_data")]
    public List<XML_ITEM_TYPE> item_types = new List<XML_ITEM_TYPE>();

    [XmlArray("items")]
    [XmlArrayItem("item")]
    public List<XML_ITEM> items = new List<XML_ITEM>();
    
    [XmlArray("bins")]
    [XmlArrayItem("bin")]
    public List<XML_BIN> bins = new List<XML_BIN>();
    
    [XmlArray("props")]
    [XmlArrayItem("prop")]
    public List<XML_PROP> props = new List<XML_PROP>();
}

public class XML_ITEM_TYPE
{
    [XmlAttribute("id")]
    public int id;

    [XmlElement("prefab")]
    public XML_PREFAB prefab;

    [XmlElement("description")]
    public string description;

    [XmlArray("bins")]
    [XmlArrayItem("bin")]
    public List<XML_ITEM_TYPE_BIN> bins = new List<XML_ITEM_TYPE_BIN>();
}

public class XML_ITEM_TYPE_BIN
{
    [XmlAttribute("id")]
    public int id;
    
    [XmlAttribute("score")]
    public float score;
}

public class XML_ITEM
{
    [XmlAttribute("id")]
    public int id;

    [XmlElement("transform")]
    public XML_TRANSFORM transform;
}

public class XML_BIN
{
    [XmlAttribute("id")]
    public int id;
    
    [XmlElement("prefab")]
    public XML_PREFAB prefab;

    [XmlElement("transform")]
    public XML_TRANSFORM transform;
}

public class XML_PROP
{
    [XmlElement("prefab")]
    public XML_PREFAB prefab;

    [XmlElement("transform")]
    public XML_TRANSFORM transform;
}

public class XML_PREFAB
{
    [XmlAttribute("name")]
    public string name;
}

public class XML_TRANSFORM
{
    [XmlElement("position")]
    public Vector3 position;
    
    [XmlElement("rotation")]
    public Vector3 rotation;
}
