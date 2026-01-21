using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class BACKPACK_SLOT_OVERLAY : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("components/children")]
    [SerializeField] Image image;
    [SerializeField] TextMeshProUGUI count_text;
    [SerializeField] TOOLTIP_OVERLAY tooltip;

    [Header("configuration")]
    [SerializeField] INVENTORY inventory;
    [SerializeField] int index;

    
    
    void OnEnable()
    {
        this.inventory.backpack_slots[this.index].modified += this.refresh;
        this.refresh();
    }
    
    void OnDisable()
    {
        this.inventory.backpack_slots[this.index].modified -= this.refresh;
        //maybe should be handled in character.cs
        this.tooltip.hide();
    }
    
    public void OnPointerEnter(PointerEventData event_data)
    {
        if (this.inventory.backpack_slots[this.index].count() > 0)
        {
            this.tooltip.show(this.inventory.backpack_slots[this.index].get_type().description);
        }
    }

    public void OnPointerExit(PointerEventData event_data)
    {
        if (this.inventory.backpack_slots[this.index].count() > 0)
        {
            this.tooltip.hide();
        }
    }

    public void OnPointerDown(PointerEventData event_data)
    {
        if (event_data.button == PointerEventData.InputButton.Left)
        {
            this.inventory.cursor_slot.exchange(this.inventory.backpack_slots[this.index]);
            if (this.inventory.backpack_slots[this.index].count() > 0)
            {
                this.tooltip.show(this.inventory.backpack_slots[this.index].get_type().description);
            }
            else
            {
                this.tooltip.hide();
            }
        }
    }

    void refresh()
    {
        this.image.sprite = this.inventory.backpack_slots[this.index].count() > 0 ? this.inventory.backpack_slots[this.index].get_type().sprite : null;
        this.image.enabled = this.inventory.backpack_slots[this.index].count() > 0;
        this.count_text.text = this.inventory.backpack_slots[this.index].count() > 1 ? this.inventory.backpack_slots[this.index].count().ToString() : null;
        this.count_text.enabled = this.inventory.backpack_slots[this.index].count() > 1;
    }
}
