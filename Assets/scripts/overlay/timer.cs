using UnityEngine;
using TMPro;

public class TIMER_OVERLAY : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI timer_text;

    
    
    void Update()
    {
        int minutes = Mathf.FloorToInt(Time.timeSinceLevelLoad / 60);
        int seconds = Mathf.FloorToInt(Time.timeSinceLevelLoad % 60);
        this.timer_text.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
