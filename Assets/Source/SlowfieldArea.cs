using UnityEngine;
using System.Collections;

public class SlowfieldArea : MonoBehaviour {

	public float range;
	public float time;
	private float timeAlive;
	new public ParticleSystem particleSystem;
	public Transform waveThingie;
	public float slowAmount;

	public AnimationCurve dieCurve;
	public float dieTime;

	private bool isAlive = false;

	public static float staticMultiplier = 1f;

	// Use this for initialization
	void Start () {
		if (time > 0.1f) Invoke ("FuckThisNoise", time);
		isAlive = false;
		StartCoroutine (Die (true));
	}

	void FuckThisNoise () {
		isAlive = false;
		StartCoroutine (Die (false));
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (isAlive) {
			if (time > 0.1f) {
				Utility.ChangeParticleEmmisionRate (particleSystem, Mathf.Lerp (5, 0, timeAlive / time));
				particleSystem.startLifetime = Mathf.Lerp (3, 0, (timeAlive - 1) / time);
				timeAlive += Time.fixedDeltaTime;
			}
			Collider[] nearby = Physics.OverlapSphere (new Vector3 (transform.position.x, transform.position.y), range, Game.game.enemyLayer);
			for (int i = 0; i < nearby.Length; i++) {
				nearby [i].GetComponent<Enemy> ().freezeMultiplier = slowAmount * staticMultiplier;
			}
			waveThingie.transform.localScale = Vector3.one * (range + Mathf.Sin (Time.time) / 3);
		}
	}

	// This is more or less fucked in the arse.
	IEnumerator Die (bool reverse) {
		float t = 0f;
		bool isDying = true;
		while (isDying) {
			t += Time.fixedDeltaTime / dieTime;
			if (reverse) {
				waveThingie.localScale = Vector3.one * dieCurve.Evaluate (1-t) * range;
				if (waveThingie.localScale.x > range - 0.1f) {
					isDying = false;
					isAlive = true;
				}
			}else{
				waveThingie.localScale = Vector3.one * dieCurve.Evaluate (t) * range;
				if (waveThingie.localScale.magnitude < 0.1f) {
					Destroy (gameObject);
				}
			}
			yield return new WaitForFixedUpdate ();
		}
	}
}
