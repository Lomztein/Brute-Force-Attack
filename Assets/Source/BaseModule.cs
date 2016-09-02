using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaseModule : Module {

	[Header ("Targeting")]
	public Transform target;
	public Vector3 targetPos;
	private Vector3 prevPos;
	private Vector3 targetVel;
	public TargetFinder.SortType sortType = TargetFinder.SortType.Closest;

	public LayerMask targetLayer;
	private TargetFinder targetFinder = new TargetFinder ();

	public static bool enableAdvancedTracking;

	[Header ("Stats")]
	public float range;
	private float targetingRange;
	private float fastestBulletSpeed;
	public List<Colour> priorities;
    public List<Colour> ignore;
	public int maxSupportedWeight;
	public int currentWeight;

	[Header ("Boosts")]
	public float damageBoost = 1f;
	public float firerateBoost = 1f;

	// Update is called once per frame
	void Update () {
        if (!target) {
            FindTarget ();
        } else if (Vector3.Distance (transform.position, targetPos) > GetRange () || !target.gameObject.activeSelf) {
            target = null;
		}
	}

    void FixedUpdate () {
        if (target) {
            if (enableAdvancedTracking && fastestBulletSpeed > 1.1) {
                Vector3 delPos = target.position - prevPos;
                if (delPos.magnitude > 0.1f) {
                    targetVel = delPos / Time.fixedDeltaTime;
                    targetPos = target.position + Vector3.Distance (transform.position, target.position) / fastestBulletSpeed * targetVel;
                    prevPos = target.position;
                } else {
                    targetPos = target.position;
                }
            } else {
                targetPos = target.position;
            }
        }
    }

	public override void Start () {
		base.Start ();
		targetingRange = range;
	}

    public override bool UpgradeModule () {
        bool passed = base.UpgradeModule();
        upgradeMul = 1f + ((float)upgradeCount / MAX_UPGRADE_AMOUNT);
        return passed;
    }

    public override float GetRange () {
        if (Game.game && Game.game.gamemode != Gamemode.VDay)
            return Mathf.Min (range, targetingRange) * upgradeMul * ResearchMenu.rangeMul;
        else
            return Mathf.Min (range, targetingRange) * upgradeMul * ResearchMenu.rangeMul * 2.5f;
    }

    public float GetBaseRange () {
        if (Game.game && Game.game.gamemode != Gamemode.VDay)
            return range * upgradeMul * ResearchMenu.rangeMul;
        else
            return range * upgradeMul * ResearchMenu.rangeMul * 2.5f;
    }

	void FindTarget () {
		target = targetFinder.FindTarget (transform.position, GetRange (), targetLayer, priorities.ToArray (), ignore.ToArray (), sortType);
        if (target)
			targetPos = target.position;
	}

    public void OnToggleModBlue (int newIndex) {
        ChangePriority (Colour.Blue, newIndex);
    }
    public void OnToggleModGreen (int newIndex) {
        ChangePriority (Colour.Green, newIndex);
    }
    public void OnToggleModYellow (int newIndex) {
        ChangePriority (Colour.Yellow, newIndex);
    }
    public void OnToggleModOrange (int newIndex) {
        ChangePriority (Colour.Orange, newIndex);
    }
    public void OnToggleModRed (int newIndex) {
        ChangePriority (Colour.Red, newIndex);
    }
    public void OnToggleModPurple (int newIndex) {
        ChangePriority (Colour.Purple, newIndex);
    }
    public void OnToggleModAll ( int newIndex ) {
        ChangePriority (Colour.Blue, newIndex);
        ChangePriority (Colour.Green, newIndex);
        ChangePriority (Colour.Yellow, newIndex);
        ChangePriority (Colour.Orange, newIndex);
        ChangePriority (Colour.Red, newIndex);
        ChangePriority (Colour.Purple, newIndex);
        ForceUpdateAllModGUI ();
    }

    public void OnInitializeToggleModBlue (ModuleMod mod) {
        mod.GetReturnedMeta (GetSortMeta (Colour.Blue));
    }
    public void OnInitializeToggleModGreen (ModuleMod mod) {
        mod.GetReturnedMeta (GetSortMeta (Colour.Green));
    }
    public void OnInitializeToggleModYellow (ModuleMod mod) {
        mod.GetReturnedMeta (GetSortMeta (Colour.Yellow));
    }
    public void OnInitializeToggleModOrange (ModuleMod mod) {
        mod.GetReturnedMeta (GetSortMeta (Colour.Orange));
    }
    public void OnInitializeToggleModRed (ModuleMod mod) {
        mod.GetReturnedMeta (GetSortMeta (Colour.Red));
    }
    public void OnInitializeToggleModPurple (ModuleMod mod) {
        mod.GetReturnedMeta (GetSortMeta (Colour.Purple));
    }

    public void OnInitializeToggleModAll ( ModuleMod mod ) {
        // Ignore, normal, prioritize
        int[] results = new int[3];

        for (int i = 0; i < allColours.Length; i++) {
            Colour col = allColours[i];

            if (ignore.Contains (col))
                results[2]++;
            if (priorities.Contains (col))
                results[1]++;

            if (!priorities.Contains (col) && !ignore.Contains(col))
                results[0]++;
        }

        for (int i = 0; i < results.Length; i++) {
            if (results[i] == allColours.Length) {
                mod.GetReturnedMeta(i);
                return;
            }
            Debug.Log (results[i] + ", " + allColours.Length);
        }

        mod.GetReturnedMeta (3);
    }

    public void OnInitializeToggleModSorting (ModuleMod mod) {
        mod.GetReturnedMeta ((int)sortType);
    }

    public int GetSortMeta (Colour colour) {
        if (priorities.Contains (colour)) {
            return 1;
        } else if (ignore.Contains (colour))
            return 2;
        return 0;
    }

    public void ChangePriority (Colour colour, int newPriority) {
        switch (newPriority) {
            case 0:
                if (priorities.Contains (colour))
                    priorities.Remove (colour);
                if (ignore.Contains (colour))
                    ignore.Remove (colour);
                break;

            case 1:
                if (!priorities.Contains (colour))
                    priorities.Add (colour);
                if (ignore.Contains (colour))
                    ignore.Remove (colour);
                break;

            case 2:
                if (priorities.Contains (colour))
                    priorities.Remove (colour);
                if (!ignore.Contains (colour))
                    ignore.Add (colour);
                break;
        }

    }

	void OnNewModuleAdded () {
        GetFastestBulletSpeed ();
	}

	public void GetFastestBulletSpeed () {
        if (Game.currentScene == Scene.Play) {
            Weapon[] weapons = GetComponentsInChildren<Weapon>();
            float speed = 0;
            float r = float.MaxValue;
            for (int i = 0; i < weapons.Length; i++) {
                if (weapons[i].bulletSpeed > speed) {
                    speed = weapons[i].bulletSpeed;
                    if (weapons[i].GetBulletData().isHitscan) {
                    speed = 100000;
                    }
                }

                if (!priorities.Contains(weapons[i].GetBulletData().effectiveAgainst)) {
                    priorities.Add(weapons[i].GetBulletData().effectiveAgainst);
                }

                if (weapons[i].weaponModule.rangeMultiplier * GetBaseRange () < r) {
                    r = weapons[i].weaponModule.rangeMultiplier * GetBaseRange ();
                }

                weapons[i].damageMul = damageBoost;
                weapons[i].firerateMul = (1f / firerateBoost);
            }

            targetingRange = r;
            fastestBulletSpeed = speed;
        }
	}

	public void RequestRange (GameObject rangeIndicator) {
		rangeIndicator.SendMessage ("GetRange", GetRange ());
	}

	public override string ToString () {
		string text = "";
		text += "Range: " + (GetRange ()).ToString () + " - \n\n";
		if (damageBoost != 1f) {
			text += "Damage Multiplier: " + damageBoost.ToString () + " - \n\n";
		}
		if (firerateBoost != 1f) {
			text += "Firerate Mulitplier: " + firerateBoost.ToString ();
		}
		return text;
	}

    public void OnToggleModSorting (int newSort) {
        sortType = (TargetFinder.SortType)newSort;
    }

	public override void SetIsBeingPlaced () {
		base.SetIsBeingPlaced ();
		targetingRange = range;
	}
}
