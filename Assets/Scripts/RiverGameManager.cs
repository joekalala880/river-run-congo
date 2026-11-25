using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Runs the whole trip: the sun timer, the distance to the village, supplies, canoe health, win and lose.
public class RiverGameManager : MonoBehaviour
{
	public enum State { Title, Playing, Won, Lost }

	[Header("Rules")]
	public float sunsetSeconds = 180f;      // the sun is the timer: about 3 minutes from afternoon to dark
	public float villageDistance = 2400f;   // meters from the market to the village dock
	public int boxesNeeded = 10;
	public int maxHealth = 3;

	[Header("World")]
	public Canoe canoe;
	public Light sun;
	public Camera cam;
	public WaterScroller water;
	public RiverSound sound;

	[Header("UI")]
	public GameObject titlePanel;
	public GameObject hud;
	public Text boxesText;
	public Image[] hearts;
	public RectTransform progressMarker;
	public RectTransform progressTrack;
	public RectTransform sunIcon;
	public Image sunIconImage;
	public RectTransform energyFill;
	public Image energyFillImage;
	public Text warningText;
	public GameObject endPanel;
	public Text endTitle;
	public Text endText;

	// These are public so the play-mode checks can set them
	[HideInInspector] public float elapsed;
	[HideInInspector] public float distance;

	public State CurrentState { get; private set; }
	public int Boxes { get; private set; }
	public int Health { get; private set; }
	public bool IsPlaying { get { return CurrentState == State.Playing; } }
	public float SunAmount { get { return Mathf.Clamp01(elapsed / sunsetSeconds); } }   // 0 = afternoon, 1 = dark
	public float Remaining { get { return villageDistance - distance; } }

	// How fast the world moves toward the canoe right now
	public float Speed { get { return IsPlaying ? canoe.ForwardSpeed : 0f; } }

	static readonly Color dayLight = new Color(1f, 0.96f, 0.86f);
	static readonly Color orangeLight = new Color(1f, 0.6f, 0.28f);
	static readonly Color redLight = new Color(0.75f, 0.22f, 0.18f);
	static readonly Color daySky = new Color(0.55f, 0.78f, 0.95f);
	static readonly Color orangeSky = new Color(0.98f, 0.58f, 0.32f);
	static readonly Color nightSky = new Color(0.09f, 0.05f, 0.14f);

	void Awake()
	{
		// Music and sound effects
		if (sound == null)
			sound = GetComponent<RiverSound>();
		if (sound == null)
			sound = gameObject.AddComponent<RiverSound>();

		// My name on the screens and the Congo river names during the trip
		if (GetComponent<RiverNames>() == null)
			gameObject.AddComponent<RiverNames>().manager = this;
	}

	void Start()
	{
		Time.timeScale = 1f;
		Health = maxHealth;
		CurrentState = State.Title;
		titlePanel.SetActive(true);
		hud.SetActive(false);
		endPanel.SetActive(false);
		sound.StartMusic();   // the national anthem plays from the title screen and through the trip
		UpdateSun();
		UpdateUI();
	}

	void Update()
	{
		if (CurrentState == State.Title)
		{
			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
				StartGame();
			return;
		}

		if (CurrentState != State.Playing)
		{
			if (Input.GetKeyDown(KeyCode.R))
				PlayAgain();
			return;
		}

		elapsed += Time.deltaTime;
		distance += Speed * Time.deltaTime;

		UpdateSun();

		// Lose: the sun went down before you got home
		if (elapsed >= sunsetSeconds)
		{
			Lose("NIGHT FELL", "It got dark before you reached the village.\nSupplies: " + Boxes + " / " + boxesNeeded);
			return;
		}

		// Reached the village dock
		if (distance >= villageDistance)
		{
			distance = villageDistance;
			if (Boxes >= boxesNeeded)
				Win();
			else
				Lose("NOT ENOUGH SUPPLIES", "You got home, but you only brought " + Boxes + " / " + boxesNeeded + " boxes.");
			return;
		}

		UpdateUI();
	}

	public void StartGame()
	{
		if (CurrentState != State.Title)
			return;
		CurrentState = State.Playing;
		titlePanel.SetActive(false);
		hud.SetActive(true);
		UpdateUI();
	}

