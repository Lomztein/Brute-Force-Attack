using UnityEngine;
using System.Collections;

public class MainMenuScene : MonoBehaviour {

	public void Play () {
		Application.LoadLevel ("pv_play");
	}

	public void Build () {
		Application.LoadLevel ("pv_assemblyeditor");
	}

	public void Quit () {
		Application.Quit ();
	}
}
