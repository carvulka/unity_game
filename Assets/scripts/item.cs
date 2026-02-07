using UnityEngine;
using System.Collections.Generic;

public class ITEM : MonoBehaviour
{
    public class TYPE : ScriptableObject
    {
        public GameObject prefab;
        public Sprite sprite;
        public string description;
        public float score;
        public float mass;
        public int target_id;
    }

    public class STATE
    {
        public float multiplier;
    }

    public TYPE type;
    public STATE state;
}
