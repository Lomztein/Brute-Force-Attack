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

    public static int healPerWave = 0;

	void Start () {
        cur = this;
	}

    public void Initialize () {
        Game.currentScene = Scene.Play;
        datastreamGraphic.position = Vector3.down * (Game.game.battlefieldHeight / 2 - 3f);
        datastreamGraphic.localScale = new Vector3 (Game.game.battlefieldWidth, 6f, 1f);
    }

    void FixedUpdate () {
        lerpingHealth = Mathf.Lerp (lerpingHealth, healthAmount, 10 * Time.fixedDeltaTime);
        datastreamMaterial.SetFloat ("_DatastreamHealth", lerpingHealth / STARTING_HEALTH);
    }

    public void EmitCorruptionParticles (Vector3 pos, float multiplier) {
        correptParticles.transform.position = pos;
        correptParticles.transform.localScale = Vector3.one * multiplier;
        correptParticles.Emit (Mathf.RoundToInt (100 * multiplier));
    }
}
