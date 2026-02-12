using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SPAWN_POINT : MonoBehaviour
{
    [System.Serializable]
    public class POOL_ENTRY
    {
        public GameObject prefab;
        public int weight;
        public float mass;
    }

    [SerializeField] int empty_weight;
    [SerializeField] List<POOL_ENTRY> pool;

    void Start()
    {
        if (this.pool == null || this.pool.Count == 0) return;
        
        int total_weight = this.empty_weight + this.pool.Sum(e => e.weight);
        int random = Random.Range(0, total_weight);
        int cumulative = 0;
        foreach (POOL_ENTRY pool_entry in this.pool)
        {
            cumulative += pool_entry.weight;
            if (random < cumulative)
            {
                GameObject prop_object = Instantiate(pool_entry.prefab, this.transform.position, this.transform.rotation);
                prop_object.AddComponent<Rigidbody>().mass = pool_entry.mass;
                return;
            }
        }
    }
}
