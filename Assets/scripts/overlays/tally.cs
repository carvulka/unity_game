using UnityEngine;
using TMPro;

public class TALLY : MonoBehaviour
{
    [Header("components")]
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] INVENTORY inventory;
    
    
    
    void OnEnable()
    {
        this.inventory.tally_event += this.refresh;
        this.refresh();
    }
    
    void OnDisable()
    {
        this.inventory.tally_event -= this.refresh;
    }
    
    void refresh()
    {
        this.text.text = this.inventory.get_current_tally_count().ToString() + "/" + this.inventory.get_total_tally_count().ToString();
    }
}
