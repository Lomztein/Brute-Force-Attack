using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour {

	public float cameraMovementSpeed;
	public LayerMask turretLayer;

	public PurchaseMenu purchaseMenu;

	public SpriteRenderer placementSprite;
	private GameObject purchaseModule;
	private Module pModule;
	private bool isPlacing;
	private bool isRotting;

	private Vector3 pos;
	private Vector3 placePos;
	private Quaternion placeRot;

	private Module hitModule;

	private float camDepth;
	private float ang;

	public Module focusRoot;
	public TowerContextMenu contextMenu;

	public Vector3[] canPlaceTestPos;

	public static PlayerInput cur;

	void Start () {
		cur = this;
		camDepth = Camera.main.transform.position.z;
	}

	public void SelectPurchaseable (GameObject purModule) {
		placementSprite.enabled = true;
		placementSprite.sprite = purModule.transform.FindChild ("Sprite").GetComponent<SpriteRenderer>().sprite;
		purchaseModule = purModule;
		pModule = purModule.GetComponent<Module>();
		isPlacing = true;
	}

	void CancelPurchase () {
		isPlacing = false;
		purchaseModule = null;
		pModule = null;
		isRotting = false;
		placementSprite.enabled = false;
	}

	public void OpenTowerMenu () {
		contextMenu.gameObject.SetActive (true);
		contextMenu.OpenModule (focusRoot);
	}
	
	// Update is called once per frame
	void Update () {
		MoveCamera ();

		if (isPlacing && !EnemySpawn.waveStarted) {

			// Grap mouse position, and round it.
			pos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			pos = new Vector3 (Mathf.Round (pos.x/1f - Module.offsets[pModule.moduleClass-1]) * 1, Mathf.Round (pos.y/1f - Module.offsets[pModule.moduleClass-1]) * 1, pos.z);

			if (!isRotting) {
				placePos = new Vector3 (pos.x + Module.offsets[pModule.moduleClass-1], pos.y + Module.offsets[pModule.moduleClass-1], 0f);
				placeRot = Quaternion.Euler (0,0,90f);
			}

			UpdatePlacementSprite ();
			GetHitModule ();

			if (hitModule)
				placeRot = hitModule.transform.rotation;

			if (isRotting) {
				ang = Mathf.RoundToInt (Angle.CalculateAngle (placePos, pos) / 45f) * 45;
					if (Input.GetButton ("LCtrl"))
						placeRot = Quaternion.Euler (0,0,ang);
			}

			if (Input.GetMouseButtonDown (1))
				CancelPurchase ();

			if (!purchaseMenu.isOpen) {
				if (Input.GetMouseButtonUp (0))
					PlaceModule ();

				if (Input.GetMouseButtonDown (0))
					isRotting = true;
			}
		}
	}

	bool CanPlaceAtPos (Vector3 pos) {

		int hits = 4;

		for (int i = 0; i < canPlaceTestPos.Length; i++) {

			Ray ray = new Ray (new Vector3 (pos.x + canPlaceTestPos[i].x * pModule.moduleClass, pos.y + canPlaceTestPos[i].y * pModule.moduleClass, camDepth), Vector3.forward * -camDepth * 2f);
			RaycastHit hit;

			Debug.DrawRay (ray.origin, ray.direction, Color.blue);

			if (Physics.Raycast (ray, out hit, -camDepth * 2f, turretLayer)) {
				// Make sure you cannot place modules on weapons
				if (hitModule)
					if (hitModule.moduleType == Module.Type.Weapon || hitModule.moduleType == Module.Type.Independent || hitModule.moduleClass < pModule.moduleClass || hitModule.moduleLayer >= 45)
						hits--;
			}else{
				if (hitModule)
					hits--;
			}
		}

		if (hits < 3)
			return false;

		// Handle structural module multiple collision checks
		if (pModule.moduleType == Module.Type.Structural) {
			StructuralModule str = pModule.GetComponent<StructuralModule>();

			for (int i = 0; i < str.colCheckPoints.Length; i++) {
				Vector3 p = placePos + placeRot * str.colCheckPoints[i];
				if (hitModule)
					p += Vector3.forward * (hitModule.transform.position.z - 1);
				if (Physics.CheckSphere (p, pModule.moduleClass / 4f))
					return false;
			}
		}

		return true;
	}

	void GetHitModule () {
		Ray ray = new Ray (placePos + Vector3.forward * camDepth, Vector3.forward * -camDepth * 2f);
		RaycastHit hit;
		
		if (Physics.Raycast (ray, out hit, -camDepth * 2f, turretLayer)) {
			hitModule = hit.collider.GetComponent<Module>();
		}else{
			hitModule = null;
		}
	}

	void PlaceModule () {

		bool allowPlacement = true;
		isRotting = false;
		
		// Figure out if there is a module on mouse position, and figure out what type.
		Ray ray = new Ray (placePos + Vector3.forward * camDepth, Vector3.forward * -camDepth * 2f);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, -camDepth * 2f, turretLayer)) {

			// Handle module placement on another module
			placePos = hit.transform.position + (placePos - hit.collider.transform.position);
			Debug.Log (placePos + ", " + allowPlacement);
			allowPlacement = CanPlaceAtPos (placePos);
		}

		if (allowPlacement) {
			GameObject m = (GameObject)Instantiate (purchaseModule, placePos, placeRot);
			m.transform.SetParent (hit.transform);
		}

		if (!Input.GetButton ("LShift")) {
			isPlacing = false;
			placementSprite.enabled = false;
		}
	}

	void UpdatePlacementSprite () {
		if (CanPlaceAtPos (placePos)) {
			placementSprite.color = Color.green;
		}else{
			placementSprite.color = Color.red;
		}

		placementSprite.transform.position = placePos + Vector3.forward * (camDepth + 1f);
		placementSprite.transform.rotation = placeRot;
	}

	void MoveCamera () {

		Vector3 movement = new Vector3 ();

		if (Input.mousePosition.x < 10)
			movement.x = -cameraMovementSpeed;
		if (Input.mousePosition.x > Screen.width - 10)
			movement.x = cameraMovementSpeed;
		if (Input.mousePosition.y < 10)
			movement.y = -cameraMovementSpeed;
		if (Input.mousePosition.y > Screen.height - 10)
			movement.y = cameraMovementSpeed;

		transform.position += movement * Time.deltaTime;
	}

	void OnDrawGizmos () {
		if (pModule) {
			if (pModule.moduleType == Module.Type.Structural) {
				StructuralModule str = pModule.GetComponent<StructuralModule>();
				for (int i = 0; i < str.colCheckPoints.Length; i++) {
					Vector3 pos = placePos + placeRot * str.colCheckPoints[i];
					if (hitModule)
						pos += Vector3.forward * (hitModule.transform.position.z - 1);
					Gizmos.DrawSphere (pos, pModule.moduleClass / 4f);
				}
			}
		}
	}
}
