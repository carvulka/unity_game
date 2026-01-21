using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class CURSOR_SLOT_OVERLAY : MonoBehaviour
{
    [Header("components/children")]
    [SerializeField] Image image;
    [SerializeField] TextMeshProUGUI count_text;

    [Header("configuration")]
    [SerializeField] INVENTORY inventory;
    
    
    
    void OnEnable()
    {
        this.inventory.cursor_slot.modified += this.refresh;
        this.refresh();
    }
    
    void OnDisable()
    {
        this.inventory.cursor_slot.modified -= this.refresh;
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
        this.image.sprite = this.inventory.cursor_slot.count() > 0 ? this.inventory.cursor_slot.get_type().sprite : null;
        this.image.enabled = this.inventory.cursor_slot.count() > 0;
        this.count_text.text = this.inventory.cursor_slot.count() > 1 ? this.inventory.cursor_slot.count().ToString() : null;
        this.count_text.enabled = this.inventory.cursor_slot.count() > 1;
    }
}
