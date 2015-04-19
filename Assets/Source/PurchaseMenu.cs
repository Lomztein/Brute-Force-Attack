using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class PurchaseMenu : MonoBehaviour {

	private float targetHeight = -Screen.height / 2 - 120;

	public bool isOpen;
	public RectTransform rect;

	public GameObject[] purchaseables;
	public RectTransform firstTopButton;
	public RectTransform firstButtomButton;

	public GameObject buttonPrefab;
	public Transform buttonMask;

	public RectTransform scrollThingie;
	public PlayerInput playerInput;

	// Use this for initialization
	// TODO Implement multiple raycasts before placing objects.
	// TODO Implement "collision check points" on structural modules.

	public void InitializePurchaseMenu () {

		// Count weapons and other types
		int w = 0;
		int o = 0;

		// Itereate through each purchaseable, and instantiate a button for it.
		for (int i = 0; i < purchaseables.Length; i++ ) {

			Module m = purchaseables[i].GetComponent<Module>();

			// Instantiate weapon buttons on lower button row,
			// and all other types on top row.

			GameObject newButton = null;
			if (m.moduleType == Module.Type.Weapon) {
				newButton = (GameObject)Instantiate (buttonPrefab, firstButtomButton.position + Vector3.right * 94 * w, Quaternion.identity);
				w++;
			}else{
				newButton = (GameObject)Instantiate (buttonPrefab, firstTopButton.position + Vector3.right * 94 * o, Quaternion.identity);
				o++;
			}
			Button button = newButton.GetComponent<Button>();
			newButton.transform.GetChild (0).GetComponent<Image>().sprite = purchaseables[i].transform.FindChild ("Sprite").GetComponent<SpriteRenderer>().sprite;
			newButton.transform.SetParent (buttonMask, true);
			newButton.transform.localScale = Vector3.one;
			AddPurchaseButtonListener (button, i);

		}

		int max = Mathf.Max (w,o);
		int newX = max * 94 + 30;
		scrollThingie.sizeDelta = new Vector2 (newX, scrollThingie.sizeDelta.y);
		scrollThingie.localPosition += new Vector3 (scrollThingie.localPosition.x + newX/4f, scrollThingie.localPosition.y);
	
	}

	void AddPurchaseButtonListener (Button button, int index) {
		button.onClick.AddListener (() => {
			SelectPurchaseable (index);
		});

		Module m = purchaseables[index].GetComponent<Module>();
		button.GetComponent<HoverContextElement>().text = m.moduleName + ", " + m.moduleCost.ToString () + " LoC";
	}

	public void SelectPurchaseable (int index) {
		playerInput.SelectPurchaseable (purchaseables[index]);
	}

	// Update is called once per frame
	void Update () {

		// Animate closing and opening
		rect.localPosition = new Vector3 (rect.localPosition.x, Mathf.Lerp (rect.localPosition.y, targetHeight, 30f * Time.deltaTime));

		if (isOpen && Input.mousePosition.y > 300) {
			isOpen = false;
			targetHeight = -Screen.height / 2 - 120;
		}

		if (Input.mousePosition.y < 30) {
			isOpen = true;
			targetHeight = -Screen.height / 2 + 150;
		}
	
	}
}
