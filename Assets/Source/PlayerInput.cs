using UnityEngine;
using System.Collections;
using IngameEditors;

public class PlayerInput : MonoBehaviour {

	private enum WallDragStatus { Inactive, Adding, Removing };

	public float cameraMovementSpeed;
	public LayerMask turretLayer;

	public PurchaseMenu purchaseMenu;

	public Transform placementParent;
	public GameObject purchaseModule;
	private Module pModule;
	public bool isPlacing;
	private bool isRotting;
    public bool isUpgrading;
    public GameObject hoverMarker;
    public SpriteRenderer upgradingMarker;
    public GameObject constructionFlash;

    private bool enableFastBuild;
	private Vector2 dragStart;
	private Vector2 dragEnd;

	private Vector3 pos;
	[HideInInspector] public Vector3 placePos;
	private Quaternion placeRot;
	
	private Module hitModule;

	[HideInInspector] public float camDepth;
	private float ang;

	public Module focusRoot;
	public AssemblyContextMenu contextMenu;

	public Vector3[] canPlaceTestPos;
    public LayerMask blockBuildLayer;

	public static PlayerInput cur;

	public bool isEditingWalls;
	private WallDragStatus wallDragStatus;
	private Vector3 wallDragStart;
	public Renderer wallDragGraphic;

    private Vector3 wallGraphicStart;
    private Vector3 wallGraphicEnd;

	public Material placementMaterial;
	public Material defualtMaterial;
	public int currentCost;

	private RangeIndicator rangeIndicator;
	private float indicatorRange;
	private GameObject activePurchaseCopy; // Activated copy of purchaseModule, ment for using SendMessage and stuffz.
	private Material rangeIndicatorMaterial;

	public BoxCollider selectorCollider;
	public Transform selectorDragGraphic;

	private Vector3 startSelectorDrag;
	private Vector3 endSelectorDrag;
	public LayerMask selectorMask;

	private GameObject activeAbility;

	void Start () {
		cur = this;
		camDepth = Camera.main.transform.position.z;
		rangeIndicator = RangeIndicator.CreateRangeIndicator (null, Vector3.zero, false, false).GetComponent<RangeIndicator> ();
		rangeIndicatorMaterial = rangeIndicator.transform.GetChild (0).GetComponent<Renderer>().material;
	}

	public void SelectPurchaseable (GameObject purModule, bool resetRotation) {
		CancelAll ();
        GameObject loc = Instantiate (purModule);
		loc.BroadcastMessage ("SetIsBeingPlaced", SendMessageOptions.DontRequireReceiver);
		if (resetRotation)
			loc.transform.rotation = Quaternion.Euler (0,0, loc.transform.eulerAngles.z + placementParent.eulerAngles.z);
		loc.transform.parent = placementParent;
		loc.transform.position = placementParent.position;
		purchaseModule = purModule;
		pModule = purModule.GetComponent<Module>();
		isPlacing = true;

		rangeIndicator.ForceParent (loc, placePos);

		if (activePurchaseCopy)
			Destroy (activePurchaseCopy);

		activePurchaseCopy = (GameObject)Instantiate (purchaseModule, Vector3.right * 10000f, Quaternion.identity);
		activePurchaseCopy.GetComponent<Module>().isOnBattlefield = false;
	}

	public GameObject SelectAbilty (GameObject ability, AbilityButton button) {
		CancelAll ();
		if (activeAbility)
			Destroy (activeAbility);
		activeAbility = (GameObject)Instantiate (ability);
		activeAbility.GetComponent<Ability>().button = button;
		return activeAbility;
	}

    void CancelAll () {
		CancelPurchase ();
		if (isEditingWalls)
			EditWalls (true);
        if (isUpgrading) {
            ToggleUpgrading (true);
        }
	}

    public void ToggleUpgrading (bool fromCancelAll) {
        if (!fromCancelAll)
            CancelAll();
        isUpgrading = !isUpgrading;
        for (int i = 0; i < Game.currentModules.Count; i++) {
            Game.currentModules[i].UpdateHoverContextElement ();
        }

        upgradingMarker.gameObject.SetActive (isUpgrading);
        if (!isUpgrading)
            upgradingMarker.transform.position = Vector3.right * 10000f;
    }

