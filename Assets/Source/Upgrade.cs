using UnityEngine;
using System.Collections;

[System.Serializable]
public class Upgrade {

	public string upgradeName;
	public string upgradeFunction = "Upgrade";

	public int upgradeCost;
	public int upgradeAmount;
	public int maxUpgradeAmount = 1;
	public Sprite upgradeSprite;
	public int prerequisiteID = -1;
	public GameObject button;

	public void Purchase (GameObject buyer) {
		if (prerequisiteID >= 0) {
			Upgrade prerequisite = ResearchMenu.cur.upgrades[prerequisiteID];
			if (prerequisite.upgradeAmount >= prerequisite.maxUpgradeAmount) {
				HandlePurchase (buyer);
			}
		}else{
			HandlePurchase (buyer);
		}
	}

	void HandlePurchase (GameObject b) {
		upgradeAmount++;
		upgradeCost *= (int)((float)upgradeCost * 1.5f);
		b.SendMessage (upgradeFunction);
	}
}
