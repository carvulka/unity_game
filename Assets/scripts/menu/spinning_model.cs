using UnityEngine;

public class SPINNING_MODEL : MonoBehaviour
{
	[Header("configuration")]
    [SerializeField] public Vector3 rotation = new Vector3(-2, -4, 4);

    void Update()
    {
        this.transform.Rotate(rotation * Time.deltaTime);
    }
}
