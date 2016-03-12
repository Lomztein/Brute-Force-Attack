using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenuScene : MonoBehaviour {

	public void Play () {
        SceneManager.LoadScene ("pv_play");
	}

	public void Build () {
        SceneManager.LoadScene ("pv_assemblybuilder");
	}

	public void Quit () {
		Application.Quit ();
	}
}
