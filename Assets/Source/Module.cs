using UnityEngine;
using System.Collections;

public class Module : MonoBehaviour {

	public enum Type { Base, Rotator, Weapon, Structural, Independent };

	public BaseModule parentBase;

	public string moduleName;
	public string moduleDesc;

	public Type moduleType;
	public int moduleClass = 2;

	public int moduleLayer;

	public Upgrade[] upgrades;
	public bool isRoot;

	void Start () {
		InitializeModule ();
		StartModule ();
	}

	void Update () {
		UpdateModule ();
	}

	public virtual void StartModule () {
		// Use this a replacement for the Start function, so that
		// We don't get functions mixed up.
	}

	public virtual void UpdateModule () {
		// Like the StartModule function
	}

	void OnMouseDown () {
		PlayerInput.cur.focusRoot = FindRootModule ();
		PlayerInput.cur.OpenTowerMenu ();
	}

	void InitializeModule () {
		FindParentBase ();
		FindModuleLayer ();
		transform.position = new Vector3 (transform.position.x, transform.position.y, -moduleLayer);
		if (FindRootModule () == this) isRoot = true;
		if (isRoot) Dijkstra.ChangeArea (GetModuleRect (), false);
	}

	public Module FindRootModule () {
		Transform cur = transform;
		while (cur.parent) {
			cur = cur.parent;
		}
		return cur.GetComponent<Module>();
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
}