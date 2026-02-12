using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using TMPro;

public class SPAWNER : MonoBehaviour
{
    [Header("configuration")]
    [SerializeField] List<GameObject> prefabs;
    [SerializeField] GameObject socket_prefab;
    [SerializeField] GameObject label_prefab;

    [Header("components")]
    [SerializeField] SESSION_DATA session_data;
    [SerializeField] INVENTORY inventory;



    void Start()
    {
        string level_path = Path.Combine(Application.persistentDataPath, GLOBAL.levels_directory_path, this.session_data.level_name);
        using FileStream stream = new FileStream(level_path, FileMode.Open, FileAccess.Read);
        XmlSerializer serializer = new XmlSerializer(typeof(XML_CONFIGURATION));
        XML_CONFIGURATION configuration = (XML_CONFIGURATION)serializer.Deserialize(stream);

        foreach (XML_PROP_SPAWN_POINT prop_spawn_point in configuration.prop_spawn_points)
        {
            GameObject new_object = new GameObject("prop_spawn_point");
            new_object.transform.position = prop_spawn_point.position;
            new_object.transform.rotation = Quaternion.Euler(prop_spawn_point.rotation);
            new_object.AddComponent<PROP_SPAWN_POINT>().prop_pool_id = prop_spawn_point.prop_pool_id;
        }

        foreach (XML_ITEM_SPAWN_POINT item_spawn_point in configuration.item_spawn_points)
        {
            GameObject new_object = new GameObject("item_spawn_point");
            new_object.transform.position = item_spawn_point.position;
            new_object.transform.rotation = Quaternion.Euler(item_spawn_point.rotation);
            new_object.AddComponent<ITEM_SPAWN_POINT>();
        }

        foreach (XML_PROP prop in configuration.props)
        {
            GameObject prefab = this.prefabs.Find(p => p.name == prop.prefab.name);
            if (prefab == null) { Debug.Log($"prefab with name '{prop.prefab.name}' was not found");  continue; }

            GameObject prop_object = Instantiate(prefab, prop.position, Quaternion.Euler(prop.rotation));

            if (prop.bin != null)
            {
                prop_object.AddComponent<BIN>().category_id = prop.bin.category_id;
            }

            foreach (XML_SOCKET socket in prop.sockets)
            {
                GameObject socket_object = Instantiate(this.socket_prefab, prop_object.transform);
                socket_object.transform.localPosition = socket.position;
                socket_object.transform.localRotation = Quaternion.Euler(socket.rotation);
                BoxCollider collider = socket_object.GetComponent<BoxCollider>();
                collider.size = socket.size;
                collider.center = new Vector3(0f, socket.size.y / 2f, 0f);
                socket_object.GetComponent<SOCKET>().category_id = socket.category_id;
            }

            foreach (XML_LABEL label in prop.labels)
            {
                GameObject label_object = Instantiate(this.label_prefab, prop_object.transform);
                label_object.transform.localPosition = label.position;
                label_object.transform.localRotation = Quaternion.Euler(label.rotation);
                label_object.GetComponent<RectTransform>().sizeDelta = new Vector2(label.width, label.height);
                TextMeshPro text = label_object.GetComponent<TextMeshPro>();
                text.text = label.text;
                text.alignment = (label.vertical_alignment, label.horizontal_alignment) switch
                {
                    ("top", "left")      => TextAlignmentOptions.TopLeft,
                    ("top", "center")    => TextAlignmentOptions.Top,
                    ("top", "right")     => TextAlignmentOptions.TopRight,
                    ("middle", "left")   => TextAlignmentOptions.Left,
                    ("middle", "center") => TextAlignmentOptions.Center,
                    ("middle", "right")  => TextAlignmentOptions.Right,
                    ("bottom", "left")   => TextAlignmentOptions.BottomLeft,
                    ("bottom", "center") => TextAlignmentOptions.Bottom,
                    ("bottom", "right")  => TextAlignmentOptions.BottomRight,
                    _ => TextAlignmentOptions.Center
                };
            }
        }

        foreach (PROP_SPAWN_POINT prop_spawn_point in FindObjectsByType<PROP_SPAWN_POINT>(FindObjectsSortMode.None))
        {
            this.spawn_prop(prop_spawn_point, configuration);
        }

        foreach (ITEM_SPAWN_POINT item_spawn_point in FindObjectsByType<ITEM_SPAWN_POINT>(FindObjectsSortMode.None).OrderBy(x => Random.value))
        {
            this.spawn_item(item_spawn_point, configuration);
        }
        this.inventory.set_total_tally_count(configuration.categories.Sum(e => e.current_spawn_count));
        //this.session_data.set_total_tally_count
        //this.session_data.set_max_score
    }

