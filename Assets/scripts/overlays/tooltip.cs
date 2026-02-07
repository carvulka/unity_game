using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TOOLTIP : MonoBehaviour
{
    [Header("components")]
    [SerializeField] TextMeshProUGUI text;

    
    
    void Update()
    {
        if (Mouse.current != null)
        {
            this.transform.position = Mouse.current.position.ReadValue();
        }
    }
    
    public void show(string description)
    {
        this.text.text = description;
        this.gameObject.SetActive(true);
    }
    
    public void hide()
    {
        this.gameObject.SetActive(false);
    }
}
