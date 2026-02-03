using UnityEngine;
using System.Collections.Generic;

public class SPINNER : MonoBehaviour
{
	[Header("configuration")]
    [SerializeField] public List<GameObject> models;
    [SerializeField] public Vector3 rotation = new Vector3(-2f, -4f, 4);
    [SerializeField] public Vector3 scale = new Vector3(32f, 32f, 32f);

    void Update()
    {
        this.transform.Rotate(this.rotation * Time.deltaTime);
    }
    
    void Start()
    {
        if (this.models.Count > 0)
        {
            int random = Random.Range(0, this.models.Count);
            GameObject new_object = Instantiate(this.models[random], Vector3.zero, Quaternion.Euler(15, 30, -15));
            new_object.transform.SetParent(this.transform);
            new_object.transform.localScale = this.scale;
        }
    }
}