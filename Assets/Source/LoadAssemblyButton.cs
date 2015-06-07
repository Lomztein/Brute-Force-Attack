using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoadAssemblyButton : MonoBehaviour {

	public GameObject[] requiredModules;
	public int cost;
	public string path;
	public bool isAvailable;
	public string assemblyName;
	public Button button;

	public void Initialize () {
		ModuleAssemblyLoader.GetButtonData (path, this);
		Texture2D[] sprites = null;
		Vector3[] positions = null;
		ModuleAssemblyLoader.GetButtonSprites (this, out sprites, out positions);
		button.transform.FindChild ("Image").GetComponent<RawImage>().texture = Module.CombineSprites (sprites, positions);

		button.GetComponent<HoverContextElement> ().text = cost.ToString () + " LoC";
	}

	public void OnResearchUnlocked () {
		bool allGood = true;
		for (int i = 0; i < requiredModules.Length; i++) {
			if (!PurchaseMenu.cur.standard.Contains (requiredModules[i])) {
				allGood = false;
			}
		}

		if (cost > Game.credits) {
			allGood = false;
		}

		button.interactable = allGood;
	}

	public void Purchase () {
		PurchaseMenu.cur.LoadAssembly (path);
	}
}
