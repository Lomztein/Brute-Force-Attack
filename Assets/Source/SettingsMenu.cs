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
            cur.musicSlider.value = SettingsMenu.musicVolume;
        }

        if (PlayerPrefs.HasKey ("fSoundVolume")) {
            soundVolume = PlayerPrefs.GetFloat ("fSoundVolume");
            cur.soundSlider.value = SettingsMenu.soundVolume;
        }

        if (PlayerPrefs.HasKey ("bSoftBloom")) {
            softBloomEnabled = bool.Parse (PlayerPrefs.GetString ("bSoftBloom"));
            cur.UpdateBloom ();
        }
    }

    void OnDisable () {
        PlayerPrefs.SetFloat ("fMusicVolume", musicVolume);
        PlayerPrefs.SetFloat ("fSoundVolume", soundVolume);
        PlayerPrefs.SetString ("bSoftBloom", softBloomEnabled.ToString ());
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