    public void SetPurchaseableFromSceneObject (GameObject purModule) {
		purchaseModule = purModule;
		pModule = purModule.GetComponent<Module>();
	}

	void CancelPurchase () {
		isPlacing = false;
		purchaseModule = null;
		pModule = null;
		isRotting = false;
		foreach (Transform child in placementParent) {
			Destroy (child.gameObject);
            child.transform.parent = null;
		}
		Destroy (activePurchaseCopy);
		placementParent.rotation = Quaternion.identity;
		rangeIndicator.NullifyParent ();
	}

	public void OpenModuleMenu () {
		contextMenu.gameObject.SetActive (true);
		contextMenu.OpenAssembly (focusRoot);
	}

	public void EditWalls (bool fromCancelAll) {
		if (!fromCancelAll)
            CancelAll ();

		isEditingWalls = !isEditingWalls;
		wallDragGraphic.enabled = isEditingWalls;

		if (!isEditingWalls) {
            wallDragStart = new Vector3(10000, 0, 0);
            wallGraphicStart = new Vector3(10000, 0, 0);
            wallGraphicEnd = new Vector3(10000, 0, 0);
            wallDragStatus = WallDragStatus.Inactive;
		}
	}

    // Update is called once per frame
    void Update () {
        if (Game.currentScene == Scene.Play)
            MoveCamera ();
        // Grap mouse position, and round it.
        pos = RoundPos(Camera.main.ScreenToWorldPoint(Input.mousePosition), pModule ? pModule.moduleClass : 1);

        rangeIndicator.GetRange (0f);

        if (Input.GetButtonDown("Cancel"))
            CancelAll ();

        if (!EnemyManager.waveStarted) {
            if (isPlacing && !isEditingWalls) {

                rangeIndicator.transform.position = placementParent.position;

                if (!isRotting) {
                    placePos = new Vector3 (pos.x, pos.y, 0f);
                    placeRot = Quaternion.Euler (0, 0, 90f);
                }

                UpdatePlacementSprite ();
                GetHitModule ();

                indicatorRange = 0f;
                RangeIndicator.ForceRequestRange (activePurchaseCopy, gameObject);

                if (hitModule) {
                    placeRot = hitModule.transform.rotation;

                    if (hitModule.moduleType == Module.Type.Weapon || hitModule.moduleType == Module.Type.Independent) {
                        rangeIndicator.GetRange (0f);
                    } else if (pModule.moduleType == Module.Type.Weapon) {
                        if (hitModule.parentBase) {
                            rangeIndicator.GetRange (indicatorRange * hitModule.parentBase.GetRange ());
                        } else {
                            rangeIndicator.GetRange (indicatorRange * WeaponModule.indieRange);
                        }
                    } else {
                        rangeIndicator.GetRange (indicatorRange);
                    }
                } else {
                    if (pModule.moduleType == Module.Type.Weapon) {
                        rangeIndicator.GetRange (indicatorRange * WeaponModule.indieRange);
                    } else if (pModule.moduleType == Module.Type.Base) {
                        rangeIndicator.GetRange (pModule.GetComponent<BaseModule> ().GetRange ());
                    }
                }

                if (isRotting) {
                    ang = Mathf.RoundToInt (Angle.CalculateAngle (placePos, pos) / 90f) * 90;
                    if (Input.GetButton ("LCtrl"))
                        placeRot = Quaternion.Euler (0, 0, ang);
                } else {
                    ang = placeRot.eulerAngles.z;
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

                wallGraphicStart = wallDragGraphic.transform.position + wallDragGraphic.transform.localScale / 2f;
                wallGraphicEnd = wallDragGraphic.transform.position - wallDragGraphic.transform.localScale / 2f;

                if ((Input.GetMouseButtonDown (1) && wallDragStatus == WallDragStatus.Adding) || (Input.GetMouseButtonDown (0) && wallDragStatus == WallDragStatus.Removing)) {
                    wallDragStatus = WallDragStatus.Inactive;
                }

                if (Input.GetMouseButtonDown (0) && wallDragStatus == WallDragStatus.Inactive) {
                    wallDragStatus = WallDragStatus.Adding;
                    wallDragStart = pos;
                    wallDragGraphic.sharedMaterial.color = Color.green;
                }

                if (Input.GetMouseButtonUp (0) && wallDragStatus == WallDragStatus.Adding) {
                    wallDragStatus = WallDragStatus.Inactive;
                    Game.ChangeWalls (new Rect (wallGraphicStart.x, wallGraphicStart.y, wallGraphicEnd.x, wallGraphicEnd.y), Game.WallType.Player);
                    wallDragGraphic.sharedMaterial.color = Color.white;
                    HoverContext.ChangeText ("");
                }

                if (Input.GetMouseButtonDown (1) && wallDragStatus == WallDragStatus.Inactive) {
                    wallDragStatus = WallDragStatus.Removing;
                    wallDragStart = pos;
                    wallDragGraphic.sharedMaterial.color = Color.red;
                }

                if (Input.GetMouseButtonUp (1) && wallDragStatus == WallDragStatus.Removing) {
                    wallDragStatus = WallDragStatus.Inactive;
                    Game.ChangeWalls (new Rect (wallGraphicStart.x, wallGraphicStart.y, wallGraphicEnd.x, wallGraphicEnd.y), Game.WallType.None);
                    wallDragGraphic.sharedMaterial.color = Color.white;
                    HoverContext.ChangeText ("");
                }

                if (wallDragStatus != WallDragStatus.Inactive) {

                    wallDragGraphic.transform.localScale = new Vector3 (Mathf.Abs (pos.x - wallDragStart.x), Mathf.Abs (pos.y - wallDragStart.y)) + Vector3.one;
                    wallDragGraphic.transform.position = new Vector3 (wallDragStart.x + (pos.x - wallDragStart.x) / 2f, wallDragStart.y + (pos.y - wallDragStart.y) / 2f);
                    wallDragGraphic.sharedMaterial.mainTextureScale = new Vector2 (wallDragGraphic.transform.localScale.x, wallDragGraphic.transform.localScale.y);

                    //Vector3 absStart = new Vector3(Mathf.Abs(wallDragStart.x), Mathf.Abs(wallDragStart.y));
                    //Vector3 absPos = new Vector3(Mathf.Abs(pos.x), Mathf.Abs(pos.y));



                    Rect rect = Game.PositivizeRect (new Rect (wallGraphicStart.x, wallGraphicStart.y, wallGraphicEnd.x, wallGraphicEnd.y));

                    int rectX = Mathf.RoundToInt(rect.x);
                    int rectY = Mathf.RoundToInt(rect.y);
                    int rectW = Mathf.RoundToInt(rect.width);
                    int rectH = Mathf.RoundToInt(rect.height);

                    if (wallDragStatus == WallDragStatus.Adding) {
                        HoverContext.ChangeText ("Cost: " + Game.GetWallingCost (rectX, rectY, rectW, rectH, Game.WallType.Player));
                    } else {
                        HoverContext.ChangeText("Cost: " + Game.GetWallingCost(rectX, rectY, rectW, rectH, Game.WallType.None));
                    }

                } else {
                    wallDragGraphic.transform.position = pos + Vector3.forward;
                    wallDragGraphic.transform.localScale = Vector3.one;
                    wallDragGraphic.sharedMaterial.mainTextureScale = new Vector2 (wallDragGraphic.transform.localScale.x, wallDragGraphic.transform.localScale.y);
                    HoverContext.ChangeText ("");
                }
            }
        }

        if (!isPlacing && !isEditingWalls) {

            Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast (ray, out hit, -camDepth * 2f, turretLayer)) {

                Module mod = hit.collider.GetComponent<Module> ();

                hoverMarker.transform.position = hit.collider.transform.position + Vector3.forward * (camDepth + 1);
                hoverMarker.transform.localScale = Vector3.one * mod.moduleClass;

                if (isUpgrading) {
                    upgradingMarker.transform.position = hit.collider.transform.position + Vector3.right * mod.moduleClass * 2f;
                    if (mod.GetUpgradeCost ((int)mod.moduleType) > Game.credits || !mod.IsAssemblyUpgradeable (-1)) {
                        upgradingMarker.color = Color.red;
                    } else {
                        upgradingMarker.color = Color.green;
                    }
                }
            } else {
                hoverMarker.transform.position = Vector3.right * 10000f;
                upgradingMarker.transform.position = (Vector3)(Vector2)(Camera.main.ScreenToWorldPoint (Input.mousePosition) + Vector3.right * 2f) + Vector3.forward * (camDepth + 1f);
                upgradingMarker.color = Color.white;
            }
        }

        if (isUpgrading) {
            if (Input.GetMouseButtonDown (1))
                CancelAll ();
        }
    }

