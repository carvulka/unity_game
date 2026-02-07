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
        XML_CONFIGURATION xml_configuration = (XML_CONFIGURATION)serializer.Deserialize(stream);

        foreach (XML_PROP_SPAWN_POINT xml_prop_spawn_point in xml_configuration.prop_spawn_points)
        {
            GameObject new_object = new GameObject("prop_spawn_point");
            new_object.transform.position = xml_prop_spawn_point.position;
            new_object.transform.rotation = Quaternion.Euler(xml_prop_spawn_point.rotation);
            new_object.AddComponent<PROP_SPAWN_POINT>().prop_pool_id = xml_prop_spawn_point.prop_pool_id;
        }

        foreach (XML_ITEM_SPAWN_POINT xml_item_spawn_point in xml_configuration.item_spawn_points)
        {
            GameObject new_object = new GameObject("item_spawn_point");
            new_object.transform.position = xml_item_spawn_point.position;
            new_object.transform.rotation = Quaternion.Euler(xml_item_spawn_point.rotation);
            new_object.AddComponent<ITEM_SPAWN_POINT>().item_pool_id = xml_item_spawn_point.item_pool_id;
        }

        foreach (XML_STATIC_PROP xml_prop in xml_configuration.props)
        {
            GameObject prefab = this.prefabs.Find(p => p.name == xml_prop.prefab.name);
            if (prefab == null) { Debug.Log($"prefab with name '{xml_prop.prefab.name}' was not found");  continue; }

            GameObject prop_object = Instantiate(prefab, xml_prop.position, Quaternion.Euler(xml_prop.rotation));

            if (xml_prop.bin != null)
            {
                prop_object.AddComponent<BIN>().id = xml_prop.bin.id;
            }

            foreach (XML_SOCKET xml_socket in xml_prop.sockets)
            {
                GameObject socket_object = Instantiate(this.socket_prefab, prop_object.transform);
                socket_object.transform.localPosition = xml_socket.position;
                socket_object.transform.localRotation = Quaternion.Euler(xml_socket.rotation);
                BoxCollider collider = socket_object.GetComponent<BoxCollider>();
                collider.size = xml_socket.size;
                collider.center = new Vector3(0f, xml_socket.size.y / 2f, 0f);
                socket_object.GetComponent<SOCKET>().id = xml_socket.id;
            }

            foreach (XML_LABEL xml_label in xml_prop.labels)
            {
                GameObject label_object = Instantiate(this.label_prefab, prop_object.transform);
                label_object.transform.localPosition = xml_label.position;
                label_object.transform.localRotation = Quaternion.Euler(xml_label.rotation);
                label_object.GetComponent<RectTransform>().sizeDelta = new Vector2(xml_label.width, xml_label.height);
                TextMeshPro text = label_object.GetComponent<TextMeshPro>();
                text.text = xml_label.text;
                text.alignment = (xml_label.vertical_alignment, xml_label.horizontal_alignment) switch
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

        foreach (PROP_SPAWN_POINT prop_spawn_point in Object.FindObjectsByType<PROP_SPAWN_POINT>(FindObjectsSortMode.None))
        {
            this.spawn_prop(prop_spawn_point, xml_configuration);
        }

        foreach (ITEM_SPAWN_POINT item_spawn_point in Object.FindObjectsByType<ITEM_SPAWN_POINT>(FindObjectsSortMode.None).OrderBy(x => Random.value))
        {
            this.spawn_item(item_spawn_point, xml_configuration);
        }
        this.inventory.set_total_tally_count(xml_configuration.item_pools.Sum(pool => pool.current_spawn_count));
    }

    void spawn_prop(PROP_SPAWN_POINT prop_spawn_point, XML_CONFIGURATION xml_configuration)
    {
        XML_PROP_POOL xml_prop_pool = xml_configuration.prop_pools.Find(p => p.id == prop_spawn_point.prop_pool_id);
        if (xml_prop_pool == null) { Debug.Log($"prop pool with id '{prop_spawn_point.prop_pool_id}' was not found"); return; }

        XML_PROP xml_prop = this.sample_prop_pool(xml_prop_pool);
        if (xml_prop == null) { return; }
        
        GameObject prefab = this.prefabs.Find(p => p.name == xml_prop.prefab.name);
        if (prefab == null) { Debug.Log($"prop prefab with name '{xml_prop.prefab.name}' was not found"); return; }

        GameObject prop_object = Instantiate(prefab, prop_spawn_point.transform.position, prop_spawn_point.transform.rotation);
        prop_object.AddComponent<Rigidbody>().mass = xml_prop.mass;

        foreach (PROP_SPAWN_POINT child_spawn_point in prop_object.GetComponentsInChildren<PROP_SPAWN_POINT>(true))
        {
            this.spawn_prop(child_spawn_point, xml_configuration);
        }
    }

    void spawn_item(ITEM_SPAWN_POINT item_spawn_point, XML_CONFIGURATION xml_configuration)
    {
        XML_ITEM_POOL xml_item_pool = xml_configuration.item_pools.Find(p => p.id == item_spawn_point.item_pool_id);
        if (xml_item_pool == null) { Debug.Log($"item pool with id '{item_spawn_point.item_pool_id}' was not found"); return; }

        if (xml_item_pool.current_spawn_count >= xml_item_pool.spawn_count) { return; }

        XML_ITEM xml_item = sample_item_pool(xml_item_pool);
        if (xml_item == null) { return; }
        
        GameObject prefab = this.prefabs.Find(p => p.name == xml_item.prefab.name);
        if (prefab == null) { return; }
        
        GameObject item_object = Instantiate(prefab, item_spawn_point.transform.position, item_spawn_point.transform.rotation);

        if (xml_item.item_type == null)
        {
            xml_item.item_type = ScriptableObject.CreateInstance<ITEM.TYPE>();
            xml_item.item_type.prefab = prefab;
            xml_item.item_type.sprite = this.load_sprite(xml_item.image.path);
            xml_item.item_type.description = xml_item.description;
            xml_item.item_type.score = xml_item.score;
            xml_item.item_type.mass = xml_item.mass;
            xml_item.item_type.target_id = xml_item_pool.id;
        }
        
        ITEM item = item_object.AddComponent<ITEM>();
        item.type = xml_item.item_type;
        item.state = new ITEM.STATE { multiplier = 1f };

        item_object.AddComponent<Rigidbody>().mass = xml_item.item_type.mass;
        
        xml_item_pool.current_spawn_count = xml_item_pool.current_spawn_count + 1;
    }

    XML_ITEM sample_item_pool(XML_ITEM_POOL xml_item_pool)
    {
        int total_weight = xml_item_pool.empty_weight + xml_item_pool.items.Sum(i => i.spawn_weight);
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

    XML_PROP sample_prop_pool(XML_PROP_POOL xml_prop_pool)
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
    [XmlArray("item_pools")]
    [XmlArrayItem("item_pool")]
    public List<XML_ITEM_POOL> item_pools = new List<XML_ITEM_POOL>();

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
    public List<XML_STATIC_PROP> props = new List<XML_STATIC_PROP>();
}

public class XML_ITEM_SPAWN_POINT
{
    [XmlAttribute("item_pool_id")]
    public int item_pool_id;

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

public class XML_STATIC_PROP
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
    [XmlAttribute("id")]
    public int id;
}

public class XML_SOCKET
{
    [XmlAttribute("id")]
    public int id;

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

public class XML_ITEM_POOL
{
    [XmlAttribute("id")]
    public int id;

    [XmlAttribute("empty_weight")]
    public int empty_weight;

    [XmlAttribute("spawn_count")]
    public int spawn_count;

    [XmlElement("item")]
    public List<XML_ITEM> items;



    [XmlIgnore]
    public int current_spawn_count = 0;
}

public class XML_ITEM
{
    [XmlElement("spawn_weight")]
    public int spawn_weight;
    
    [XmlElement("prefab")]
    public XML_PREFAB prefab;
    
    [XmlElement("image")]
    public XML_IMAGE image;

    [XmlElement("description")]
    public string description;

    [XmlElement("score")]
    public float score;

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
    public List<XML_PROP> props;
}

public class XML_PROP
{
    [XmlElement("spawn_weight")]
    public int spawn_weight;
    
    [XmlElement("prefab")]
    public XML_PREFAB prefab;

    [XmlElement("mass")]
    public float mass;
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
