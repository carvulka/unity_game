using UnityEngine;
using TMPro;

public class TALLY_OVERLAY : MonoBehaviour
{
    [Header("components/children")]
    [SerializeField] TextMeshProUGUI tally_text;

    [Header("configuration")]
    [SerializeField] INVENTORY inventory;
    
    
    
    void OnEnable()
    {
        this.inventory.scored += this.refresh;
        this.refresh();
    }
    
    void OnDisable()
    {
        this.inventory.scored -= this.refresh;
    }
    
    void refresh()
    {
        this.tally_text.text = this.inventory.get_sorted_count().ToString() + "/" + this.inventory.get_total_count().ToString();
    }
}
