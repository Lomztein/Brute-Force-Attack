using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Datastream : MonoBehaviour {

    public static Datastream cur;

	public static bool repurposeEnemies;
	public static bool enableFirewall;

	public static int healthAmount = 100;
    public const int STARTING_HEALTH = 100;
    public float lerpingHealth;
    public Transform datastreamGraphic;
    public Material datastreamMaterial;
    public ParticleSystem correptParticles;

    public GameObject numberPrefab;
    public DatastreamNumber[] numbers;
    public float flyDistance;
    public float flySpeed;
    public Vector3 start;

    public Texture2D[] numberTextures;
    public Material materialPrefab;

    public static int healPerWave = 0;

	void Start () {
        cur = this;
	}

    public void Initialize () {
        Game.currentScene = Scene.Play;
        datastreamGraphic.position = Vector3.down * (Game.game.battlefieldHeight / 2 - 3f);
        datastreamGraphic.localScale = new Vector3 (Game.game.battlefieldWidth, 6f, 1f);
        StartCoroutine (InitializeNumbers ());
    }

    void FixedUpdate () {
        lerpingHealth = Mathf.Lerp (lerpingHealth, healthAmount, 10 * Time.fixedDeltaTime);
        datastreamMaterial.SetFloat ("_DatastreamHealth", Mathf.Clamp01 (lerpingHealth / STARTING_HEALTH));
    }

    public void EmitCorruptionParticles (Vector3 pos, float multiplier) {
        correptParticles.transform.position = pos;
        correptParticles.transform.localScale = Vector3.one * multiplier;
        correptParticles.Emit (Mathf.RoundToInt (100 * multiplier));
        UpdateNumberMaterials ();
    }

    IEnumerator InitializeNumbers () {

        Rect rect = new Rect (-Game.game.battlefieldWidth / 2, -Game.game.battlefieldHeight / 2 + 6, Game.game.battlefieldWidth / 2, -Game.game.battlefieldHeight / 2);
        Game.ChangeWalls (rect, Game.WallType.Unbuildable);

        numbers = new DatastreamNumber[100];

        for (int i = 0; i < numbers.Length; i++) {
            GameObject newNumber = (GameObject)Instantiate (numberPrefab);
            DatastreamNumber num = newNumber.GetComponent<DatastreamNumber> ();
            numbers[i] = num;

            num.transform.parent = transform;
            num.datastream = this;
            num.ResetNumber ();

            num.materialInstance = Instantiate (materialPrefab);
            num.materialInstance.SetFloat ("_Index", (float)i / numbers.Length);
            num.materialInstance.SetTexture ("_MainTex", numberTextures[Random.Range (0, numberTextures.Length)]);
            num.materialInstance.SetFloat ("_DatastreamHealth", Mathf.Clamp01 ((float)healthAmount / STARTING_HEALTH));
            num.renderer.material = num.materialInstance;

            yield return new WaitForEndOfFrame ();
        }
    }

    void UpdateNumberMaterials () {
        for (int i = 0; i < numbers.Length; i++) {
            numbers[i].materialInstance.SetFloat ("_DatastreamHealth", (float)healthAmount / STARTING_HEALTH);
        }
    }
}
