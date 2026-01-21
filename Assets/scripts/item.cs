using UnityEngine;
using System.Collections.Generic;

public class ITEM : MonoBehaviour
{
    public class TYPE : ScriptableObject
    {
        public GameObject prefab;
        public Sprite sprite;
        public string description;
        public List<ITEM.TYPE.BIN> bins = new List<ITEM.TYPE.BIN>();

        public class BIN
        {
            public int id;
            public float score;
        }
    }

    public class STATE
    {
        public float multiplier;
    }

    public TYPE type;
    public STATE state;
}
