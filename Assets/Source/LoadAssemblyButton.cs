using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class LoadAssemblyButton : MonoBehaviour {

	public GameObject[] requiredModules;
	public int cost;
	public Assembly assembly;
	public bool isAvailable;
	public string assemblyName;
	public Button button;

    private List<Module> missingModules;
    private bool allModulesResearched;
    private bool affordable;

	public void Initialize () {
		ModuleAssemblyLoader.GetButtonData (assembly, this);
		Texture2D[] sprites = null;
		Vector3[] positions = null;
		ModuleAssemblyLoader.GetSpriteData (assembly, out sprites, out positions);
		button.transform.FindChild ("Image").GetComponent<RawImage>().texture = Module.CombineSprites (sprites, positions);

        OnResearchUnlocked ();
        ButtonUpdate ();
    }

    void OnMouseEnter () {
        assembly.ChangeHighlightRequiredResearch (true);
    }

    void OnMouseExit () {
        assembly.ChangeHighlightRequiredResearch (false);
    }

    public void ButtonUpdate () {
        if (allModulesResearched) {
            if (cost <= Game.credits) {
                button.interactable = true;
		        button.GetComponent<HoverContextElement> ().text = assemblyName + ", " + cost.ToString () + " LoC";
            } else {
                button.interactable = false;
                button.GetComponent<HoverContextElement> ().text = assemblyName + ", " + cost.ToString () + " LoC - INSUFFICIENT FUNDS";
            }
        } else {
            string required = "";
            for (int i = 0; i < missingModules.Count; i++) {
                required += "\n\t" + missingModules[i].moduleName;
            }
            button.interactable = false;
            button.GetComponent<HoverContextElement> ().text = assemblyName + " - RESEARCH NEEDED"
                + required;
        }
    }

	public void OnResearchUnlocked () {
        missingModules = new List<Module> ();
		bool allGood = true;
		for (int i = 0; i < requiredModules.Length; i++) {
			if (!PurchaseMenu.cur.standard.Contains (requiredModules[i])) {
                missingModules.Add (requiredModules[i].GetComponent<Module>());
				allGood = false;
			}
		}

        allModulesResearched = allGood;
        ButtonUpdate ();
	}

	public void Purchase () {
		PurchaseMenu.cur.LoadAssembly (assembly);
	}
}
