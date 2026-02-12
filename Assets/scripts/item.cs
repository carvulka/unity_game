using UnityEngine;
using System.Collections.Generic;

public class ITEM : MonoBehaviour
{
    public class TYPE : ScriptableObject
    {
        public GameObject prefab;
        public Sprite sprite;
        public string description;
        public float mass;
        public int category_id;
        public float score;
    }

    public class STATE
    {
        public float multiplier;
    }

    public TYPE type;
    public STATE state;
}
