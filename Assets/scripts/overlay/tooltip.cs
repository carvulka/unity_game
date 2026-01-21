using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TOOLTIP_OVERLAY : MonoBehaviour
{
    [Header("configuration")]
    [SerializeField] Vector2 offset = new Vector2(32, 0);
    
    [Header("components/children")]
    [SerializeField] TextMeshProUGUI description_text;

    
    
    void Update()
    {
        if (Mouse.current != null)
        {
            this.transform.position = Mouse.current.position.ReadValue() + offset;
        }
    }
    
    public void show(string description)
    {
        this.description_text.text = description;
        this.gameObject.SetActive(true);
    }
    
    public void hide()
    {
        this.gameObject.SetActive(false);
    }
}
