using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AbilityButton : MonoBehaviour {

	public GameObject ability;
	public Slider slider;

	private Button button;
	private float timeRemaining;
	private Ability abilityData;
	private GameObject activeAbility;

	public Image image;
	public HoverContextElement hoverContext;

	// Use this for initialization
	void Start () {
		button = GetComponent<Button>();
		abilityData = ability.GetComponent<Ability>();
		hoverContext.text = abilityData.abilityName + ", " + abilityData.abilityCost + " Credits";
		if (abilityData.abilitySprite) image.sprite = abilityData.abilitySprite;
		slider.value = 0f;
	}

	public void OnClicked () {
		button.interactable = false;
		activeAbility = PlayerInput.cur.SelectAbilty (ability, this);
	}

	public void OnAbilityUsed () {
		button.interactable = false;
		StartCoroutine (Cooldown ());
	}

	IEnumerator Cooldown () {
		slider.maxValue = abilityData.cooldownTime;
		timeRemaining = abilityData.cooldownTime;
		while (timeRemaining > 0f) {
			timeRemaining -= Time.fixedDeltaTime;
			slider.value = timeRemaining;
			yield return new WaitForFixedUpdate ();
		}
		button.interactable = true;
	}

	void Update () {
		if (!activeAbility && timeRemaining <= 0f) {
			OnAbilityCanceled ();
		}
	}

	public void OnAbilityCanceled () {
		button.interactable = true;
		if (activeAbility) 
			Destroy (activeAbility);
	}
}
