using UnityEngine;
using System.Collections;

public class PlayerInput : MonoBehaviour {

	private enum WallDragStatus { Inactive, Adding, Removing };

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
	public ModuleContextMenu contextMenu;

	public Vector3[] canPlaceTestPos;

	public static PlayerInput cur;

	private bool isEditingWalls;
	private WallDragStatus wallDragStatus;
	private Vector3 wallDragStart;
	public Renderer wallDragGraphic;

	void Start () {
		cur = this;
		camDepth = Camera.main.transform.position.z;
	}

	public void SelectPurchaseable (GameObject purModule) {
		if (isEditingWalls)
			EditWalls ();
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

	public void OpenModuleMenu () {
		contextMenu.gameObject.SetActive (true);
		contextMenu.OpenModule (focusRoot);
	}

	public void EditWalls () {
		CancelPurchase ();
		isEditingWalls = !isEditingWalls;
		wallDragGraphic.enabled = isEditingWalls;
	}
	
	// Update is called once per frame
	void Update () {
		MoveCamera ();
		// Grap mouse position, and round it.
		pos = RoundPos (Camera.main.ScreenToWorldPoint (Input.mousePosition));
		
		if (!EnemySpawn.waveStarted) {
			if (isPlacing && !isEditingWalls) {

				// Offset class 1 modules
				if (pModule.moduleClass == 1)
					pos += Vector3.one * 0.5f;

				if (!isRotting) {
					placePos = new Vector3 (pos.x, pos.y, 0f);
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

			if (!isPlacing && isEditingWalls) {

				if (Input.GetButtonDown ("Cancel"))
					EditWalls ();

				if ((Input.GetMouseButtonDown (1) && wallDragStatus == WallDragStatus.Adding) || (Input.GetMouseButtonDown (0) && wallDragStatus == WallDragStatus.Removing)) {
					wallDragStatus = WallDragStatus.Inactive;
				}

				if (Input.GetMouseButtonDown (0) && wallDragStatus == WallDragStatus.Inactive) {
					wallDragStatus = WallDragStatus.Adding;
					wallDragStart = pos;
					wallDragGraphic.material.color = Color.green;
				}

				if (Input.GetMouseButtonUp (0) && wallDragStatus == WallDragStatus.Adding) {
					wallDragStatus = WallDragStatus.Inactive;
					Game.ChangeWalls (new Rect (wallDragStart.x, wallDragStart.y, pos.x - wallDragStart.x, pos.y - wallDragStart.y), true);
					wallDragGraphic.material.color = Color.white;
					HoverContext.ChangeText ("");
				}

				if (Input.GetMouseButtonDown (1) && wallDragStatus == WallDragStatus.Inactive) {
					wallDragStatus = WallDragStatus.Removing;
					wallDragStart = pos;
					wallDragGraphic.material.color = Color.red;
				}

				if (Input.GetMouseButtonUp (1) && wallDragStatus == WallDragStatus.Removing) {
					wallDragStatus = WallDragStatus.Inactive;
					Game.ChangeWalls (new Rect (wallDragStart.x, wallDragStart.y, pos.x - wallDragStart.x, pos.y - wallDragStart.y), false);
					wallDragGraphic.material.color = Color.white;
					HoverContext.ChangeText ("");
				}

				if (wallDragStatus != WallDragStatus.Inactive) {
					if (wallDragStart.x <= pos.x && wallDragStart.y <= pos.y) {
						wallDragGraphic.transform.localScale = new Vector3 (Mathf.Abs (pos.x - wallDragStart.x), Mathf.Abs (pos.y - wallDragStart.y));
						wallDragGraphic.transform.position = new Vector3 (wallDragStart.x + (pos.x - wallDragStart.x) / 2f, wallDragStart.y + (pos.y - wallDragStart.y) / 2f);

						int startX = Mathf.RoundToInt (Game.game.pathfinding.WorldToNode (new Vector3 (wallDragStart.x,wallDragStart.y)).x);
						int startY = Mathf.RoundToInt (Game.game.pathfinding.WorldToNode (new Vector3 (wallDragStart.x,wallDragStart.y)).y);
						int w = Mathf.RoundToInt (pos.x - wallDragStart.x);
						int h = Mathf.RoundToInt (pos.y - wallDragStart.y);
						
						if (wallDragStatus == WallDragStatus.Adding) {
							HoverContext.ChangeText ("Cost: " + Game.GetWallingCost (startX, startY, w, h, true));
						}else{
							HoverContext.ChangeText ("Cost: " + Game.GetWallingCost (startX, startY, w, h, false));
						}
					}

				}else{
					wallDragGraphic.transform.position = pos + Vector3.one * 0.5f;
					wallDragGraphic.transform.localScale = Vector3.one;
					HoverContext.ChangeText ("");
				}
			}
		}
	}

	Vector3 RoundPos (Vector3 p) {
		return new Vector3 (Mathf.Round (p.x/1f) * 1, Mathf.Round (p.y/1f) * 1, p.z);
	}

	bool CanPlaceAtPos (Vector3 pos) {

		int hits = 4;
		if (pModule.moduleCost > Game.credits)
			return false;

		for (int i = 0; i < canPlaceTestPos.Length; i++) {

			Ray ray = new Ray (new Vector3 (pos.x + canPlaceTestPos[i].x * pModule.moduleClass, pos.y + canPlaceTestPos[i].y * pModule.moduleClass, camDepth), Vector3.forward * -camDepth * 2f);
			RaycastHit hit;

			Debug.DrawRay (ray.origin, ray.direction, Color.blue);

			Module locModule = null;
			if (Physics.Raycast (ray, out hit, -camDepth * 2f, turretLayer)) {
				// Make sure you cannot place modules on weapons
				if (locModule == null) {
					locModule = hit.transform.GetComponent<Module>();
				}else if (locModule != hit.transform.GetComponent<Module>())
					return false;

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
			allowPlacement = CanPlaceAtPos (placePos);
		}

		if (allowPlacement) {
			GameObject m = (GameObject)Instantiate (purchaseModule, placePos, placeRot);
			Game.credits -= pModule.moduleCost;
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
	;