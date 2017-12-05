using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ModuleAssemblyLoader : MonoBehaviour {

	public string file;
	public List<GameObject> moduleObjects;

	public static void GetSpriteData (Assembly assembly, out Texture2D[] sprites, out Vector3[] positions) {
		List<Texture2D> spr = new List<Texture2D> ();
		List<Vector3> pos = new List<Vector3> ();

		for (int i = 0; i < assembly.parts.Count; i++) {
			Assembly.Part part = assembly.parts[i];
			spr.Add (PurchaseMenu.cur.GetModulePrefab (part.type)
				.transform.Find ("Sprite").GetComponent<SpriteRenderer>().sprite.texture);

			pos.Add (new Vector3 (part.x, part.y));
		}

		sprites = spr.ToArray ();
		positions = pos.ToArray ();
	}

	public static void GetButtonData (Assembly assembly, LoadAssemblyButton button) {
		List<GameObject> objects = new List<GameObject>();
		int cost = 0;
		GameObject module = null;
		for (int i = 0; i < assembly.parts.Count; i++) {
			module = PurchaseMenu.cur.GetModulePrefab (assembly.parts[i].type);
			if (module) {
				objects.Add (module);
				cost += module.GetComponent<Module>().moduleCost;
			}

			button.assemblyName = assembly.assemblyName;
		}
		button.requiredModules = objects.ToArray ();
		button.cost = cost;
	}

    public GameObject LoadAssembly (Assembly assembly, bool directToWorld = false) {
		GameObject modulePrefab = null;
		GameObject rootModule = null;
		Module module = null;
		int totalCost = 0;

		// Instantiate, give position and indicies.
		for (int i = 0; i < assembly.parts.Count; i++) {

			Assembly.Part part = assembly.parts[i];
				
			modulePrefab = PurchaseMenu.cur.GetModulePrefab (part.type);
			if (modulePrefab) {
				moduleObjects.Add ((GameObject)Instantiate (modulePrefab, Vector3.zero, Quaternion.identity));
				//moduleObjects[moduleObjects.Count - 1].SetActive (false);
				moduleObjects[moduleObjects.Count - 1].transform.parent = transform;
				module = moduleObjects[moduleObjects.Count - 1].GetComponent<Module>();
				module.enabled = false;
				totalCost += module.moduleCost;
			}else{
				Debug.LogWarning ("Tried to load a non-existing.");
				return null;
			}

			if (modulePrefab) {

				if (part.isRoot) {
					rootModule = module.gameObject;
					module.moduleIndex = 1;

                    module.assemblyName = assembly.assemblyName;
                    module.assemblyDesc = assembly.assemblyDesc;
                    module.assembly = assembly;

                    HoverContextElement el = module.gameObject.AddComponent<HoverContextElement> ();
                    el.isWorldElement = true;
                }

                module.moduleIndex = part.index;
				module.parentIndex = part.parentIndex;

				module.transform.localPosition = new Vector3 (part.x, part.y);
				module.transform.eulerAngles = new Vector3 (0,0, part.angle);

                //rootModule.GetComponent<Module> ().modules.Add (module);
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
            rootModule.GetComponent<Module> ().modules.Add (m);
        }

        // Activate gameObject, but disable module components
        if (!directToWorld) {
            PlayerInput.cur.SelectPurchaseable (rootModule, true);
            PlayerInput.cur.SetPurchaseableFromSceneObject (PlayerInput.cur.placementParent.GetChild (0).gameObject);
            PlayerInput.cur.placementParent.GetChild (0).transform.eulerAngles -= new Vector3 (0, 0, 90);
            PlayerInput.cur.currentCost = totalCost;
        } else {
            foreach (GameObject obj in moduleObjects) {
                obj.GetComponent<Module> ().enabled = true;
            }
        }

        return rootModule;
    }

    public static void ConvertLegacyAssemblyFiles () {
		string[] files = Directory.GetFiles (Game.MODULE_ASSEMBLY_SAVE_DIRECTORY, "*" + Module.MODULE_FILE_EXTENSION);

		for (int i = 0; i < files.Length; i++) {

			string[] contents = GetContents (files[i]);

			if (!contents[0].Contains ("PROJECT VIRUS MODULE ASSEMBLY FILE, EDIT WITH CAUTION"))
				continue;

			Assembly newAssembly = new Assembly ();

			Assembly.Part part = null;

			for (int j = 0; j < contents.Length; j++) {

				if (contents[j].Substring (0,5) == "name:") {
					newAssembly.assemblyName = contents[j].Substring (5);
				}

				if (contents[j].Substring (0, 5) == "type:") {
					part = new Assembly.Part ();
					part.type = contents[j].Substring (5);
				}

				if (part != null) {
					
					if (contents[j] == "\troot") {
						part.isRoot = true;
						part.index = 1;
					}
					
					if (contents[j].Substring (0,5) == "\tindx") {
						part.index = int.Parse (contents[j].Substring (6));
					}
					
					if (contents[j].Substring (0,5) == "\tpidx") {
						part.parentIndex = int.Parse (contents[j].Substring (6));
					}
					
					if (contents[j].Substring (0,5) == "\tposx") {
						part.x = float.Parse (contents[j].Substring (6));
						part.y = float.Parse (contents[j + 1].Substring (6));
					}
					
					if (contents[j].Substring (0,5) == "\trotz") {
						part.angle = float.Parse (contents[j].Substring (6));
						newAssembly.parts.Add (part);
					}
				}
			}
		
			Assembly.SaveToFile (newAssembly.assemblyName + " (NEW)", newAssembly);
		}
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

	Module FindModuleFromIndex (int index) {
		foreach (GameObject obj in moduleObjects) {
			Module m = obj.GetComponent<Module>();
			if (m.moduleIndex == index)
				return m;
		}

		return null;
	}
}
