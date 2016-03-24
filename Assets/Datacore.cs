using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Datacore : MonoBehaviour {

    public float numberDistance;
    public float numberSpeed;

    public Transform[] numbers;
    public float[] speeds;

    public GameObject[] prefabs;

    public List<GameObject> greenNunbers;
    public List<GameObject> redNumbers;

    public Material[] greenMaterials;
    public Material[] redMaterials;

    // Use this for initialization
    void Start () {
        StartCoroutine(InitializeNumbers());
	}

    IEnumerator InitializeNumbers () {
        numbers = new Transform[100];
        speeds = new float[100];

        for (int i = 0; i < numbers.Length; i++) {
            numbers[i] = ((GameObject)Instantiate(prefabs[Random.Range (0, prefabs.Length)], transform.position + transform.forward * numberDistance, Quaternion.Euler(0, 0, Random.Range(0, 360)))).transform;
            numbers[i].position = transform.position + Random.onUnitSphere * numberDistance;
            speeds[i] = Random.Range(numberSpeed - 2f, numberSpeed + 2f);
            yield return null;
        }
    }
	
	// Update is called once per frame
	void Update () {
	    for (int i = 0; i < numbers.Length; i++) {
            Transform loc = numbers[i];

            loc.position += loc.right * speeds[i] * Time.deltaTime;
            loc.LookAt(transform);
            loc.position += loc.forward * (Vector3.Distance (loc.position, transform.position) - numberDistance);

            if (loc.position.z > transform.position.z + numberDistance - 0.1f) {
                loc.position = transform.position + Random.onUnitSphere * numberDistance;
            }
        }
	}

    void OnDataTakeDamage (int damage) {
        for (int i = 0; i < damage; i++) {

        }
    }
}
