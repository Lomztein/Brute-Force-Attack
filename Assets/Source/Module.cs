using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using IngameEditors;

public enum Colour { None, Blue, Green, Yellow, Orange, Red, Purple };

public class Module : MonoBehaviour {

	public const string MODULE_FILE_EXTENSION = ".dat";
	public const int MAX_UPGRADE_AMOUNT = 5;
	
	public enum Type { Base, Rotator, Weapon, Structural, Independent };
	public Colour colour;

	public BaseModule parentBase;
	public Module rootModule;

	public string moduleName;
	public string moduleDesc;
	public string assemblyName;

	public Type moduleType;
	public int moduleClass = 2;
	public int moduleCost;
	public int moduleIndex;
	public int parentIndex;
	public int upgradeCount;
	public int upgradeCost;
	public float upgradeMul = 1f;
	
	public int moduleLayer;

	public bool isRoot;
	private List<Module> requestedModules;

	public int saveIndex;
	public int GetModuleIndex () {
		if (!isRoot) {
			Debug.LogWarning ("Tried to get save index from a non-root module D:");
		}

		saveIndex++;
		return saveIndex;
	}

	public virtual void Start () {
		InitializeModule ();
	}

	public static Texture2D CombineSprites (Texture2D[] sprites, Vector3[] positions, int ppu = 16) {
		if (sprites.Length != positions.Length) {
			Debug.LogError ("Spites array not equal length as positions array.");
			return null;
		}
		// First, get total size;
		int maxHeight = 0, maxWidth = 0, minHeight = 0, minWidth = 0, value = 0;
		for (int i = 0; i < sprites.Length; i++) {
			value =  Mathf.RoundToInt ((float)sprites[i].width / 2f + positions[i].x * (float)ppu);
			if (value > maxWidth) {
				maxWidth = value;
			}
			value =  Mathf.RoundToInt (-(float)sprites[i].width / 2f - positions[i].x * (float)ppu);
			if (value < minWidth) {
				minWidth = value;
			}
			value =  Mathf.RoundToInt ((float)sprites[i].height / 2f + positions[i].y * (float)ppu);
			if (value > maxHeight) {
				maxHeight = value;
			}
			value =  Mathf.RoundToInt (-(float)sprites[i].height / 2f - positions[i].y * (float)ppu);
			if (value < minHeight) {
				minHeight = value;
			}
		}

		Texture2D spr = new Texture2D (maxWidth - minWidth, maxHeight - minHeight);
		spr.filterMode = FilterMode.Point;
		spr.wrapMode = TextureWrapMode.Clamp;

		for (int y = 0; y < spr.height; y++) {
			for (int x = 0; x < spr.width; x++) {
				spr.SetPixel (x, y, Color.clear);
			}
		}

		for (int i = 0; i < sprites.Length; i++) {
			OverlaySprite (spr, sprites[i], positions[i]);
		}

		spr.Apply ();
		return spr;
	}

	private static void OverlaySprite (Texture2D tex, Texture2D sprite, Vector3 pos, int ppu = 16) {
		Vector3 center = (Quaternion.Euler (0,0, 90f) * pos) * (float)ppu + new Vector3 (tex.width / 2, tex.height / 2);
		for (int y = -sprite.height / 2; y < sprite.height / 2; y++) {
			for (int x = -sprite.width / 2; x < sprite.width / 2; x++) {
				Color color = sprite.GetPixel (x + sprite.width / 2, y + sprite.height / 2);
				if (color.a > 0.9f) {
					tex.SetPixel ((int)center.x + x, (int)center.y + y, color);
				}
			}
		}
	}
	
	public virtual bool UpgradeModule () {
		if (upgradeCount >= MAX_UPGRADE_AMOUNT - 1) {
			return true;
		}
		upgradeCount++;
		upgradeMul *= 1.2f;
		upgradeCost = Mathf.RoundToInt ((float)upgradeCost * 1.5f);
		if (upgradeCount >= MAX_UPGRADE_AMOUNT - 1) {
			return true;
		}

		return false;
	}

	public static float CalculateUpgradeMul (int upgradeLevel) {
		return 1f * Mathf.Pow (1.2f, (float)upgradeLevel);
	}

	public static int CalculateUpgradeCost (int startCost, int upgradeLevel) {
		float value = 0f;
		while (upgradeLevel > 0) {
			upgradeLevel--;
			value += (float)startCost * 2f * Mathf.Pow (1.5f, (float)upgradeLevel);
		}
		return Mathf.RoundToInt (value);
	}

	public StreamWriter writer;
	
	public void SaveModuleAssembly (string filename) {
		string file = Game.MODULE_ASSEMBLY_SAVE_DIRECTORY + filename + MODULE_FILE_EXTENSION;

		rootModule.writer = File.CreateText (file);
		rootModule.writer.WriteLine ("PROJECT VIRUS MODULE ASSEMBLY FILE, EDIT WITH CAUTION");
		rootModule.writer.WriteLine ("name:" + filename);
		rootModule.BroadcastMessage ("SaveModuleToAssemblyFile", file, SendMessageOptions.RequireReceiver);
		rootModule.writer.WriteLine ("END OF FILE");
		rootModule.writer.Close ();
		rootModule.writer = null;

		PurchaseMenu.cur.InitialzeAssemblyButtons ();
		PurchaseMenu.cur.CloseAssemblyButtons ();
	}
	
