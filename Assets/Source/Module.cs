using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Colour { None, Blue, Green, Yellow, Orange, Red, Purple };

public class Module : MonoBehaviour {

	public enum Type { Base, Rotator, Weapon, Structural, Independent };
	public Colour colour;

	public BaseModule parentBase;

	public string moduleName;
	public string moduleDesc;

	public Type moduleType;
	public int moduleClass = 2;
	public int moduleCost;

	public int moduleLayer;

	public Upgrade[] upgrades;
	public bool isRoot;
	private List<Module> requestedModules;

	public void Start () {
		InitializeModule ();
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
		if (FindRootModule () == this) isRoot = true;
		if (isRoot) Dijkstra.ChangeArea (GetModuleRect (), false);
		SendMessageUpwards ("OnNewModuleAdded", SendMessageOptions.DontRequireReceiver);
	}

	public void SellModule () {
		Dijkstra.ChangeArea (GetModuleRect (), true);
		Destroy (gameObject);
	}

	public Module FindRootModule () {
		Transform cur = transform;
		while (cur.parent) {
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
}
