	Vector3 RoundPos (Vector3 p, int moduleClass) {
		pos = new Vector3 (Mathf.Round (p.x/1f) * 1, Mathf.Round (p.y/1f) * 1, p.z);

	    if (moduleClass == 1) {
			pos = new Vector3 (Mathf.Round (p.x/1f + 0.5f) * 1, Mathf.Round (p.y/1f + 0.5f) * 1, p.z);
			pos -= Vector3.one * 0.5f;
			// It's pretty hacky, but it works.
		}

		return pos;
	}

	void GetRange (float _range) {
		indicatorRange = _range;
	}

	bool CanPlaceAtPos (Vector3 pos) {

		int hits = 4;
		if (currentCost > Game.credits && !purchaseMenu.stockModules.ContainsKey (pModule.gameObject)) {
            return false;
        }

        if (pModule.moduleType == Module.Type.Weapon) {
			if (hitModule) {
				if (!hitModule.parentBase)
					return false;
			}else{
				return false;
			}
		}

		for (int i = 0; i < canPlaceTestPos.Length; i++) {

            if (Game.currentScene == Scene.Play) {
                if (!Game.IsInsideBattlefield (pos + canPlaceTestPos[i]))
                    return false;

                if (Game.isWalled[(int)Game.WorldToWallPos (pos + canPlaceTestPos[i]).x, (int)Game.WorldToWallPos (pos + canPlaceTestPos[i]).y] != Game.WallType.Player)
                    return false;
            }

			Ray ray = new Ray (new Vector3 (pos.x + canPlaceTestPos[i].x * pModule.moduleClass, pos.y + canPlaceTestPos[i].y * pModule.moduleClass, camDepth), Vector3.forward * -camDepth * 2f);
			RaycastHit hit;

			Debug.DrawRay (ray.origin, ray.direction, Color.blue);

            if (Physics.Raycast(ray, -camDepth, blockBuildLayer))
                return false;

			Module locModule = null;
			if (Physics.Raycast (ray, out hit, -camDepth * 2f, turretLayer)) {
				// Make sure you cannot place modules on weapons
				if (locModule == null) {
					locModule = hit.transform.GetComponent<Module>();
				}else if (locModule != hit.transform.GetComponent<Module>())
					return false;

				if (hitModule)
					if (hitModule.moduleType == Module.Type.Weapon
                        || hitModule.moduleType == Module.Type.Independent
                        || hitModule.moduleClass < pModule.moduleClass
                        || hitModule.moduleLayer >= 45
                        || Game.currentScene == Scene.Play)
						hits--;
			}else{
				if (hitModule)
					hits--;
			}
		}

		if (hits < 4)
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

	GameObject GetStandardModule (Module.Type moduleType, int moduleClass, out string message) {
		message = "";
		for (int i = 0; i < purchaseMenu.standard.Count; i++) {
			Module mod = purchaseMenu.standard[i].GetComponent<Module>();
			if (mod.moduleType == moduleType && mod.moduleClass == moduleClass) {
				return purchaseMenu.standard[i];
			}
		}

		message = "No " + moduleType.ToString () + " found.";
		return null;
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

		bool allowPlacement = CanPlaceAtPos (placePos);
		isRotting = false;
		
		// Figure out if there is a module on mouse position, and figure out what type.
		Ray ray = new Ray (placePos + Vector3.forward * camDepth, Vector3.forward * -camDepth * 2f);
		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, -camDepth * 2f, turretLayer)) {

			// Handle module placement on another module
			placePos = hit.transform.position + (placePos - hit.collider.transform.position);
			allowPlacement = CanPlaceAtPos (placePos);
		}

