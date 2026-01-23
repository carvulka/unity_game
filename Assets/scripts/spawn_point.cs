using UnityEngine;

public class SPAWN_POINT : MonoBehaviour
{
    public enum TYPE
    {
        item,
        prop,
    }
    [SerializeField] public SPAWN_POINT.TYPE type;
    [SerializeField] public int pool_id;
}
