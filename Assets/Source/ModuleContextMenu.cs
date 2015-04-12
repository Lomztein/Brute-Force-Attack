using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ModuleContextMenu : MonoBehaviour {

	public Image moduleImage;
	public Text moduleName;
	public Text moduleDesc;
	public GameObject upgradeButton;

	private Module module;

	// Update is called once per frame
	void Update () {
		if (!module)
			ExitMenu ();

		if (Input.GetButtonDown ("Cancel"))
			ExitMenu ();
	}

	public void SellModule () {
		Destroy (module.gameObject);

	}

	public void ExitMenu () {
		gameObject.SetActive (false);
	}

	public void OpenModule (Module module) {
		this.module = module;
		moduleImage.sprite = module.transform.FindChild ("Sprite").GetComponent<SpriteRenderer>().sprite;
		moduleName.text = module.moduleName;
		moduleDesc.text = "A class " + module.moduleClass + " " + module.moduleType.ToString () + " module - " + module.moduleDesc;

		GameObject butt = upgradeButton;
		for (int i = 0; i < module.upgrades.Length; i++) {
			if (i != 0)
				butt = (GameObject)Instantiate (upgradeButton, upgradeButton.transform.position + Vector3.right * 98 * (i+1), Quaternion.identity);

			Button b = butt.GetComponent<Button>();
			AddListenerToUpgradeButton (b, module.upgrades[i]);

			Image im = butt.transform.FindChild ("Image").GetComponent<Image>();
			im.sprite = module.upgrades[i].upgradeSprite;
			Text te = butt.transform.FindChild ("UpgradeDesc").GetComponent<Text>();
			te.text = module.upgrades[i].upgradeName;
			Text co = butt.transform.FindChild ("UpgradeCost").GetComponent<Text>();
			co.text = module.upgrades[i].upgradeCost.ToString ();
		}
	}

	void AddListenerToUpgradeButton (Button button, Upgrade upgrade) {
		button.onClick.AddListener (() => {
			module.SendMessage (upgrade.upgradeFunction);
		});
	}
}
