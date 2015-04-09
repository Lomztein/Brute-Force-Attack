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
		buyer.SendMessage (upgradeFunction);
	}

}
