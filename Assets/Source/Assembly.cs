using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class Assembly {

	public string assemblyName;
	public string assemblyDesc;

	public List<Part> parts = new List<Part>();

	[System.NonSerialized]
    public Texture2D texture;

	[System.NonSerialized]
    public Research[] requiredResearch = new Research[0];

    [System.Serializable]
    public class Part {
        public bool isRoot;
		public string type;
        public int index;
        public int parentIndex;
        public float x;
		public float y;
        public float angle;

		public Part (bool _isRoot, string _type, int _index, int _parentIndex, float _x , float _y, float _angle) {
			isRoot = _isRoot;
			type = _type;
			index = _index;
			parentIndex = _parentIndex;
			x = _x;
			y = _y;
			angle = _angle;
		}

		public Part () {
		}
    }

	public static Assembly LoadFromFile (string fileName, bool isFullPath = false) {

		if (!isFullPath) fileName = Game.MODULE_ASSEMBLY_SAVE_DIRECTORY + fileName + Module.MODULE_FILE_EXTENSION;

        Assembly data = Utility.LoadObjectFromFile<Assembly> (fileName);

		Texture2D[] sprites = null;
		Vector3[] positions = null;

		ModuleAssemblyLoader.GetSpriteData (data, out sprites, out positions);
		data.texture = Module.CombineSprites (sprites, positions);

        List<Research> hls = new List<Research> ();
        if (Game.currentScene == Scene.Play) {
            for (int j = 0; j < data.parts.Count; j++) {

                for (int i = 0; i < ResearchMenu.cur.research.Count; i++) {
                    Research r = ResearchMenu.cur.research[i];
                    if (r.func == "UnlockModule") {
                        Module mod = ResearchMenu.cur.unlockableModules[int.Parse (r.meta)].GetComponent<Module> ();

                        if (mod.moduleName == data.parts[j].type && !hls.Contains (r)) {
                            hls.Add (r);
                        }
                    }
                }
            }
        }

        data.requiredResearch = hls.ToArray ();
		return data;
	}

    public void ChangeHighlightRequiredResearch (bool highlight) {
        if (requiredResearch != null)
            for (int i = 0; i < requiredResearch.Length; i++) {
                Research r = requiredResearch[i];
                if (r.highlighter)
                    r.highlighter.gameObject.SetActive (highlight);
            }
    }

	public static void SaveToFile (string fileName, Assembly assembly) {
        Utility.SaveObjectToFile (Game.MODULE_ASSEMBLY_SAVE_DIRECTORY + fileName + Module.MODULE_FILE_EXTENSION, assembly);
	}

    public Texture2D GetSprite () {
        List<Texture2D> sprites = new List<Texture2D>();
        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < parts.Count; i++) {

            GameObject m = PurchaseMenu.cur.GetModulePrefab(parts[i].type);
            sprites.Add(m.GetComponentInChildren<SpriteRenderer>().sprite.texture);
            positions.Add(new Vector3(parts[i].x, parts[i].y));

        }

        return Module.CombineSprites(sprites.ToArray(), positions.ToArray());
    }

    public static void GetAssemblyDescData(Assembly assembly, out int cost, out int rootClass, out int techLevel, out int dps) {
        rootClass = PurchaseMenu.cur.GetModulePrefab (assembly.parts [ 0 ].type).GetComponent<Module> ().moduleClass;
        cost = 0;
        techLevel = 0;
        dps = 0;

        for (int i = 0; i < assembly.parts.Count; i++) {
            GameObject mod = PurchaseMenu.cur.GetModulePrefab (assembly.parts [ i ].type);
            cost += mod.GetComponent<Module> ().moduleCost;
            for (int j = 0; j < ResearchMenu.cur.research.Count; j++) {
                Research research = ResearchMenu.cur.research [ j ];

                if (research.func == "UnlockModule") {
                    if (mod == ResearchMenu.cur.unlockableModules [ int.Parse (research.meta) ] && techLevel < research.y)
                        techLevel = research.y;
                }
            }

            WeaponModule wep = mod.GetComponent<WeaponModule> ();
            if (wep)
                dps += Mathf.RoundToInt (wep.weapon.GetDPS ());
        }
    }

    public class Comparer : IComparer<Assembly> {
        public int Compare(Assembly x, Assembly y) {
            int cost1, rootClass1, techLevel1, dps1;
            int cost2, rootClass2, techLevel2, dps2;

            GetAssemblyDescData (x, out cost1, out rootClass1, out techLevel1, out dps1);
            GetAssemblyDescData (y, out cost2, out rootClass2, out techLevel2, out dps2);

            return Mathf.Clamp (cost1 - cost2, -1, 1);
        }
    }
}