    void spawn_prop(PROP_SPAWN_POINT prop_spawn_point, XML_CONFIGURATION configuration)
    {
        XML_PROP_POOL prop_pool = configuration.prop_pools.Find(p => p.id == prop_spawn_point.prop_pool_id);
        if (prop_pool == null) { Debug.Log($"prop pool with id '{prop_spawn_point.prop_pool_id}' was not found"); return; }

        XML_PROP_POOL_ENTRY prop_pool_entry = this.sample_prop_pool(prop_pool);
        if (prop_pool_entry == null) { return; }
        
        GameObject prop_prefab = this.prefabs.Find(p => p.name == prop_pool_entry.prefab.name);
        if (prop_prefab == null) { Debug.Log($"prop prefab with name '{prop_pool_entry.prefab.name}' was not found"); return; }

        GameObject prop_object = Instantiate(prop_prefab, prop_spawn_point.transform.position, prop_spawn_point.transform.rotation);
        prop_object.AddComponent<Rigidbody>().mass = prop_pool_entry.mass;
    }

    void spawn_item(ITEM_SPAWN_POINT item_spawn_point, XML_CONFIGURATION configuration)
    {
        XML_ITEM_POOL_ENTRY item_pool_entry = this.sample_item_pool(configuration.item_pool);
        if (item_pool_entry == null) { return; }

        GameObject item_prefab = this.prefabs.Find(p => p.name == item_pool_entry.prefab.name);
        if (item_prefab == null) { return; }
        
        GameObject item_object = Instantiate(item_prefab, item_spawn_point.transform.position, item_spawn_point.transform.rotation);

        XML_CATEGORY category = configuration.categories.Find(e => e.id == item_pool_entry.category_id);
        if (item_pool_entry.item_type == null)
        {
            item_pool_entry.item_type = ScriptableObject.CreateInstance<ITEM.TYPE>();
            item_pool_entry.item_type.prefab = item_prefab;
            item_pool_entry.item_type.sprite = this.load_sprite(item_pool_entry.image.path);
            item_pool_entry.item_type.description = item_pool_entry.description;
            item_pool_entry.item_type.mass = item_pool_entry.mass;
            item_pool_entry.item_type.category_id = category.id;
            item_pool_entry.item_type.score = category.score;
        }
        
        ITEM item = item_object.AddComponent<ITEM>();
        item.type = item_pool_entry.item_type;
        item.state = new ITEM.STATE { multiplier = 1f };

        item_object.AddComponent<Rigidbody>().mass = item_pool_entry.item_type.mass;

        category.current_spawn_count = category.current_spawn_count + 1;
        if (category.current_spawn_count == category.max_spawn_count)
        {
            configuration.item_pool.items.FindAll(e => e.category_id == category.id).ForEach(e => e.weight = 0);
        }
    }

    XML_PROP_POOL_ENTRY sample_prop_pool(XML_PROP_POOL prop_pool)
    {
        int total_weight = prop_pool.empty_weight + prop_pool.props.Sum(e => e.weight);
        int random = Random.Range(0, total_weight);
        int cumulative = 0;
        foreach (XML_PROP_POOL_ENTRY prop_pool_entry in prop_pool.props)
        {
            cumulative += prop_pool_entry.weight;
            if (random < cumulative)
            {
                return prop_pool_entry;
            }
        }
        return null;
    }
    
    XML_ITEM_POOL_ENTRY sample_item_pool(XML_ITEM_POOL item_pool)
    {
        int total_weight = item_pool.items.Sum(e => e.weight);
        int random = Random.Range(0, total_weight);
        int cumulative = 0;
        foreach (XML_ITEM_POOL_ENTRY item_pool_entry in item_pool.items)
        {
            cumulative += item_pool_entry.weight;
            if (random < cumulative)
            {
                return item_pool_entry;
            }
        }
        return null;
    }

    

