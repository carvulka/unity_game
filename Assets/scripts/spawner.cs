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
    [SerializeField] INVENTORY inventory;



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

        PROP_SPAWN_POINT[] prop_spawn_points = Object.FindObjectsByType<PROP_SPAWN_POINT>(FindObjectsSortMode.None);
        foreach (PROP_SPAWN_POINT prop_spawn_point in prop_spawn_points)
        {
            this.spawn_prop(prop_spawn_point, xml_configuration);
        }

        ITEM_SPAWN_POINT[] item_spawn_points = Object.FindObjectsByType<ITEM_SPAWN_POINT>(FindObjectsSortMode.None);
        var shuffled_item_spawn_points = item_spawn_points.OrderBy(x => Random.value);
        int item_count = 0;
        foreach (ITEM_SPAWN_POINT item_spawn_point in shuffled_item_spawn_points)
        {
            if (this.spawn_item(item_spawn_point, xml_configuration))
            {
                item_count++;
            }
        }
        this.inventory.set_total_count(item_count);
    }

    void spawn_prop(PROP_SPAWN_POINT prop_spawn_point, XML_CONFIGURATION xml_configuration)
    {
        XML_PROP_POOL xml_prop_pool = xml_configuration.prop_pools.Find(p => p.id == prop_spawn_point.prop_pool_id);
        if (xml_prop_pool == null) { return; }

        XML_PROP xml_prop = this.choose_prop(xml_prop_pool);
        if (xml_prop == null) { return; }
        
        GameObject prefab = this.prefabs.Find(p => p.name == xml_prop.prefab.name);
        if (prefab == null) { return; }

        GameObject new_object = Instantiate(prefab, prop_spawn_point.transform.position, prop_spawn_point.transform.rotation);
        if (new_object == null) { return; }

        PROP_SPAWN_POINT[] child_spawn_points = new_object.GetComponentsInChildren<PROP_SPAWN_POINT>(true);
        foreach (PROP_SPAWN_POINT child_spawn_point in child_spawn_points)
        {
            this.spawn_prop(child_spawn_point, xml_configuration);
        }
    }

    bool spawn_item(ITEM_SPAWN_POINT item_spawn_point, XML_CONFIGURATION xml_configuration)
    {
        XML_ITEM_POOL xml_item_pool = xml_configuration.item_pools.Find(p => p.id == item_spawn_point.item_pool_id);
        if (xml_item_pool == null) { return false; }

        if (xml_item_pool.current_spawn_count >= xml_item_pool.spawn_count) { return false; }

        XML_ITEM xml_item = choose_item(xml_item_pool);
        if (xml_item == null) { return false; }
        
        GameObject prefab = this.prefabs.Find(p => p.name == xml_item.prefab.name);
        if (prefab == null) { return false; }
        
        GameObject new_object = Instantiate(prefab, item_spawn_point.transform.position, item_spawn_point.transform.rotation);
        if (new_object == null) { return false; }

        if (xml_item.item_type == null)
        {
            xml_item.item_type = ScriptableObject.CreateInstance<ITEM.TYPE>();
            xml_item.item_type.prefab = prefab;
            xml_item.item_type.sprite = this.load_sprite(xml_item.image.path);
            xml_item.item_type.description = xml_item.description;
            xml_item.item_type.score = xml_item.score;
            xml_item.item_type.target_id = xml_item_pool.id;
        }
        
        ITEM item = new_object.AddComponent<ITEM>();
        item.type = xml_item.item_type;
        item.state = new ITEM.STATE { multiplier = 1f };
        
        xml_item_pool.current_spawn_count = xml_item_pool.current_spawn_count + 1;
        return true;
    }

    XML_ITEM choose_item(XML_ITEM_POOL xml_item_pool)
    {
        int total_weight = xml_item_pool.items.Sum(i => i.spawn_weight);
        int random = Random.Range(0, total_weight);
        int cumulative = 0;
        foreach (XML_ITEM item in xml_item_pool.items)
        {
            cumulative += item.spawn_weight;
            if (random < cumulative)
            {
                return item;
            }
        }
        return null;
    }

    XML_PROP choose_prop(XML_PROP_POOL xml_prop_pool)
    {
        int total_weight = xml_prop_pool.empty_weight + xml_prop_pool.props.Sum(i => i.spawn_weight);
        int random = Random.Range(0, total_weight);
        int cumulative = 0;
        foreach (XML_PROP prop in xml_prop_pool.props)
        {
            cumulative += prop.spawn_weight;
            if (random < cumulative)
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
    [XmlArray("item_pools")]
    [XmlArrayItem("item_pool")]
    public List<XML_ITEM_POOL> item_pools;
    
    [XmlArray("prop_pools")]
    [XmlArrayItem("prop_pool")]
    public List<XML_PROP_POOL> prop_pools;
}

public class XML_ITEM_POOL
{
    [XmlAttribute("id")]
    public int id;
    
    [XmlAttribute("spawn_count")]
    public int spawn_count;

    [XmlElement("item")]
    public List<XML_ITEM> items;

    [XmlIgnore]
    public int current_spawn_count;
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

    [XmlElement("score")]
    public float score;

    [XmlIgnore]
    public ITEM.TYPE item_type = null;
}

public class XML_PROP_POOL
{
    [XmlAttribute("id")]
    public int id;

    [XmlAttribute("empty_weight")]
    public int empty_weight;

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
