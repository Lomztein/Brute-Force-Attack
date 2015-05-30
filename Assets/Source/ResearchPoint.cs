using UnityEngine;
using System.Collections;

public class ResearchPoint : MonoBehaviour {

	public float researchValue;
	private bool isCapturing;
	public ParticleSystem[] particle;
	
	public void Capture (float multiplier, float speed) {
		if (!isCapturing) {
			isCapturing = true;
			StartCoroutine (DoCapture (multiplier, speed));
		}
	}

	IEnumerator DoCapture (float multiplier, float speed) {
		float time = researchValue * multiplier / speed;

		float[] particlesMax = new float[particle.Length];
		for (int i = 0; i < particle.Length; i++) {
			particlesMax [i] = particle [i].emissionRate;
		}

		float startTime = time;

		while (time > 0) {
			Game.researchProgress += speed * Time.fixedDeltaTime;
			time -= Time.fixedDeltaTime;
			for (int i = 0; i < particle.Length; i++) {
				particle [i].emissionRate = time / startTime * particlesMax[i];
			}
			yield return new WaitForFixedUpdate ();
		}

		Destroy (gameObject);
		yield return null;
	}

	void OnMouseDown () {
		Capture (1, 1);
	}
}