	public void CollectBox()
	{
		if (!IsPlaying)
			return;
		Boxes++;
		sound.Box();
		UpdateUI();
	}

	public void Damage()
	{
		if (!IsPlaying)
			return;
		Health--;
		sound.Hit();
		UpdateUI();
		if (Health <= 0)
			Lose("YOUR CANOE BROKE", "The river was too dangerous.\nSupplies: " + Boxes + " / " + boxesNeeded);
	}

	void Win()
	{
		CurrentState = State.Won;
		sound.End(true);
		int seconds = (int)elapsed;
		ShowEnd("YOU MADE IT HOME!", "The village has food and medicine.\nSupplies: " + Boxes + " / " + boxesNeeded +
			"\nTrip time: " + seconds / 60 + ":" + (seconds % 60).ToString("00"), new Color(0.55f, 1f, 0.55f));
	}

	void Lose(string title, string text)
	{
		CurrentState = State.Lost;
		sound.End(false);
		ShowEnd(title, text, new Color(1f, 0.45f, 0.4f));
	}

	void ShowEnd(string title, string text, Color color)
	{
		UpdateUI();
		endTitle.text = title;
		endTitle.color = color;
		endText.text = text + "\n\nPress R to play again";
		endPanel.SetActive(true);
		warningText.text = "";
	}

	public void PlayAgain()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	// The sun goes down, the light turns orange and then red, and the sky gets dark
	void UpdateSun()
	{
		float t = SunAmount;
		Color light = t < 0.5f ? dayLight : t < 0.8f ? Color.Lerp(dayLight, orangeLight, (t - 0.5f) / 0.3f) : Color.Lerp(orangeLight, redLight, (t - 0.8f) / 0.2f);
		Color sky = t < 0.45f ? daySky : t < 0.8f ? Color.Lerp(daySky, orangeSky, (t - 0.45f) / 0.35f) : Color.Lerp(orangeSky, nightSky, (t - 0.8f) / 0.2f);

		sun.color = light;
		sun.intensity = Mathf.Lerp(1.2f, 0.25f, Mathf.InverseLerp(0.6f, 1f, t));
		sun.transform.rotation = Quaternion.Euler(Mathf.Lerp(55f, 2f, t), -30f, 0f);
		RenderSettings.ambientLight = Color.Lerp(new Color(0.62f, 0.62f, 0.62f), new Color(0.12f, 0.09f, 0.16f), t * t);
		cam.backgroundColor = sky;
		RenderSettings.fogColor = sky;
	}

	void UpdateUI()
	{
		boxesText.text = "Supplies: " + Boxes + " / " + boxesNeeded;
		boxesText.color = Boxes >= boxesNeeded ? new Color(0.55f, 1f, 0.55f) : Color.white;

		for (int i = 0; i < hearts.Length; i++)
			hearts[i].color = i < Health ? new Color(0.95f, 0.2f, 0.2f) : new Color(0.25f, 0.25f, 0.25f, 0.6f);

		// Canoe marker moves from the market to the village
		float progress = Mathf.Clamp01(distance / villageDistance);
		float width = progressTrack.rect.width;
		progressMarker.anchoredPosition = new Vector2(progress * width, progressMarker.anchoredPosition.y);

		// Sun icon slides down an arc from the top to the horizon
		float t = SunAmount;
		sunIcon.anchoredPosition = new Vector2(Mathf.Lerp(-150f, 150f, t), Mathf.Sin((1f - t) * Mathf.PI * 0.5f) * 50f - 50f);
		sunIconImage.color = sun.color;

		// Paddle energy bar
		float energy = canoe.Energy / canoe.maxEnergy;
		energyFill.anchorMax = new Vector2(energy, 1);
		energyFillImage.color = canoe.CanBoost ? new Color(0.3f, 0.85f, 1f) : new Color(0.45f, 0.45f, 0.5f);

		// No number timer, but a warning when it is getting late
		if (IsPlaying && t > 0.75f)
		{
			warningText.text = "The sun is going down. Hurry!";
			warningText.color = new Color(1f, 0.6f, 0.3f, 0.6f + 0.4f * Mathf.PingPong(Time.time * 2f, 1f));
		}
		else
			warningText.text = "";
	}
}
