using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenericEnemy : MonoBehaviour {

	public float speed;
	public int health;
	public int damage;

	public bool rotateSprite;

	// TODO Add pathfinding and, well, just improve overall

	// Update is called once per frame
	void Update () {

		transform.position += Vector3.down * speed * Time.deltaTime;

		if (rotateSprite)
			transform.rotation = Quaternion.Euler (0,0,Angle.CalculateAngle (transform.position, transform.position + Vector3.down));
	
	}

	void OnTakeDamage (int d) {
		health -= d;
		if (health < 0)
			Destroy (gameObject);

	}

	void OnCollisionEnter (Collision col) {
		
		Datastream stream = col.gameObject.GetComponent<Datastream>();
		List<Transform> nearest = new List<Transform>();

		for (int j = 0; j < damage; j++) {

			float dist = float.MaxValue;
			Transform near = null;
		
			for (int i = 0; i < stream.pooledNumbers.Count; i++) {

				float d = Vector3.Distance (transform.position, stream.pooledNumbers[i].transform.position);
				if (d < dist) {
					dist = d;
					near = stream.pooledNumbers[i].transform;
				}
			}

			if (near) {
				stream.pooledNumbers.Remove(near.gameObject);
				nearest.Add (near);
			}

		}

		for (int i = 0; i < nearest.Count; i++) {
			nearest[i].GetComponent<SpriteRenderer>().color = Color.red;
		}

		Destroy (gameObject);
	}
}
