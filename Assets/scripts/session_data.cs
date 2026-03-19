using UnityEngine;

[CreateAssetMenu(fileName = "session_data", menuName = "session_data")]
public class SESSION_DATA : ScriptableObject 
{
    public string player_name = null;
    public string level_name = null;
    public float score = 0f;
    public float time = 0f;
}
