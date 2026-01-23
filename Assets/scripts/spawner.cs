using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SPAWNER : MonoBehaviour
{
    //constants
    public const string image_directory_path = "images";
    public const string configuration_path = "configuration.xml";

    [Header("configuration")]
    [SerializeField] List<GameObject> prefabs;



    void Start()
    {
        string path = Path.Combine(Application.persistentDataPath, configuration_path);
        if (!File.Exists(path))
        {
            Debug.LogError($"configuration not found at '{path}'");
            return;
        }

        XmlSerializer serializer = new XmlSerializer(typeof(XML_CONFIGURATION));

        using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);

        XML_CONFIGURATION xml_configuration = (XML_CONFIGURATION)serializer.Deserialize(stream);

        foreach (var xml_bin in xml_configuration.bins)
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

        SPAWN_POINT[] spawn_points = Object.FindObjectsByType<SPAWN_POINT>(FindObjectsSortMode.None);
        
        foreach (SPAWN_POINT spawn_point in spawn_points)
        {
            this.spawn(spawn_point, xml_configuration);
        }
    }

    void spawn(SPAWN_POINT spawn_point, XML_CONFIGURATION xml_configuration)
    {
        switch (spawn_point.type)
        {
            case SPAWN_POINT.TYPE.item:
                {
                    XML_ITEM xml_item = choose_item(xml_configuration.item_pool);
                    if (xml_item == null) { return; }
                    
                    GameObject prefab = this.prefabs.Find(p => p.name == xml_item.prefab.name);
                    if (prefab == null) { return; }
                    
                    GameObject instance = Instantiate(prefab, spawn_point.transform.position, spawn_point.transform.rotation);
                    if (instance == null) { return; }
                    
                    ITEM.TYPE item_type = ScriptableObject.CreateInstance<ITEM.TYPE>();
                    item_type.prefab = prefab;
                    item_type.sprite = this.load_sprite(xml_item.image.path);
                    item_type.description = xml_item.description;
                    foreach (var bin in xml_item.bin_scores)
                    {
                        item_type.bin_scores.Add(new ITEM.TYPE.BIN_SCORE { id = bin.id, score = bin.score });
                    }
                    break;
                }
            case SPAWN_POINT.TYPE.prop:
                {
                    XML_PROP xml_prop = choose_prop(xml_configuration.prop_pools.Find(p => p.id == spawn_point.pool_id));
                    if (xml_prop == null) { return; }
                    
                    GameObject prefab = this.prefabs.Find(p => p.name == xml_prop.prefab.name);
                    if (prefab == null) { return; }
                    
                    GameObject instance = Instantiate(prefab, spawn_point.transform.position, spawn_point.transform.rotation);
                    if (instance == null) { return; }
                    
                    SPAWN_POINT[] child_spawn_points = instance.GetComponentsInChildren<SPAWN_POINT>(true);
                    foreach (SPAWN_POINT child_spawn_point in child_spawn_points)
                    {
                        this.spawn(child_spawn_point, xml_configuration);
                    }
                    break;
                }
        }
    }

    XML_ITEM choose_item(XML_ITEM_POOL xml_item_pool)
    {
        int total_weight = xml_item_pool.no_spawn_weight + xml_item_pool.items.Sum(i => i.spawn_weight);
        int random = Random.Range(0, total_weight);
        int cumulative = 0;
        foreach (XML_ITEM item in xml_item_pool.items)
        {
            cumulative += item.spawn_weight;
            if (random <= cumulative)
            {
                return item;
            }
        }
        return null;
    }

    XML_PROP choose_prop(XML_PROP_POOL xml_prop_pool)
    {
        int total_weight = xml_prop_pool.no_spawn_weight + xml_prop_pool.props.Sum(i => i.spawn_weight);
        int random = Random.Range(0, total_weight);
        int cumulative = 0;
        foreach (XML_PROP prop in xml_prop_pool.props)
        {
            cumulative += prop.spawn_weight;
            if (random <= cumulative)
            {
                return prop;
            }
        }
        return null;
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
    [XmlElement("item_pool")]
    public XML_ITEM_POOL item_pool;
    
    [XmlArray("prop_pools")]
    [XmlArrayItem("prop_pool")]
    public List<XML_PROP_POOL> prop_pools;

    [XmlArray("bins")]
    [XmlArrayItem("bin")]
    public List<XML_BIN> bins;
}

public class XML_ITEM_POOL
{
    [XmlAttribute("no_spawn_weight")]
    public int no_spawn_weight;
    
    [XmlElement("item")]
    public List<XML_ITEM> items;
}

public class XML_ITEM
{
    [XmlElement("prefab")]
    public XML_PREFAB prefab;

    [XmlElement("description")]
    public string description;

    [XmlElement("image")]
    public XML_IMAGE image;

    [XmlElement("spawn_weight")]
    public int spawn_weight;

    [XmlArray("bins")]
    [XmlArrayItem("bin")]
    public List<XML_BIN_SCORE> bin_scores;
}

public class XML_PROP_POOL
{
    [XmlAttribute("id")]
    public int id;

    [XmlAttribute("no_spawn_weight")]
    public int no_spawn_weight;

    [XmlElement("prop")]
    public List<XML_PROP> props;
}

public class XML_PROP
{
    [XmlElement("prefab")]
    public XML_PREFAB prefab;

    [XmlElement("spawn_weight")]
    public int spawn_weight;
}

public class XML_PREFAB
{
    [XmlAttribute("name")]
    public string name;
}

public class XML_IMAGE
{
    [XmlAttribute("path")]
    public string path;
}

public class XML_BIN_SCORE
{
    [XmlAttribute("id")]
    public int id;

    [XmlAttribute("score")]
    public int score;
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

public class XML_TRANSFORM 
{
    [XmlElement("position")]
    public Vector3 position;

    [XmlElement("rotation")]
    public Vector3 rotation;
}