        // I'll go and fix the wall blocking bug now. I'm gonna go for the first solution described in my Reddit reply for now.

		if (allowPlacement) {
            ConstructionFlashShrink.Create(Vector3.one * pModule.moduleClass * 16f, placePos - Vector3.back * (camDepth + 5));

            GameObject m = (GameObject)Instantiate (purchaseModule, placePos, placementParent.GetChild (0).rotation);
			m.BroadcastMessage ("ResetMaterial");
			rangeIndicator.NullifyParent ();
			if (Game.currentScene == Scene.Play) {
				if (purchaseMenu.stockModules.ContainsKey (purchaseModule)) {
					purchaseMenu.stockModules[purchaseModule]--;
					if (purchaseMenu.stockModules[purchaseModule] < 1) {
						purchaseMenu.stockModules.Remove (purchaseModule);
					}
					PurchaseMenu.UpdateButtons ();
				}else{
					Game.credits -= currentCost;
				}
			}
			m.transform.SetParent (hit.transform);
		}

		if (!Input.GetButton ("LShift")) {
			CancelPurchase ();
		}
	}

	void UpdatePlacementSprite () {
		if (CanPlaceAtPos (placePos)) {
			placementMaterial.color = Color.green;
			rangeIndicatorMaterial.color = Color.green;
		}else{
			placementMaterial.color = Color.red;
			rangeIndicatorMaterial.color = Color.red;
		}

		placementParent.position = placePos;
		placementParent.rotation = placeRot;

		if (hitModule) {
			placementParent.position += Vector3.forward * (hitModule.transform.position.z - 1);
		}else{
			placementParent.position -= Vector3.forward * placementParent.position.z;
		}
	}

	void MoveCamera () {

		Vector3 movement = new Vector3 ();

		if (Game.enableMouseMovement) {
			if (Input.mousePosition.x < 10)
				movement.x = -cameraMovementSpeed;
			if (Input.mousePosition.x > Screen.width - 10)
				movement.x = cameraMovementSpeed;
			if (Input.mousePosition.y < 10)
				movement.y = -cameraMovementSpeed;
			if (Input.mousePosition.y > Screen.height - 10)
				movement.y = cameraMovementSpeed;
		}else{
			movement.x += Input.GetAxis ("RightLeft") * cameraMovementSpeed;
			movement.y += Input.GetAxis ("UpDown") * cameraMovementSpeed;
		}

		Vector3 camPos = transform.position;
		if (camPos.x > Game.game.battlefieldWidth / 2f)
			camPos.x = Game.game.battlefieldWidth / 2f;
		if (camPos.y > Game.game.battlefieldHeight / 2f)
			camPos.y = Game.game.battlefieldHeight / 2f;
		if (camPos.x < -Game.game.battlefieldWidth / 2f)
			camPos.x = -Game.game.battlefieldWidth / 2f;
		if (camPos.y < -Game.game.battlefieldHeight / 2f)
			camPos.y = -Game.game.battlefieldHeight / 2f;

		transform.position = camPos;
		transform.position += movement * Time.deltaTime;
	}

	void OnDrawGizmos () {
		Gizmos.DrawSphere (placePos, 0.5f);
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

        if (isEditingWalls && wallDragStatus != WallDragStatus.Inactive) {
            Gizmos.DrawSphere(pos, 0.5f);
            Gizmos.DrawSphere(wallDragStart, 0.5f);
            Gizmos.DrawCube(wallDragStart + (pos - wallDragStart) / 2f, new Vector3 (Mathf.Abs (wallDragStart.x - pos.x), Mathf.Abs (wallDragStart.y - pos.y)) + Vector3.one);
        }
	}
}
