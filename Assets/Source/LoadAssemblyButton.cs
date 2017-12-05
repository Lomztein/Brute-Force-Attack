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
		button.transform.Find ("Image").GetComponent<RawImage>().texture = Module.CombineSprites (sprites, positions);

        OnResearchUnlocked ();
        ButtonUpdate ();
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
            if (assembly.assemblyDesc != "") {
                button.GetComponent<HoverContextElement> ().text += "\n" + Utility.ConstructDescriptionText (assembly.assemblyDesc, (int)(assembly.assemblyName.Length * 1.5f));
            }
        } else {
            string required = "";
            for (int i = 0; i < missingModules.Count; i++) {
                required += "\n\t" + missingModules[i].moduleName;
            }
            button.interactable = false;
            button.GetComponent<HoverContextElement> ().text = assemblyName + ", " + cost.ToString () +  " LoC - RESEARCH NEEDED"
                + required;
        }
    }

	public void OnResearchUnlocked () {
        missingModules = new List<Module> ();
		bool allGood = true;
		for (int i = 0; i < requiredModules.Length; i++) {
            Module m = requiredModules[i].GetComponent<Module> ();
            if (!PurchaseMenu.cur.standard.Contains (requiredModules[i]) && !missingModules.Contains (m)) {
                missingModules.Add (m);
				allGood = false;
			}
		}

        allModulesResearched = allGood;
        ButtonUpdate ();
	}

    void OnMouseDownElement () {
        if (!button.interactable) {
            assembly.ChangeHighlightRequiredResearch (true);
            Game.game.researchMenu.ToggleResearchMenu ();
        }
    }

    public void Purchase () {
		PurchaseMenu.cur.LoadAssembly (assembly);
	}
}