    public Sprite load_sprite(string image_name)
    {
        string image_path = Path.Combine(Application.persistentDataPath, GLOBAL.images_directory_path, image_name);
        if (!File.Exists(image_path))
        {
            Debug.Log($"image not found at '{image_path}'");
            return null;
        }
        byte[] data = File.ReadAllBytes(image_path);
        //fix (apparently not garbage collected)
        Texture2D texture = new Texture2D(1, 1);
        if (!texture.LoadImage(data))
        {
            Debug.Log($"failed to load image at '{image_path}'");
            return null;
        }
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}

[XmlRoot("configuration")]
public class XML_CONFIGURATION
{
    [XmlArray("categories")]
    [XmlArrayItem("category")]
    public List<XML_CATEGORY> categories = new List<XML_CATEGORY>();
    
    [XmlElement("item_pool")]
    public XML_ITEM_POOL item_pool;

    [XmlArray("prop_pools")]
    [XmlArrayItem("prop_pool")]
    public List<XML_PROP_POOL> prop_pools = new List<XML_PROP_POOL>();

    [XmlArray("item_spawn_points")]
    [XmlArrayItem("item_spawn_point")]
    public List<XML_ITEM_SPAWN_POINT> item_spawn_points = new List<XML_ITEM_SPAWN_POINT>();

    [XmlArray("prop_spawn_points")]
    [XmlArrayItem("prop_spawn_point")]
    public List<XML_PROP_SPAWN_POINT> prop_spawn_points = new List<XML_PROP_SPAWN_POINT>();

    [XmlArray("props")]
    [XmlArrayItem("prop")]
    public List<XML_PROP> props = new List<XML_PROP>();
}

public class XML_CATEGORY
{
    [XmlAttribute("id")]
    public int id;

    [XmlAttribute("score")]
    public int score;

    [XmlAttribute("max_spawn_count")]
    public int max_spawn_count;



    [XmlIgnore]
    public int current_spawn_count = 0;
}

public class XML_ITEM_POOL
{
    [XmlElement("item")]
    public List<XML_ITEM_POOL_ENTRY> items;
}

public class XML_ITEM_POOL_ENTRY
{
    [XmlAttribute("weight")]
    public int weight;

    [XmlAttribute("category_id")]
    public int category_id;
    
    [XmlElement("prefab")]
    public XML_PREFAB prefab;
    
    [XmlElement("image")]
    public XML_IMAGE image;

    [XmlElement("description")]
    public string description;

    [XmlElement("mass")]
    public float mass;

    
    
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
    public List<XML_PROP_POOL_ENTRY> props;
}

public class XML_PROP_POOL_ENTRY
{
    [XmlAttribute("weight")]
    public int weight;
    
    [XmlElement("prefab")]
    public XML_PREFAB prefab;

    [XmlElement("mass")]
    public float mass;
}

public class XML_ITEM_SPAWN_POINT
{
    [XmlElement("position")]
    public Vector3 position;

    [XmlElement("rotation")]
    public Vector3 rotation;
}

public class XML_PROP_SPAWN_POINT
{
    [XmlAttribute("prop_pool_id")]
    public int prop_pool_id;

    [XmlElement("position")]
    public Vector3 position;
    
    [XmlElement("rotation")]
    public Vector3 rotation;
}

public class XML_PROP
{
    [XmlElement("prefab")]
    public XML_PREFAB prefab;

    [XmlElement("position")]
    public Vector3 position;

    [XmlElement("rotation")]
    public Vector3 rotation;

    [XmlElement("bin")]
    public XML_BIN bin;

    [XmlArray("sockets")]
    [XmlArrayItem("socket")]
    public List<XML_SOCKET> sockets = new List<XML_SOCKET>();

    [XmlArray("labels")]
    [XmlArrayItem("label")]
    public List<XML_LABEL> labels = new List<XML_LABEL>();
}

public class XML_BIN
{
    [XmlAttribute("category_id")]
    public int category_id;
}

public class XML_SOCKET
{
    [XmlAttribute("category_id")]
    public int category_id;

    [XmlElement("position")]
    public Vector3 position;

    [XmlElement("rotation")]
    public Vector3 rotation;
    
    [XmlElement("size")]
    public Vector3 size;
}

public class XML_LABEL
{
    [XmlElement("text")]
    public string text;

    [XmlElement("horizontal_alignment")]
    public string horizontal_alignment;

    [XmlElement("vertical_alignment")]
    public string vertical_alignment;
    
    [XmlElement("position")]
    public Vector3 position;

    [XmlElement("rotation")]
    public Vector3 rotation;

    [XmlElement("width")]
    public float width;

    [XmlElement("height")]
    public float height;
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
