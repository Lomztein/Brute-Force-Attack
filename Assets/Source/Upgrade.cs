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

	public void Purchase (GameObject buyer) {
		HandlePurchase (buyer);
	}

	void HandlePurchase (GameObject b) {
		upgradeAmount++;
		upgradeCost *= (int)((float)upgradeCost * 1.5f);
		b.SendMessage (upgradeFunction);
	}
}
