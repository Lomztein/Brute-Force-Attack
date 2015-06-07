using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ModuleAssemblyLoader : MonoBehaviour {

	public string file;
	public List<GameObject> moduleObjects;

	public static void GetButtonSprites (LoadAssemblyButton button, out Texture2D[] sprites, out Vector3[] positions) {
		List<Texture2D> spr = new List<Texture2D> ();
		List<Vector3> pos = new List<Vector3> ();
		string[] contents = GetContents (button.path);

		pos.Add (Vector3.zero);
		for (int i = 0; i < contents.Length; i++) {
			if (contents[i].Substring (0, 5) == "type:") {
				spr.Add (PurchaseMenu.cur.GetModulePrefab (contents[i].Substring (5))
				         .transform.FindChild ("Sprite").GetComponent<SpriteRenderer>().sprite.texture);
			}

			if (contents[i].Substring (0,5) == "\tposx") {
				pos.Add (new Vector3 (float.Parse (contents[i].Substring (6)),
				                                              float.Parse (contents[i + 1].Substring (6))));
			}
		}

		sprites = spr.ToArray ();
		positions = pos.ToArray ();
	}

	public static void GetButtonData (string path, LoadAssemblyButton button) {
		string[] contents = GetContents (path);
		List<GameObject> objects = new List<GameObject>();
		int cost = 0;
		GameObject module = null;
		for (int i = 0; i < contents.Length; i++) {
			if (contents[i].Substring (0, 5) == "type:") {
				module = PurchaseMenu.cur.GetModulePrefab (contents[i].Substring (5));
				if (module) {
					objects.Add (module);
					cost += module.GetComponent<Module>().moduleCost;
				}
			}

			if (contents[i].Substring (0,5) == "\tlevl") {
				cost += Module.CalculateUpgradeCost (module.GetComponent<Module>().moduleCost, int.Parse (contents[i].Substring (6)));
			}			

			if (contents[i].Substring (0, 5) == "name:")
				button.assemblyName = contents[i].Substring (5);
		}
		button.requiredModules = objects.ToArray ();
		button.cost = cost;
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

					if (contents[i].Substring (0,5) == "\tlevl") {
						int level = int.Parse (contents[i].Substring (6));
						module.upgradeCount = level;
						module.upgradeMul = Module.CalculateUpgradeMul (level);
						totalCost += Module.CalculateUpgradeCost (module.moduleCost, level);
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
			PlayerInput.cur.currentCost = totalCost;
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
		int maxTries = short.MaxValue;

		while (true && maxTries > 0) {
			maxTries--;
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
