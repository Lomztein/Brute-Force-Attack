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
		}else{

			if (enableAdvancedTracking && fastestBulletSpeed > 1.1) {
				Vector3 delPos = target.position - prevPos;
				if (delPos.magnitude > 0.1f) {
					targetVel = delPos/Time.fixedDeltaTime;
					targetPos = target.position + Vector3.Distance (transform.position, target.position)/fastestBulletSpeed * targetVel;
					prevPos = target.position;
				}else{
					targetPos = target.position;
				}
			}else{
				targetPos = target.position;
			}

			if (Vector3.Distance (transform.position, targetPos) > GetRange () || !target.gameObject.activeSelf)
				target = null;
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

    public float GetRange () {
        if (Game.game && Game.game.gamemode != Gamemode.VDay)
            return Mathf.Min (range, targetingRange) * upgradeMul * ResearchMenu.rangeMul;
        else
            return Mathf.Min (range, targetingRange) * upgradeMul * ResearchMenu.rangeMul * 2.5f;
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

    public void OnInitializeToggleModSort (ModuleMod mod) {
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
                if (weapons[i].transform.parent.GetComponent<WeaponModule>().parentBase == this) {
                    if (weapons[i].bulletSpeed > speed) {
                        speed = weapons[i].bulletSpeed;
                        if (weapons[i].GetBulletData().isHitscan) {
                            speed = 100000;
                        }
                    }

                    if (!priorities.Contains(weapons[i].GetBulletData().effectiveAgainst)) {
                        priorities.Add(weapons[i].GetBulletData().effectiveAgainst);
                    }

                    if (weapons[i].maxRange < r) {
                        r = weapons[i].maxRange * weapons[i].upgradeMul * ResearchMenu.rangeMul;
                    }

                    weapons[i].damageMul = damageBoost;
                    weapons[i].firerateMul = (1f / firerateBoost);
                }
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

    public void OnToggleModSort (int newSort) {
        sortType = (TargetFinder.SortType)newSort;
    }

	public override void SetIsBeingPlaced () {
		base.SetIsBeingPlaced ();
		targetingRange = range;
	}
}
