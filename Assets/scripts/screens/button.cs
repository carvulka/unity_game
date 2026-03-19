using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BUTTON : MonoBehaviour
{
    [Header("configuration")]
    [SerializeField] Color default_color = new Color(1f, 1f, 1f);
    [SerializeField] Color hover_color = new Color(0f, 0f, 0f);
    [SerializeField] bool is_alternate = false;
    
    [Header("components")]
    [SerializeField] TMP_Text label;

    [Header("state")]
    [SerializeField] string text;
    
    
    
    void OnValidate()
    {
        this.set_default();
    }


    
    public void set_text(string text)
    {
        this.text = text;
        this.set_default();
    }
    
    public string get_text()
    {
        return this.text;
    }
    
    public void set_default()
    {
        this.label.text = this.is_alternate? this.text : $" {this.text}";
        this.label.color = this.default_color;
    }

    public void set_hover()
    {
        this.label.text = this.is_alternate? $"[{this.text}]" : $">{this.text}";
        this.label.color = this.hover_color;
    }
}
