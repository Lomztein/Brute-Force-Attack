using UnityEngine;
using System.Collections;

public class Ability : MonoBehaviour {

	public string abilityName;
	public int abilityCost;
	public float cooldownTime;
	public Sprite abilitySprite;
	public AbilityButton button;

	public void Update () {
		if (Input.GetMouseButton (1))
			button.OnAbilityCanceled ();
	}

	public void RotateAnimateCrosshair (Transform crosshair, float size) {
		crosshair.transform.localScale = Vector3.one * size + Vector3.one * Mathf.Sin (Time.time * 5f) * 0.2f;
		crosshair.Rotate (0, 0, 80 * Time.deltaTime);
	}

}