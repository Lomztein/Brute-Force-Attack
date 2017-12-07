using UnityEngine;
using System.Collections.Generic;
using IngameEditors;
using System.Linq;

public enum Colour { None, Blue, Green, Yellow, Orange, Red, Purple };

public class Module : MonoBehaviour {

    public const string MODULE_FILE_EXTENSION = ".dat";
    public const int MAX_UPGRADE_AMOUNT = 10;
    public static Colour[] allColours = new Colour[] { Colour.Blue, Colour.Green, Colour.Yellow, Colour.Orange, Colour.Red, Colour.Purple };
    public bool isOnBattlefield = true;

    public enum Type { Base, Rotator, Weapon, Structural, Independent };
    public Colour colour;

    public BaseModule parentBase;
    public Module rootModule;
    public List<Module> modules;
    public ModuleMod[] moduleMods;

    public string moduleName;
    [TextArea]
    public string moduleDesc;
    public string assemblyDesc;
    public string assemblyName;

    public bool[] upgradeButtonDisabled;
    public string[] upgradeNameReplacement = new string[3];
    public string[] upgradeDescReplacement = new string[3];
    public Sprite[] upgradeImageReplacement = new Sprite[3];
    public string scoreName;
    public int score;

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
		if (isOnBattlefield) InitializeModule ();
	}

    // A lot of these functions look similar, and I'm sure there is a better way, but I am yet to learn that way.
    public int GetAssemblyCost () {
        if (isRoot) {
            int cost = 0;
            for (int i = 0; i < modules.Count; i++) {
                cost += modules[i].moduleCost;
            }
            return cost;
        }
        Debug.LogWarning ("Tried to get assembly cost from non-root module.");
        return 0;
    }

    public float GetAssemblyDPS () {
        if (isRoot) {
            float dps = 0f;
            for (int i = 0; i < modules.Count; i++) {
                dps += modules[i].GetEfficiency ();
            }
            return dps;
        } else {
            Debug.LogWarning ("Tried to grap assembly DPS from non-root module.");
        }
        return 0f;
    }

    public int GetAssemblyUpgradeLevel (Type type) {
        if (isRoot) {
            for (int i = 0; i < modules.Count; i++) {
                if (modules[i].moduleType == type)
                    return modules[i].upgradeCount;
            }
        } else {
            Debug.LogWarning("Tried to grap assembly upgrade count from non-root module.");
        }
        return 0;
    }

    public float GetAssemblyAVGTurnSpeed () {
        if (isRoot) {
            float turnSpeed = 0f;
            RotatorModule[] rots = GetComponentsInChildren<RotatorModule> ();
            for (int i = 0; i < modules.Count; i++) {
                turnSpeed += modules[i].GetSpeed ();
            }
            turnSpeed /= rots.Length;
            return turnSpeed;
        } else {
            Debug.LogWarning ("Tried to grap assembly turnspeed from non-root module.");
        }
        return 0;
    }

    public void SetStartingUpgradeCost () {
        upgradeCost = moduleCost * 2;
    }

    public static Texture2D CombineSprites (Texture2D[] sprites, Vector3[] positions, int ppu = 16) {
		if (sprites.Length != positions.Length) {
			Debug.LogError ("Sprites array not equal length as positions array.");
			return null;
		}
		// First, get total size. <-- Top notch commenting there pal.
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

    public const float UPGRADE_CONST = 0.2f;
	
	public virtual bool UpgradeModule () {
		if (upgradeCount >= MAX_UPGRADE_AMOUNT) {
			return true;
		}
		upgradeCount++;
        // Debug.Log (GetUpgradePercentage ());
        upgradeMul *= 1 + GetUpgradePercentage ();
		upgradeCost = Mathf.RoundToInt (upgradeCost * 1.5f);
		if (upgradeCount >= MAX_UPGRADE_AMOUNT) {
			return true;
		}

        UpdateHoverContextElement ();
		return false;
	}

    public bool IsAssemblyUpgradeable (int t) {
        Module.Type type = (Module.Type)t;
        bool upgradeable = true;
        bool anyOfModule = false;

        if (t >= 0) {
            for (int i = 0; i < modules.Count; i++) {
                if (modules[i].moduleType == type && !modules[i].IsUpgradeable()) {
                    upgradeable = false;
                    break;
                }
            }
        } else {
            for (int i = 0; i < modules.Count; i++) {
                if (modules[i].IsUpgradeable()) {
                    return true;
                }
            }
            return false;
        }

        return upgradeable;
    }

    public bool UpgradeAssembly (int t) {
        Module.Type type = (Module.Type)t;

        if (Game.currentScene == Scene.Play && Game.credits >= rootModule.GetUpgradeCost (t) && IsAssemblyUpgradeable (t)) {

            bool canUpgrade = true;
            Game.credits -= rootModule.GetUpgradeCost (t);

            foreach (Module module in modules) {
                if (module.moduleType == type) {
                    canUpgrade = !module.UpgradeModule ();
                }
            }

            rootModule.upgradeButtonDisabled[t] = !canUpgrade;
            return canUpgrade;
        }

        return true;
    }

    public bool IsUpgradeable () {
        return upgradeCount < MAX_UPGRADE_AMOUNT;
    }

    public int GetUpgradeCost (int t) {

        int cost = 0;
        Module.Type type = (Module.Type)t;

        foreach (Module module in modules) {
            if (module.moduleType == type && module.IsUpgradeable ()) {
                cost += module.upgradeCost;
            }
        }

        return cost;
    }

    public float GetUpgradePercentage () {
        return 0.3f;
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

	public Assembly assembly;
	
	public void SaveModuleAssembly (string filename) {
		string file = Game.MODULE_ASSEMBLY_SAVE_DIRECTORY + filename + MODULE_FILE_EXTENSION;

		assembly = new Assembly ();
        assembly.assemblyName = assemblyName;
        assembly.assemblyDesc = assemblyDesc;

		rootModule.BroadcastMessage ("SaveModuleToAssembly", SendMessageOptions.RequireReceiver);
        Debug.Log(file);
		Assembly.SaveToFile (filename, assembly);
	}
	
	void SaveModuleToAssembly () {
		if (transform.parent) {
			rootModule.assembly.parts.Add (new Assembly.Part (isRoot, moduleName, moduleIndex, transform.parent.GetComponent<Module> ().moduleIndex, 
			                                                  (transform.position.x - rootModule.transform.position.x),
			                                                  (transform.position.y - rootModule.transform.position.y)
			                                                  , transform.eulerAngles.z));
		}else{
			rootModule.assembly.parts.Add (new Assembly.Part (isRoot, moduleName, moduleIndex, 0, 
			                                                  (transform.position.x - rootModule.transform.position.x),
			                                                  (transform.position.y - rootModule.transform.position.y)
			                                                  , transform.eulerAngles.z));
		}
	}

    public virtual float GetRange () {
        if (parentBase)
            return parentBase.GetRange ();
        else
            return 0f;
    }

    public virtual float GetEfficiency () {
        return 0f;
    }

    public virtual float GetSpeed () {
        return 0f;
    }

    public void InitializeModule () {

		if (upgradeButtonDisabled == null || upgradeButtonDisabled.Length != 3)
            upgradeButtonDisabled = new bool[3];

        if (upgradeCost == 0)
            SetStartingUpgradeCost ();

		FindParentBase ();
		FindModuleLayer ();
		transform.position = new Vector3 (transform.position.x, transform.position.y, -moduleLayer);

        Game.currentModules.Add (this);

		rootModule = FindRootModule ();
		if (rootModule == this) {
			isRoot = true;
			moduleIndex = 0;
			saveIndex = 0;

            if (Game.currentScene == Scene.Play) {
                Game.ChangeWalls(GetRelativeModuleRect(), Game.WallType.WithTurret, true);
            } else
                AssemblyEditorScene.cur.rootModule = this;

            UpdateHoverContextElement ();
            modules = new List<Module> ();
        }

        if (!rootModule.modules.Contains (this))
            rootModule.modules.Add (this);

        rootModule.UpdateAssemblyColliders ();

        moduleIndex = rootModule.GetModuleIndex ();

        if (parentBase) parentBase.GetFastestBulletSpeed ();
		SendMessageUpwards ("OnNewModuleAdded", SendMessageOptions.DontRequireReceiver);
	}

    void UpdateAssemblyColliders () {
        if (isRoot) {
            for (int i = 0; i < modules.Count; i++) {
                if (modules[i] != this && Game.currentScene == Scene.Play) {
                    modules[i].GetComponent<Collider> ().enabled = false;
                }
            }
            UpdateHoverContextElement ();
        }
    }

    public int GetFullUpgradeCost () {
        return GetUpgradeCost (0) + GetUpgradeCost (1) + GetUpgradeCost (2);
    }

    public void UpdateHoverContextElement () {
        if (isRoot && Game.currentScene == Scene.Play) {
            HoverContextElement el = GetComponent<HoverContextElement> ();
            if (Game.game && PlayerInput.cur.isUpgrading) {
                if (IsAssemblyUpgradeable(-1)) {
                    el.text = "Upgrade Cost: " + GetFullUpgradeCost() + " LoC" +
                    "\n\tLevels - R/D/T: " + GetAssemblyUpgradeLevel(Type.Base) + "/" + GetAssemblyUpgradeLevel(Type.Weapon) + "/" + GetAssemblyUpgradeLevel(Type.Rotator);
                } else {
                    el.text = "Maxed out";
                }
            } else {
                el.text = assemblyName + " - " + ((int)GetAssemblyDPS ()).ToString () + " DPS";
            }
        }
    }

    public void SellAssembly () {
        for (int i = 0; i < modules.Count; i++) {
            modules[i].SellModule ();
        }
    }

	public void SellModule () {
        Game.credits += GetSellAmount ();

        PlayerInput.cur.contextMenu.ExitMenu();
        AssemblyCircleMenu.Close();

		DestroyModule ();
	}

    public int GetSellAmount () {
        return Mathf.FloorToInt (moduleCost * (float)(upgradeCount + 1) * 0.75f) + 1;
    }

	public void BlockArea () {
        if (isRoot && Game.currentScene == Scene.Play) {
            Pathfinding.ChangeArea (GetModuleRect (), false);
        }
	}

	public void StockModule () {
		BroadcastMessage ("Stockify");
	}

	void Stockify () {
		if (Game.currentScene == Scene.Play) Pathfinding.ChangeArea (GetModuleRect (), true);
		DestroyModule ();
		PurchaseMenu.AddStock (this);
	}

	public void DestroyModule (bool removeFromList = true) {
		Destroy (gameObject);
		if (removeFromList)
            Game.currentModules.Remove (this);

        if (Game.currentScene == Scene.Play) {
            Pathfinding.ChangeArea (GetModuleRect (), true);
            Game.ChangeWalls (GetRelativeModuleRect (), Game.WallType.Player, true);
        }

        rootModule.modules.Remove (this);
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

    public Rect GetRelativeModuleRect () {
        Bounds b = GetComponent<BoxCollider>().bounds;
        return new Rect(transform.position.x - b.size.x / 2f,
                        transform.position.y - b.size.y / 2f,
                        transform.position.x + b.size.x / 2f,
                        transform.position.y + b.size.y / 2f);
    }

    void OnMouseDownElement () {
        // The entire isUpgrading part of this isn't used anymore. Remove if it annoys you.
        if (PlayerInput.cur.isUpgrading && isRoot && Game.credits >= GetFullUpgradeCost ()) {
            UpgradeAssembly (0);
            UpgradeAssembly (1);
            UpgradeAssembly (2);

            UpdateHoverContextElement();
            GetComponent<HoverContextElement> ().ForceUpdate ();
        } else if (!PlayerInput.cur.isUpgrading) {
            AssemblyCircleMenu.Open(Camera.main.WorldToScreenPoint(transform.position), this);
        }
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

    public void ForceUpdateAllModGUI () {
        List<ModuleMod> remaining = moduleMods.ToList ();
        while (remaining.Count > 0) {
            remaining[0].UpdateGUIElement ();
            remaining.AddRange (remaining[0].subparts);
            remaining.RemoveAt (0);
        }
    }

	public void RequestChildModules () {
		requestedModules = new List<Module>();
		rootModule.BroadcastMessage ("ReturnModuleToRequester", this, SendMessageOptions.DontRequireReceiver);
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
		transform.Find ("Sprite").GetComponent<SpriteRenderer>().material = PlayerInput.cur.defualtMaterial;
		GetComponent<Collider>().enabled = true;
		enabled = true;
	}

	public virtual void SetIsBeingPlaced () {
		transform.Find ("Sprite").GetComponent<SpriteRenderer>().material = PlayerInput.cur.placementMaterial;
		GetComponent<Collider>().enabled = false;
		enabled = false;
	}

	public override string ToString () {
		return "Uh, this is the base module functionality code. Why would you ever want to grap the stats of this?";
	}

}