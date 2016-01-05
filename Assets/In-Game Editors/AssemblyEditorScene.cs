using UnityEngine;
using System.Collections;
using IngameEditors;
using System.Linq;

namespace IngameEditors {

	public class AssemblyEditorScene : MonoBehaviour {

		public PurchaseMenu purchaseMenu;
		public float zoomSpeed;

		// Use this for initialization
		void Start () {
			Game.InitializeSaveDictionaries ();
			ResearchMenu.InitializeAllStatics ();
			Game.currentScene = Scene.AssemblyBuilder;
			Game.credits = int.MaxValue;
			purchaseMenu.standard = Resources.LoadAll<GameObject> ("Modules").ToList ();
			purchaseMenu.Initialize ();
		}

		public void QuitScene () {
			Game.currentScene = Scene.Play;
			Application.LoadLevel ("pv_menu");
		}

		void Update () {
			Camera.main.orthographicSize = Mathf.Lerp (Camera.main.orthographicSize, 
			                                           Camera.main.orthographicSize + zoomSpeed * Input.GetAxis ("Scroll Wheel") * Time.deltaTime,
			                                           -zoomSpeed * Time.deltaTime);
		}
	}
}