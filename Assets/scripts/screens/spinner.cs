using UnityEngine;
using System.Collections.Generic;

public class SPINNER : MonoBehaviour
{
    [Header("configuration")]
    [SerializeField] List<GameObject> prefabs = new List<GameObject>();
    [SerializeField] Vector3 rotation = new Vector3(15f, 30f, -15f);
    [SerializeField] Vector3 rotation_velocity = new Vector3(-2f, -4f, 4);
    [SerializeField] Vector3 scale = new Vector3(32f, 32f, 32f);

    void Update()
    {
        this.transform.Rotate(this.rotation_velocity * Time.deltaTime);
    }
    
    void Start()
    {
        if (this.prefabs.Count > 0)
        {
            GameObject new_object = Instantiate(this.prefabs[Random.Range(0, this.prefabs.Count)], Vector3.zero, Quaternion.Euler(this.rotation), this.transform);
            new_object.transform.localScale = this.scale;
        }
    }
}