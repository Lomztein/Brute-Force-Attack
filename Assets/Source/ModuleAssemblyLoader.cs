using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ModuleAssemblyLoader : MonoBehaviour {

	public string file;
	public List<GameObject> moduleObjects;

	/// <summary>
	/// Loads modules assembly from path 'path'.
	/// </summary>
	/// <param name="path">Load path</param>
	public void LoadAssembly (string path) {

		string[] contents = GetContents (path);
		GameObject modulePrefab = null;

		// Instantiate, give position and indicies.
		for (int i = 0; i < contents.Length; i++) {
			if (contents[i] == "type:") {
				modulePrefab = PurchaseMenu.cur.GetModulePrefab (contents[i].Substring (5));
				continue;
			}

			if (modulePrefab) {
				moduleObjects.Add ((GameObject)Instantiate (modulePrefab));
				moduleObjects[moduleObjects.Count - 1].transform.parent = transform;
				Module module = moduleObjects[moduleObjects.Count - 1].GetComponent<Module>();

				if (contents[i] == "\troot")
					continue;

				if (contents[i] == "\tindx") {
					module.moduleIndex = int.Parse (contents[i].Substring (6));
				}

				if (contents[i] == "\tpidx") {
					module.parentIndex = int.Parse (contents[i].Substring (6));
				}

				if (contents[i] == "\tposx") {
					module.transform.localPosition = new Vector3 (float.Parse (contents[i].Substring (6)),
					                                              float.Parse (contents[i + 1].Substring (6)));
				}
			}
		}

		foreach (GameObject obj in moduleObjects) {
			Module m = obj.GetComponent<Module>();
			if (m.parentIndex != 0) {
				m.transform.parent = FindModuleFromIndex (m.parentIndex).transform;
				m.transform.position = new Vector3 (m.transform.position.x,
				                                    m.transform.position.y,
				                                    m.transform.parent.position.z - 1);
			}

			m.Start ();
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

	string[] GetContents (string file) {
		StreamReader reader = File.OpenText (file);
		List<string> con = new List<string>();

		while (true) {
			if (reader.ReadLine () == "END OF FILE") {
				break;
			}else{
				con.Add (reader.ReadLine ());
			}
		}

		return con.ToArray ();
	}
}
