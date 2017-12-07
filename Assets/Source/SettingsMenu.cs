using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {

    public Bloom bloom;
    public Text toggleBloomText;
    public Slider musicSlider;
    public Slider soundSlider;

    public static float musicVolume;
    public static float soundVolume;
    public static bool softBloomEnabled = true;

    public static SettingsMenu cur;

    void Awake () {
        cur = this;
    }

	// Use this for initialization
	public static void LoadSettings () {
        if (PlayerPrefs.HasKey ("fMusicVolume")) {
            musicVolume = PlayerPrefs.GetFloat ("fMusicVolume");
        } else
            musicVolume = 1f;

        cur.musicSlider.value = musicVolume;

        if (PlayerPrefs.HasKey ("fSoundVolume")) {
            soundVolume = PlayerPrefs.GetFloat ("fSoundVolume");
        } else
            soundVolume = 1f;

        cur.soundSlider.value = soundVolume;

        if (PlayerPrefs.HasKey ("bSoftBloom")) {
            softBloomEnabled = bool.Parse (PlayerPrefs.GetString ("bSoftBloom"));
        } else
            softBloomEnabled = true;

        cur.UpdateBloom ();
        cur.OnDisable ();
    }

    void OnDisable () {
        PlayerPrefs.SetFloat ("fMusicVolume", musicVolume);
        PlayerPrefs.SetFloat ("fSoundVolume", soundVolume);
        PlayerPrefs.SetString ("bSoftBloom", softBloomEnabled.ToString ());
        PlayerPrefs.Save ();
    }

    // Update is called once per frame
    void Update () {
        musicVolume = musicSlider.value;
        soundVolume = soundSlider.value;
    }

    public void ToggleSoftBloom () {
        softBloomEnabled = !softBloomEnabled;
        UpdateBloom ();
    }

    public void UpdateBloom () {
        if (softBloomEnabled) {
            bloom.quality = Bloom.BloomQuality.High;
            toggleBloomText.text = "Bloom Mode: Soft";
        } else {
            bloom.quality = Bloom.BloomQuality.Cheap;
            toggleBloomText.text = "Bloom Mode: Sharp";
        }
    }
}
