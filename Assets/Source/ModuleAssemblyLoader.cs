using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ModuleAssemblyLoader : MonoBehaviour {

	public string file;
	public List<GameObject> moduleObjects;

	public static bool GetButtonData (string path, LoadAssemblyButton button) {
		string[] contents = GetContents (path);
		List<GameObject> objects = new List<GameObject>();
		int cost = 0;
		for (int i = 0; i < contents.Length; i++) {
			if (contents[i].Substring (0, 5) == "type:") {
				GameObject module = PurchaseMenu.cur.GetModulePrefab (contents[i].Substring (5));
				if (module) {
					objects.Add (module);
					cost += module.GetComponent<Module>().moduleCost;
				}else{
					return false;
				}
			}

			if (contents[i].Substring (0, 5) == "name:")
				button.assemblyName = contents[i].Substring (5);
		}
		button.requiredModules = objects.ToArray ();
		button.cost = cost;
		return true;
	}

	public void LoadAssembly (string path) {
		if (File.Exists (path)) {
		
			file = path;

			string[] contents = GetContents (path);
			GameObject modulePrefab = null;
			GameObject rootModule = null;
			Module module = null;
			int totalCost = 0;
			
			// Instantiate, give position and indicies.
			for (int i = 0; i < contents.Length; i++) {
				
				if (contents[i].Substring (0, 5) == "type:") {
					modulePrefab = PurchaseMenu.cur.GetModulePrefab (contents[i].Substring (5));
					if (modulePrefab) {
						moduleObjects.Add ((GameObject)Instantiate (modulePrefab, Vector3.zero, Quaternion.identity));
						//moduleObjects[moduleObjects.Count - 1].SetActive (false);
						moduleObjects[moduleObjects.Count - 1].transform.parent = transform;
						module = moduleObjects[moduleObjects.Count - 1].GetComponent<Module>();
						module.enabled = false;
						totalCost += module.moduleCost;
					}else{
						Debug.LogWarning ("Tried to load a non-existing or non-researched module");
						return;
					}
				}

				if (modulePrefab) {

					if (contents[i] == "\troot") {
						rootModule = module.gameObject;
						module.moduleIndex = 1;
					}

					if (contents[i].Substring (0,5) == "\tindx") {
						module.moduleIndex = int.Parse (contents[i].Substring (6));
					}

					if (contents[i].Substring (0,5) == "\tpidx") {
						module.parentIndex = int.Parse (contents[i].Substring (6));
					}

					if (contents[i].Substring (0,5) == "\tposx") {
						module.transform.localPosition = new Vector3 (float.Parse (contents[i].Substring (6)),
						                                              float.Parse (contents[i + 1].Substring (6)));
					}

					if (contents[i].Substring (0,5) == "\trotz") {
						module.transform.eulerAngles = new Vector3 (0,0, float.Parse (contents[i].Substring (6)));
					}
				}
			}

			// Set parents
			foreach (GameObject obj in moduleObjects) {
				Module m = obj.GetComponent<Module>();
				if (m.parentIndex != 0) {
					m.transform.parent = FindModuleFromIndex (m.parentIndex).transform;
					m.transform.position = new Vector3 (m.transform.position.x,
					                                    m.transform.position.y,
					                                    m.transform.parent.position.z - 1);
				}

			}

			// Activate gameObject, but disable module components
			PlayerInput.cur.SelectPurchaseable (rootModule, true);
			PlayerInput.cur.SetPurchaseableFromSceneObject (PlayerInput.cur.placementParent.GetChild (0).gameObject);
			PlayerInput.cur.placementParent.GetChild (0).transform.eulerAngles -= new Vector3 (0,0, 90);
		}else{
			Debug.LogWarning ("File not found at " + file);
		}
	}

	Module FindModuleFromIndex (int index) {
		foreach (GameObject obj in moduleObjects) {
			Module m = obj.GetComponent<Module>();
			if (m.moduleIndex == index)
				return m;
		}

		return null;
	}

	public static string[] GetContents (string file) {
		StreamReader reader = File.OpenText (file);
		List<string> con = new List<string>();

		while (true) {
			string loc = reader.ReadLine ();
			if (loc == "END OF FILE") {
				break;
			}else{
				con.Add (loc);
			}
		}

		return con.ToArray ();
	}
}
