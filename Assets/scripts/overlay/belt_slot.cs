using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class BELT_SLOT_OVERLAY : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("components/children")]
    [SerializeField] Image image;
    [SerializeField] TextMeshProUGUI count_text;
    [SerializeField] TOOLTIP_OVERLAY tooltip;
    [SerializeField] GameObject border;

    [Header("configuration")]
    [SerializeField] INVENTORY inventory;
    [SerializeField] int index;

    
    
    void OnEnable()
    {
        this.inventory.belt_slots[this.index].modified += this.refresh;
        this.inventory.belt_slots[this.index].selected += this.set_border;
        this.refresh();
    }
    
    void OnDisable()
    {
        this.inventory.belt_slots[this.index].modified -= this.refresh;
        this.inventory.belt_slots[this.index].selected -= this.set_border;
    }
    
    void set_border()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.transform.parent as RectTransform);
        this.border.transform.position = this.transform.position;
    }
    
    public void OnPointerEnter(PointerEventData event_data)
    {
        if (this.inventory.belt_slots[this.index].count() > 0)
        {
            this.tooltip.show(this.inventory.belt_slots[this.index].get_type().description);
        }
    }

    public void OnPointerExit(PointerEventData event_data)
    {
        if (this.inventory.belt_slots[this.index].count() > 0)
        {
            this.tooltip.hide();
        }
    }

    public void OnPointerDown(PointerEventData event_data)
    {
        if (event_data.button == PointerEventData.InputButton.Left)
        {
            this.inventory.cursor_slot.exchange(this.inventory.belt_slots[this.index]);
            if (this.inventory.belt_slots[this.index].count() > 0)
            {
                this.tooltip.show(this.inventory.belt_slots[this.index].get_type().description);
            }
            else
            {
                this.tooltip.hide();
            }
        }
    }

    void refresh()
    {
        this.image.sprite = this.inventory.belt_slots[this.index].count() > 0 ? this.inventory.belt_slots[this.index].get_type().sprite : null;
        this.image.enabled = this.inventory.belt_slots[this.index].count() > 0;
        this.count_text.text = this.inventory.belt_slots[this.index].count() > 1 ? this.inventory.belt_slots[this.index].count().ToString() : null;
        this.count_text.enabled = this.inventory.belt_slots[this.index].count() > 1;
    }
}
