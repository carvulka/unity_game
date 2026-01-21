using UnityEngine;
using TMPro;

public class SCORE_OVERLAY : MonoBehaviour
{
    [Header("components/children")]
    [SerializeField] TextMeshProUGUI score_text;

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
        this.score_text.text = this.inventory.get_score().ToString();
    }
}
