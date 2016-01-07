using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Assembly : ScriptableObject {

    public Part[] parts;
    public Sprite sprite;
    public Button[] requiredResearch;

    [System.Serializable]
    public class Part {
        public bool isRoot;
        public int index;
        public int parentIndex;
        public Vector2 position;
        public float angle;
    }
}
