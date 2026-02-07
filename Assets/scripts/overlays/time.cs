using UnityEngine;
using TMPro;

public class TIME : MonoBehaviour
{
    [Header("components")]
    [SerializeField] TextMeshProUGUI text;

    
    
    void Update()
    {
        int minutes = Mathf.FloorToInt(Time.timeSinceLevelLoad / 60);
        float seconds = Time.timeSinceLevelLoad % 60;
        this.text.text = $"{minutes:D2}:{seconds:00.000}";
    }
}
