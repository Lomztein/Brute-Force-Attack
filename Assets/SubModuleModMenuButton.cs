using UnityEngine;
using System.Collections;

public class SubModuleModMenuButton : MonoBehaviour {

    public Module module;
    // What even is this class name?

	void OnMouseEnterElement () {
        PlayerInput.cur.hoverMarker.transform.position = module.transform.position;
        PlayerInput.cur.hoverMarker.transform.localScale = Vector3.one * module.moduleClass;
    }

    void OnMouseExitElement () {
        PlayerInput.cur.hoverMarker.transform.position = Vector3.right * 10000;
    }
}
