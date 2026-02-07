using UnityEngine;
using TMPro;

public class SCORE : MonoBehaviour
{
    [Header("components")]
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] INVENTORY inventory;
    
    
    
    void OnEnable()
    {
        this.inventory.score_event += this.refresh;
        this.refresh();
    }
    
    void OnDisable()
    {
        this.inventory.score_event -= this.refresh;
    }
    
    void refresh()
    {
        this.text.text = this.inventory.get_score().ToString();
    }
}
