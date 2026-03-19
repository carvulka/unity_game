using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class BELT_SLOT : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("components")]
    [SerializeField] Image image;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] TOOLTIP tooltip;
    [SerializeField] GameObject border;
    [SerializeField] INVENTORY inventory;
    [SerializeField] int index;

    
    
    void OnEnable()
    {
        this.inventory.get_belt_slots()[this.index].modify_event += this.refresh;
        this.inventory.get_belt_slots()[this.index].select_event += this.set_border;
        this.refresh();
    }
    
    void OnDisable()
    {
        this.inventory.get_belt_slots()[this.index].modify_event -= this.refresh;
        this.inventory.get_belt_slots()[this.index].select_event -= this.set_border;
    }
    
    void set_border()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.transform.parent as RectTransform);
        this.border.transform.position = this.transform.position;
    }
    
    public void OnPointerEnter(PointerEventData event_data)
    {
        if (this.inventory.get_belt_slots()[this.index].count() > 0)
        {
            this.tooltip.show(this.inventory.get_belt_slots()[this.index].type.description);
        }
    }

    public void OnPointerExit(PointerEventData event_data)
    {
        if (this.inventory.get_belt_slots()[this.index].count() > 0)
        {
            this.tooltip.hide();
        }
    }

    public void OnPointerDown(PointerEventData event_data)
    {
        if (event_data.button == PointerEventData.InputButton.Left)
        {
            this.inventory.get_cursor_slot().exchange(this.inventory.get_belt_slots()[this.index]);
            if (this.inventory.get_belt_slots()[this.index].count() > 0)
            {
                this.tooltip.show(this.inventory.get_belt_slots()[this.index].type.description);
            }
            else
            {
                this.tooltip.hide();
            }
        }
    }

    void refresh()
    {
        this.image.sprite = this.inventory.get_belt_slots()[this.index].count() > 0 ? this.inventory.get_belt_slots()[this.index].type.sprite : null;
        this.image.enabled = this.inventory.get_belt_slots()[this.index].count() > 0;
        this.text.text = this.inventory.get_belt_slots()[this.index].count() > 1 ? this.inventory.get_belt_slots()[this.index].count().ToString() : null;
        this.text.enabled = this.inventory.get_belt_slots()[this.index].count() > 1;
    }
}
