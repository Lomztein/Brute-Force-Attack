using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class AbilityBar : MonoBehaviour {

	public GameObject[] researchableAbilities;
	public RectTransform contentRect;

	public GameObject buttonPrefab;
	public float buttonSize = 50f;
	public float buttonHeight = 28f;

	private static AbilityBar instance;
	private static List<AbilityButton> currentButtons = new List<AbilityButton>();
	private static Vector3 contentRectPos;

	void Start () {
		instance = this;
		contentRectPos = contentRect.position;
		AddAbilityTest ();
	}

	void AddAbilityTest () {
		for (int i = 0; i < researchableAbilities.Length; i++) {
			AddAbility (i);
		}
	}

	public static void AddAbility (int index) {
		GameObject newButton = (GameObject)Instantiate (instance.buttonPrefab, new Vector3 (instance.buttonSize * currentButtons.Count + instance.buttonSize / 2f, instance.buttonHeight), Quaternion.identity);

		AbilityButton butt = newButton.GetComponent<AbilityButton>();
		butt.ability = instance.researchableAbilities[index];
		newButton.transform.SetParent (instance.contentRect.transform, false);
		instance.contentRect.position = contentRectPos;
		instance.contentRect.sizeDelta = new Vector2 (instance.buttonSize * currentButtons.Count, instance.buttonSize);
		currentButtons.Add (butt);
	}

}
