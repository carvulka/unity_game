using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class CURSOR_SLOT : MonoBehaviour
{
    [Header("components")]
    [SerializeField] Image image;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] INVENTORY inventory;
    
    
    
    void OnEnable()
    {
        this.inventory.get_cursor_slot().modify_event += this.refresh;
        this.refresh();
    }
    
    void OnDisable()
    {
        this.inventory.get_cursor_slot().modify_event -= this.refresh;
    }
    
    void Update()
    {
        if (Mouse.current != null)
        {
            this.transform.position = Mouse.current.position.ReadValue();
        }
    }
    
    void refresh()
    {
        this.image.sprite = this.inventory.get_cursor_slot().count() > 0 ? this.inventory.get_cursor_slot().type.sprite : null;
        this.image.enabled = this.inventory.get_cursor_slot().count() > 0;
        this.text.text = this.inventory.get_cursor_slot().count() > 1 ? this.inventory.get_cursor_slot().count().ToString() : null;
        this.text.enabled = this.inventory.get_cursor_slot().count() > 1;
    }
}
