using UnityEngine;
using System.Collections;

public class LoadAssemblyButton : MonoBehaviour {

	public GameObject[] requiredModules;
	public int cost;
	public string path;
	public bool isAvailable;
	public string assemblyName;

	public void OnResearchUnlocked () {
		isAvailable = ModuleAssemblyLoader.GetButtonData (path, this);
	}

	public void Purchase () {
		PurchaseMenu.cur.LoadAssembly (path);
	}
}
