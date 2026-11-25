using UnityEngine;
using UnityEngine.UI;

// Puts my name on the title and end screens, and shows the names of Congo rivers during the trip.
// The trip is cut into parts. Each part has a river name, a banner and a wooden sign on the bank.
public class RiverNames : MonoBehaviour
{
	public RiverGameManager manager;
	public string author = "Joel Kalala";
	public string[] rivers = { "Fleuve Congo", "Mai-Ndombe", "Kasai", "Ubangi", "Lualaba", "Lomami" };
	public float bannerSeconds = 3.5f;

	Text banner;
	Text bannerSmall;
	Text currentRiver;
	Text endCredit;
	int section = -1;
	int nextSign = 1;
	float bannerTimer;
	Font font;

	public string CurrentRiverName { get { return section >= 0 ? rivers[section] : ""; } }

	float SectionLength { get { return manager.villageDistance / rivers.Length; } }

	void Start()
	{
		font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

		// Title screen: "Made by Joel Kalala"
		Text credit = MakeText(manager.titlePanel.transform, "Made By", "Made by " + author, 30, new Vector2(0.5f, 0.5f), new Vector2(0, -295));
		credit.color = new Color(1f, 0.95f, 0.75f);
		credit.fontStyle = FontStyle.Italic;

		// End screen
		endCredit = MakeText(manager.endPanel.transform, "Made By", "A game by " + author, 26, new Vector2(0.5f, 0.5f), new Vector2(0, -160));
		endCredit.color = new Color(1, 1, 1, 0.8f);

		// Banner when you enter a new river
		bannerSmall = MakeText(manager.hud.transform, "River Banner Small", "You are on the", 24, new Vector2(0.5f, 1f), new Vector2(0, -175));
		banner = MakeText(manager.hud.transform, "River Banner", "", 52, new Vector2(0.5f, 1f), new Vector2(0, -218));
		banner.fontStyle = FontStyle.Bold;

		// The river name in the trip panel (between Market and Village)
		Transform trip = manager.progressTrack.parent;
		currentRiver = MakeText(trip, "Current River", "", 20, new Vector2(0.5f, 1f), new Vector2(0, -19));
		currentRiver.color = new Color(1f, 0.85f, 0.35f);

		SetBannerAlpha(0);
	}

	void Update()
	{
		if (!manager.IsPlaying)
		{
			if (manager.CurrentState != RiverGameManager.State.Title)
				SetBannerAlpha(0);
			return;
		}

		// Which river are we on?
		int now = Mathf.Clamp((int)(manager.distance / SectionLength), 0, rivers.Length - 1);
		if (now != section)
		{
			section = now;
			banner.text = rivers[section];
			currentRiver.text = rivers[section];
			bannerTimer = bannerSeconds;
		}

		// Banner fades in and out
		if (bannerTimer > 0)
		{
			bannerTimer -= Time.deltaTime;
			float t = bannerSeconds - bannerTimer;
			SetBannerAlpha(Mathf.Clamp01(Mathf.Min(t / 0.4f, bannerTimer / 0.8f)));
		}
		else
			SetBannerAlpha(0);

		// A wooden sign on the bank, placed so you pass it when the new river starts
		RiverSpawner spawner = GetComponent<RiverSpawner>();
		if (nextSign < rivers.Length && manager.distance >= nextSign * SectionLength - spawner.spawnZ)
		{
			MakeSign(rivers[nextSign], nextSign * SectionLength - manager.distance);
			nextSign++;
		}
	}

	void SetBannerAlpha(float a)
	{
		banner.color = new Color(1f, 0.82f, 0.2f, a);
		bannerSmall.color = new Color(1f, 1f, 1f, a);
	}

	void MakeSign(string riverName, float z)
	{
		GameObject sign = new GameObject("River Sign " + riverName);
		sign.transform.position = new Vector3(-7.4f, 0, z);
		Color wood = new Color(0.5f, 0.3f, 0.15f);
		Part(sign, new Vector3(-1.3f, 1.2f, 0), new Vector3(0.2f, 2.4f, 0.2f), wood);
		Part(sign, new Vector3(1.3f, 1.2f, 0), new Vector3(0.2f, 2.4f, 0.2f), wood);
		Part(sign, new Vector3(0, 2.3f, 0), new Vector3(3.6f, 1f, 0.12f), new Color(0.85f, 0.7f, 0.4f));

		GameObject label = new GameObject("Name");
		label.transform.SetParent(sign.transform, false);
		label.transform.localPosition = new Vector3(0, 2.3f, -0.08f);
		TextMesh text = label.AddComponent<TextMesh>();
		text.text = riverName;
		text.font = font;
		text.fontSize = 64;
		text.characterSize = 0.07f;
		text.anchor = TextAnchor.MiddleCenter;
		text.color = new Color(0.25f, 0.12f, 0.05f);
		label.GetComponent<MeshRenderer>().sharedMaterial = font.material;

		// Turn a little toward the river so it is easy to read
		sign.transform.rotation = Quaternion.Euler(0, -25f, 0);

		RiverObject mover = sign.AddComponent<RiverObject>();
		mover.kind = RiverObject.Kind.Decoration;
		mover.manager = manager;
	}

	static void Part(GameObject parent, Vector3 pos, Vector3 scale, Color color)
	{
		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
		Destroy(go.GetComponent<Collider>());
		go.transform.SetParent(parent.transform, false);
		go.transform.localPosition = pos;
		go.transform.localScale = scale;
		go.GetComponent<Renderer>().material.color = color;
	}

	Text MakeText(Transform parent, string name, string value, int size, Vector2 anchor, Vector2 position)
	{
		GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Outline));
		RectTransform rt = (RectTransform)go.transform;
		rt.SetParent(parent, false);
		rt.anchorMin = anchor;
		rt.anchorMax = anchor;
		rt.pivot = new Vector2(0.5f, 0.5f);
		rt.sizeDelta = new Vector2(900, size + 20);
		rt.anchoredPosition = position;
		Text text = go.GetComponent<Text>();
		text.font = font;
		text.text = value;
		text.fontSize = size;
		text.alignment = TextAnchor.MiddleCenter;
		text.color = Color.white;
		text.horizontalOverflow = HorizontalWrapMode.Overflow;
		text.verticalOverflow = VerticalWrapMode.Overflow;
		text.raycastTarget = false;
		go.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0.8f);
		return text;
	}
}
