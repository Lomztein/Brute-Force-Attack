using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public enum Colour { None, Blue, Green, Yellow, Orange, Red, Purple };

public class Module : MonoBehaviour {

	public const string MODULE_FILE_EXTENSION = ".module";
	
	public enum Type { Base, Rotator, Weapon, Structural, Independent };
	public Colour colour;

	public BaseModule parentBase;
	public Module rootModule;

	public string moduleName;
	public string moduleDesc;

	public Type moduleType;
	public int moduleClass = 2;
	public int moduleCost;
	public int moduleIndex;
	public int parentIndex;

	public int moduleLayer;

	public Upgrade[] upgrades;
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

	public void Start () {
		InitializeModule ();
	}

	public StreamWriter writer;
	public int assemblyCost;
	
	public void SaveModuleAssembly (string filename) {
		string file = Game.MODULE_ASSEMBLY_SAVE_DIRECTORY + filename + MODULE_FILE_EXTENSION;

		rootModule.writer = File.CreateText (file);
		rootModule.writer.WriteLine ("PROJECT VIRUS MODULE FILE, EDIT WITH CAUTION");
		rootModule.writer.WriteLine ("name:" + filename);
		rootModule.BroadcastMessage ("SaveModuleToAssemblyFile", file, SendMessageOptions.RequireReceiver);
		rootModule.writer.WriteLine ("cost:" + assemblyCost);
		rootModule.writer.WriteLine ("END OF FILE");
		rootModule.writer.WriteLine ("DO NOT REMOVE 'END OF FILE' NOTICE, IT'LL CRASH THE GAME");
		rootModule.writer.WriteLine ("Ugh, this feels too professional.. *fartnoises*");
		rootModule.writer.Close ();
		rootModule.writer = null;
		assemblyCost = 0;

		PurchaseMenu.cur.InitialzeAssemblyButtons ();
		PurchaseMenu.cur.CloseAssemblyButtons ();
	}
	
	void SaveModuleToAssemblyFile (string file) {
		rootModule.writer.WriteLine ("type:" + moduleName.ToString ());
		if (transform.parent) {
			rootModule.writer.WriteLine ("\tindx:" + moduleIndex.ToString ());
			rootModule.writer.WriteLine ("\tpidx:" + transform.parent.GetComponent<Module>().moduleIndex.ToString ());
			rootModule.writer.WriteLine ("\tposx:" + transform.localPosition.x.ToString ());
			rootModule.writer.WriteLine ("\tposy:" + transform.localPosition.y.ToString ());
			rootModule.writer.WriteLine ("\trotz:" + transform.eulerAngles.z.ToString ());
		}else{
			rootModule.writer.WriteLine ("\troot");
		}
		rootModule.assemblyCost += moduleCost;
	}

	void OnMouseDown () {
		if (!PlayerInput.cur.isPlacing && !ResearchMenu.isOpen) {
			PlayerInput.cur.focusRoot = this;
			PlayerInput.cur.OpenModuleMenu ();
		}
	}
	
	void InitializeModule () {
		FindParentBase ();
		FindModuleLayer ();
		transform.position = new Vector3 (transform.position.x, transform.position.y, -moduleLayer);
		Game.CalculatePowerLevel ();

		rootModule = FindRootModule ();
		if (rootModule == this) {
			isRoot = true;
			moduleIndex = 0;
			saveIndex = 0;
		}
		moduleIndex = rootModule.GetModuleIndex ();

		if (isRoot) Dijkstra.ChangeArea (GetModuleRect (), false);
		SendMessageUpwards ("OnNewModuleAdded", SendMessageOptions.DontRequireReceiver);
	}

	public void SellModule () {
		Dijkstra.ChangeArea (GetModuleRect (), true);
		Destroy (gameObject);
	}

	public void StockModule () {
		BroadcastMessage ("Stockify");
	}

	void Stockify () {
		Dijkstra.ChangeArea (GetModuleRect (), true);
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

	void SetIsBeingPlaced () {
		transform.FindChild ("Sprite").GetComponent<SpriteRenderer>().material = PlayerInput.cur.placementMaterial;
		GetComponent<Collider>().enabled = false;
		enabled = false;
	}

}
