	void SaveModuleToAssemblyFile (string file) {
		rootModule.writer.WriteLine ("type:" + moduleName.ToString ());
		if (transform.parent) {
			rootModule.writer.WriteLine ("\tindx:" + moduleIndex.ToString ());
			rootModule.writer.WriteLine ("\tpidx:" + transform.parent.GetComponent<Module> ().moduleIndex.ToString ());
			rootModule.writer.WriteLine ("\tposx:" + (transform.position.x - rootModule.transform.position.x).ToString ());
			rootModule.writer.WriteLine ("\tposy:" + (transform.position.y - rootModule.transform.position.y).ToString ());
		} else {
			rootModule.writer.WriteLine ("\troot");
		}
		rootModule.writer.WriteLine ("\trotz:" + transform.eulerAngles.z.ToString ());
		rootModule.writer.WriteLine ("\tlevl:" + upgradeCount.ToString ());
	}

	void OnMouseDown () {
		if (!PlayerInput.cur.isPlacing && (!ResearchMenu.isOpen || AssemblyEditorScene.isActive)) {
			PlayerInput.cur.focusRoot = this;
			PlayerInput.cur.OpenModuleMenu ();
		}
	}
	
	void InitializeModule () {
		upgradeCost = moduleCost * 2;
		FindParentBase ();
		FindModuleLayer ();
		transform.position = new Vector3 (transform.position.x, transform.position.y, -moduleLayer);

		rootModule = FindRootModule ();
		if (rootModule == this) {
			isRoot = true;
			moduleIndex = 0;
			saveIndex = 0;
		}
		moduleIndex = rootModule.GetModuleIndex ();

		if (isRoot && !AssemblyEditorScene.isActive) Pathfinding.ChangeArea (GetModuleRect (), false);
		if (parentBase) parentBase.GetFastestBulletSpeed ();
		SendMessageUpwards ("OnNewModuleAdded", SendMessageOptions.DontRequireReceiver);
	}

	public void SellModule () {
		if (!AssemblyEditorScene.isActive) Pathfinding.ChangeArea (GetModuleRect (), true);
		Destroy (gameObject);
	}

	public void StockModule () {
		BroadcastMessage ("Stockify");
	}

	void Stockify () {
		if (!AssemblyEditorScene.isActive) Pathfinding.ChangeArea (GetModuleRect (), true);
		Destroy (gameObject);
		PurchaseMenu.AddStock (this);
	}

	public Module FindRootModule () {
		Transform cur = transform;
		while (cur.parent) {
			if (cur.parent.GetComponent<Module>())
				cur = cur.parent;
		}
		return cur.GetComponent<Module>();
	}

	public Module[] GetModuleTree () {
		RequestChildModules ();
		return requestedModules.ToArray ();
	}

	public Rect GetModuleRect () {
		Bounds b = GetComponent<BoxCollider>().bounds;
		return new Rect (transform.position.x - b.size.x / 2f,
		                 transform.position.y - b.size.y / 2f,
		                 b.size.x, b.size.y);
	}

	void FindParentBase () {
		if (transform.parent == null) {
			parentBase = GetComponent<BaseModule>();
		}else{
			Transform cur = transform;
			while (parentBase == null && cur.parent) {
				parentBase = cur.GetComponent<BaseModule>();
				if (!parentBase) cur = cur.parent;
			}
			parentBase = cur.GetComponent<BaseModule>();
		}
	}

	void FindModuleLayer () {
		Transform cur = transform;
		int amount = 0;
		while (cur.parent) {
			amount++;
			cur = cur.parent;
		}
		moduleLayer = amount;
	}

	public static float CalculateTotalPowerRequirements () {
		GameObject[] modules = GameObject.FindGameObjectsWithTag ("Module");
		float power = 0f;
		foreach (GameObject m in modules) {
			Module mod = m.GetComponent<Module>();
			power += mod.moduleClass;
		}

		return power;
	}

	public void RequestChildModules () {
		requestedModules = new List<Module>();
		SendMessageUpwards ("ReturnModuleToRequester", this, SendMessageOptions.DontRequireReceiver);
		BroadcastMessage ("ReturnModuleToRequester", this, SendMessageOptions.DontRequireReceiver);
		requestedModules.Add (this);
	}

	void ReturnModuleToRequester (Module requester) {
		if (requester == this)
			return;

		requester.SendMessage ("OnRecieveModuleFromRequest" ,this, SendMessageOptions.RequireReceiver);
	}

	void OnRecieveModuleFromRequest (Module sender) {
		requestedModules.Add (sender);
	}

	void ResetMaterial () {
		transform.FindChild ("Sprite").GetComponent<SpriteRenderer>().material = PlayerInput.cur.defualtMaterial;
		GetComponent<Collider>().enabled = true;
		enabled = true;
	}

	public virtual void SetIsBeingPlaced () {
		transform.FindChild ("Sprite").GetComponent<SpriteRenderer>().material = PlayerInput.cur.placementMaterial;
		GetComponent<Collider>().enabled = false;
		enabled = false;
	}

	public override string ToString () {
		return "Uh, this is the base module functionality code. Why would you ever want to grap the stats of this?";
	}

}