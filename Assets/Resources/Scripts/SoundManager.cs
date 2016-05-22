using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {

	public AudioSource sfxSource;
	public AudioSource musicSource;
	public static SoundManager instance = null;

	public float lowPitchRange = 1;
	public float highPitchRange = 1.1f;
	
	void Awake () {
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);

		DontDestroyOnLoad(gameObject);
	}

	public void PlaySingle (AudioClip clip) {
		float randomPitch = Random.Range(lowPitchRange, highPitchRange);

		sfxSource.pitch = randomPitch;
		sfxSource.clip = clip;

		if (clip.name == "jewel_pick_up") {
			sfxSource.volume = 0.4f;
		} else {
			sfxSource.volume = 1;
		}

		sfxSource.Play();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
