using UnityEngine;

// Music and sound effects. The anthem plays when the trip starts.
// The small sound effects are made in code, so no extra files are needed.
public class RiverSound : MonoBehaviour
{
	[Header("Music")]
	public AudioClip anthem;            // Debout Congolais (US Navy Band recording)
	[Range(0, 1)] public float musicVolume = 0.55f;
	[Range(0, 1)] public float riverVolume = 0.12f;
	[Range(0, 1)] public float effectsVolume = 0.8f;

	AudioSource music;
	AudioSource river;
	AudioSource effects;
	AudioClip boxClip, hitClip, splashClip, winClip, loseClip, riverClip;
	float musicTarget;

	public bool MusicPlaying { get { return music != null && music.isPlaying; } }

	void Awake()
	{
		// The anthem file lives in Assets/Resources/Anthem.ogg
		if (anthem == null)
			anthem = Resources.Load<AudioClip>("Anthem");

		music = gameObject.AddComponent<AudioSource>();
		music.loop = true;
		music.playOnAwake = false;
		river = gameObject.AddComponent<AudioSource>();
		river.loop = true;
		effects = gameObject.AddComponent<AudioSource>();

		boxClip = Tones("Box", new[] { 880f, 1320f }, 0.09f, 0.35f);
		hitClip = Crash("Hit", 0.35f, 90f);
		splashClip = Noise("Splash", 0.5f, 0.25f);
		winClip = Tones("Win", new[] { 523f, 659f, 784f, 1047f }, 0.16f, 0.4f);
		loseClip = Tones("Lose", new[] { 392f, 330f, 262f }, 0.25f, 0.4f);
		riverClip = Noise("River", 2f, 0.03f);

		river.clip = riverClip;
		river.volume = riverVolume;
		river.Play();
	}

	void Update()
	{
		// Smooth fade for the music
		music.volume = Mathf.MoveTowards(music.volume, musicTarget, Time.unscaledDeltaTime * 0.5f);
		if (musicTarget == 0 && music.volume == 0 && music.isPlaying)
			music.Stop();
	}

	public void StartMusic()
	{
		if (anthem == null)
			return;
		music.clip = anthem;
		music.volume = musicVolume;
		musicTarget = musicVolume;
		music.Play();
	}

	public void Box() { effects.PlayOneShot(boxClip, effectsVolume); }
	public void Hit() { effects.PlayOneShot(hitClip, effectsVolume); }
	public void Splash() { effects.PlayOneShot(splashClip, effectsVolume * 0.7f); }

	public void End(bool won)
	{
		musicTarget = 0;   // the anthem fades out
		effects.PlayOneShot(won ? winClip : loseClip, effectsVolume);
	}

	// ---------- Sounds made in code ----------
	const int Rate = 44100;

	// A few short beeps one after another (pickup, win, lose)
	static AudioClip Tones(string name, float[] notes, float noteLength, float volume)
	{
		int perNote = (int)(Rate * noteLength);
		float[] data = new float[perNote * notes.Length];
		for (int n = 0; n < notes.Length; n++)
			for (int i = 0; i < perNote; i++)
			{
				float t = (float)i / Rate;
				float fade = 1f - (float)i / perNote;
				data[n * perNote + i] = Mathf.Sin(2 * Mathf.PI * notes[n] * t) * fade * volume;
			}
		return Make(name, data);
	}

	// Wood cracking: low thump plus noise
	static AudioClip Crash(string name, float length, float pitch)
	{
		int count = (int)(Rate * length);
		float[] data = new float[count];
		System.Random random = new System.Random(3);
		for (int i = 0; i < count; i++)
		{
			float t = (float)i / Rate;
			float fade = Mathf.Pow(1f - (float)i / count, 2);
			float thump = Mathf.Sin(2 * Mathf.PI * pitch * t * (1.5f - t)) * 0.7f;
			float crack = ((float)random.NextDouble() * 2f - 1f) * 0.5f;
			data[i] = (thump + crack) * fade * 0.6f;
		}
		return Make(name, data);
	}

	// Smoothed noise: splash (short) or flowing water (long, loops)
	static AudioClip Noise(string name, float length, float volume)
	{
		int count = (int)(Rate * length);
		float[] data = new float[count];
		System.Random random = new System.Random(5);
		float smooth = 0;
		bool loop = length > 1f;
		for (int i = 0; i < count; i++)
		{
			smooth = Mathf.Lerp(smooth, (float)random.NextDouble() * 2f - 1f, 0.15f);
			float fade = loop ? 1f : 1f - (float)i / count;
			data[i] = smooth * fade * volume * 4f;
		}
		return Make(name, data);
	}

	static AudioClip Make(string name, float[] data)
	{
		AudioClip clip = AudioClip.Create(name, data.Length, 1, Rate, false);
		clip.SetData(data, 0);
		return clip;
	}
}
