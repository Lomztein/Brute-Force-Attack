using UnityEngine;
using System.Collections;

public class AbilitySawmillLaunch : Ability {

	public Transform crosshair;
	public GameObject pointer;
	private bool hasClicked;
	public GameObject saw;
	
	// Update is called once per frame
	new void Update () {
		base.Update ();
		Vector3 mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		RotateAnimateCrosshair (crosshair, 2f);
		if (!hasClicked) transform.position = mousePos + Vector3.forward;
		if (Input.GetMouseButtonDown (0))
			OnClick ();

		if (hasClicked) {
			float angle = Angle.CalculateAngle (transform.position, mousePos);
			pointer.transform.rotation = Quaternion.Euler (0, 0, angle);
			if (Input.GetMouseButtonUp (0)) {
				LaunchSaw (angle);
			}
		}
	}

	void OnClick () {
		pointer.SetActive (true);
		hasClicked = true;
	}

	void LaunchSaw (float direction) {
		Instantiate (saw, transform.position, Quaternion.Euler (0, 0, direction));
		Destroy (gameObject);
		button.OnAbilityUsed ();
	}
}
