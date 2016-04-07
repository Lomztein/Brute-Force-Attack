using UnityEngine;
using System.Collections;

public class DataCarrier : MonoBehaviour {

    // Hang on, do I even need this?
    public static DataCarrier cur;

    void Awake () {
        DontDestroyOnLoad (gameObject);
        if (cur && cur != this)
            Destroy (this);
        else
            cur = this;
    }
}
