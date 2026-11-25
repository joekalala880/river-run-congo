using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Tools > River Run > Build Game: makes the materials, prefabs, scene and UI for River Run: Congo.
public static class RiverBuilder
{
	static Font font;

	[MenuItem("Tools/River Run/Build Game")]
	public static void Build()
	{
		Folder("Assets", "Materials");
		Folder("Assets", "Textures");
		Folder("Assets", "Prefabs");
		Folder("Assets", "Scenes");
		font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

		// ---------- Materials ----------
		Texture2D waterTex = SaveTexture(MakeWaterTexture(), "Assets/Textures/Water.asset");
		Texture2D arrowTex = SaveTexture(MakeArrowTexture(), "Assets/Textures/FastWaterArrows.asset");

		Material water = Mat("Water", new Color(0.62f, 0.66f, 0.6f), 0.75f);
		water.mainTexture = waterTex;
		water.mainTextureScale = new Vector2(1, 12);
		Material fast = new Material(Shader.Find("Legacy Shaders/Transparent/Diffuse"));
		fast.mainTexture = arrowTex;
		fast.mainTextureScale = new Vector2(2.5f, 1);
		fast.color = Color.white;
		SaveMat(fast, "FastWater");

		Material grass = Mat("Grass", new Color(0.25f, 0.5f, 0.2f), 0.1f);
		Material mud = Mat("Mud", new Color(0.45f, 0.33f, 0.2f), 0.1f);
		Material wood = Mat("Wood", new Color(0.5f, 0.3f, 0.15f), 0.2f);
		Material darkWood = Mat("DarkWood", new Color(0.3f, 0.18f, 0.09f), 0.2f);
		Material rock = Mat("Rock", new Color(0.45f, 0.45f, 0.47f), 0.15f);
		Material hippo = Mat("Hippo", new Color(0.5f, 0.4f, 0.45f), 0.35f);
		Material hippoPink = Mat("HippoPink", new Color(0.85f, 0.55f, 0.55f), 0.3f);
		Material white = Mat("White", Color.white, 0.4f);
		Material black = Mat("Black", new Color(0.05f, 0.05f, 0.05f), 0.4f);
		Material box = Mat("SupplyBox", new Color(1f, 0.72f, 0.15f), 0.3f);
		Material boxBand = Mat("SupplyBand", new Color(0.75f, 0.1f, 0.1f), 0.3f);
		Material leaves = Mat("Leaves", new Color(0.15f, 0.42f, 0.15f), 0.1f);
		Material skin = Mat("Skin", new Color(0.36f, 0.22f, 0.14f), 0.3f);
		Material shirt = Mat("Shirt", new Color(0.15f, 0.45f, 0.8f), 0.2f);
		Material straw = Mat("Straw", new Color(0.82f, 0.66f, 0.35f), 0.1f);
		Material wall = Mat("HutWall", new Color(0.7f, 0.5f, 0.3f), 0.1f);
		Material ripple = Mat("Ripple", new Color(0.85f, 0.9f, 0.88f), 0.6f);
		Material yellowCloth = Mat("YellowCloth", new Color(1f, 0.85f, 0.1f), 0.2f);
		Material redCloth = Mat("RedCloth", new Color(0.85f, 0.15f, 0.15f), 0.2f);

		// ---------- Prefabs ----------
		// Rock: a big gray stone
		GameObject rockGo = Root("Rock", RiverObject.Kind.Rock);
		Part(rockGo, PrimitiveType.Sphere, rock, new Vector3(0, 0.15f, 0), new Vector3(1.7f, 1.2f, 1.5f), Vector3.zero, true);
		Part(rockGo, PrimitiveType.Sphere, rock, new Vector3(0.5f, 0.05f, -0.3f), new Vector3(0.9f, 0.7f, 0.9f), Vector3.zero, false);
		GameObject rockPrefab = SavePrefab(rockGo);

		// Log: lies across the water
		GameObject logGo = Root("Log", RiverObject.Kind.Log);
		Part(logGo, PrimitiveType.Cylinder, darkWood, new Vector3(0, 0.15f, 0), new Vector3(0.7f, 1.9f, 0.7f), new Vector3(0, 0, 90), true);
		Part(logGo, PrimitiveType.Cylinder, darkWood, new Vector3(0.6f, 0.5f, 0), new Vector3(0.18f, 0.4f, 0.18f), new Vector3(0, 0, 30), false);
		GameObject logPrefab = SavePrefab(logGo);

		// Hippo: hides under the water, then pops up
		GameObject hippoGo = Root("Hippo", RiverObject.Kind.Hippo);
		Part(hippoGo, PrimitiveType.Sphere, hippo, new Vector3(0, 0, 0.4f), new Vector3(1.7f, 1.2f, 2.3f), Vector3.zero, true);
		Part(hippoGo, PrimitiveType.Sphere, hippo, new Vector3(0, 0.25f, -0.9f), new Vector3(1.25f, 0.85f, 1.2f), Vector3.zero, false);
		Part(hippoGo, PrimitiveType.Sphere, hippoPink, new Vector3(0, 0.15f, -1.4f), new Vector3(0.9f, 0.5f, 0.4f), Vector3.zero, false);
		foreach (float side in new[] { -0.3f, 0.3f })
		{
			Part(hippoGo, PrimitiveType.Sphere, white, new Vector3(side, 0.62f, -1.05f), Vector3.one * 0.26f, Vector3.zero, false);
			Part(hippoGo, PrimitiveType.Sphere, black, new Vector3(side, 0.64f, -1.17f), Vector3.one * 0.12f, Vector3.zero, false);
			Part(hippoGo, PrimitiveType.Sphere, hippo, new Vector3(side * 1.4f, 0.72f, -0.75f), new Vector3(0.18f, 0.25f, 0.12f), Vector3.zero, false);
		}
		GameObject hippoRipple = Part(hippoGo, PrimitiveType.Cylinder, ripple, Vector3.zero, new Vector3(2.4f, 0.01f, 2.4f), Vector3.zero, false);
		hippoRipple.name = "Bubbles";
		hippoGo.GetComponent<RiverObject>().ripple = hippoRipple.transform;
		GameObject hippoPrefab = SavePrefab(hippoGo);

		// Supply box: food and medicine floating in the water
		GameObject boxGo = Root("SupplyBox", RiverObject.Kind.SupplyBox);
		boxGo.transform.position = new Vector3(0, 0.25f, 0);
		GameObject crate = Part(boxGo, PrimitiveType.Cube, box, Vector3.zero, Vector3.one * 0.9f, Vector3.zero, false);
		Part(boxGo, PrimitiveType.Cube, boxBand, Vector3.zero, new Vector3(0.95f, 0.2f, 0.95f), Vector3.zero, false);
		Part(boxGo, PrimitiveType.Cube, boxBand, Vector3.zero, new Vector3(0.2f, 0.95f, 0.95f), Vector3.zero, false);
		BoxCollider grab = boxGo.AddComponent<BoxCollider>();   // a bit bigger than the box, so it is easy to grab
		grab.size = new Vector3(1.4f, 1.4f, 1.4f);
		grab.isTrigger = true;
		GameObject boxPrefab = SavePrefab(boxGo);

		// Fast water: a strip of current that pushes the canoe sideways
		GameObject fastGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
		fastGo.name = "FastWater";
		Object.DestroyImmediate(fastGo.GetComponent<Collider>());
		fastGo.transform.position = new Vector3(0, 0.03f, 0);
		fastGo.transform.localScale = new Vector3(5f, 0.02f, 12f);
		fastGo.GetComponent<Renderer>().sharedMaterial = fast;
		RiverObject fastObj = fastGo.AddComponent<RiverObject>();
		fastObj.kind = RiverObject.Kind.FastWater;
		fastObj.arrows = fastGo.GetComponent<Renderer>();
		GameObject fastPrefab = SavePrefab(fastGo);

		// Tree on the river bank
		GameObject treeGo = Root("Tree", RiverObject.Kind.Decoration);
		Part(treeGo, PrimitiveType.Cylinder, wood, new Vector3(0, 1.5f, 0), new Vector3(0.35f, 1.5f, 0.35f), Vector3.zero, false);
		Part(treeGo, PrimitiveType.Sphere, leaves, new Vector3(0, 3.4f, 0), new Vector3(2.6f, 1.8f, 2.6f), Vector3.zero, false);
		Part(treeGo, PrimitiveType.Sphere, leaves, new Vector3(0.6f, 3f, 0.4f), new Vector3(1.6f, 1.3f, 1.6f), Vector3.zero, false);
		GameObject treePrefab = SavePrefab(treeGo);

		// Village dock: the finish
		GameObject dockGo = Root("VillageDock", RiverObject.Kind.VillageDock);
		Part(dockGo, PrimitiveType.Cube, wood, new Vector3(0, 0.35f, 4f), new Vector3(14f, 0.3f, 4f), Vector3.zero, false);
		for (float x = -6f; x <= 6f; x += 3f)
			Part(dockGo, PrimitiveType.Cylinder, darkWood, new Vector3(x, 0.2f, 2.1f), new Vector3(0.3f, 0.6f, 0.3f), Vector3.zero, false);
		foreach (float side in new[] { -1f, 1f })
		{
			Part(dockGo, PrimitiveType.Cylinder, wall, new Vector3(side * 9.5f, 1.3f, 6f), new Vector3(2.6f, 1.1f, 2.6f), Vector3.zero, false);
			Part(dockGo, PrimitiveType.Sphere, straw, new Vector3(side * 9.5f, 2.7f, 6f), new Vector3(3.4f, 1.6f, 3.4f), Vector3.zero, false);
			Part(dockGo, PrimitiveType.Cylinder, wall, new Vector3(side * 12f, 1.3f, 11f), new Vector3(2.6f, 1.1f, 2.6f), Vector3.zero, false);
			Part(dockGo, PrimitiveType.Sphere, straw, new Vector3(side * 12f, 2.7f, 11f), new Vector3(3.4f, 1.6f, 3.4f), Vector3.zero, false);
		}
		Material[] clothes = { shirt, yellowCloth, redCloth, shirt, redCloth };
		for (int i = 0; i < 5; i++)
		{
			float x = -4f + i * 2f;
			Part(dockGo, PrimitiveType.Capsule, clothes[i], new Vector3(x, 1.05f, 4.5f), new Vector3(0.5f, 0.55f, 0.5f), Vector3.zero, false);
			Part(dockGo, PrimitiveType.Sphere, skin, new Vector3(x, 1.85f, 4.5f), Vector3.one * 0.38f, Vector3.zero, false);
		}
		// Sign
		Part(dockGo, PrimitiveType.Cube, darkWood, new Vector3(-2.5f, 1.4f, 2.4f), new Vector3(0.15f, 2f, 0.15f), Vector3.zero, false);
		Part(dockGo, PrimitiveType.Cube, darkWood, new Vector3(2.5f, 1.4f, 2.4f), new Vector3(0.15f, 2f, 0.15f), Vector3.zero, false);
		Part(dockGo, PrimitiveType.Cube, straw, new Vector3(0, 2.4f, 2.4f), new Vector3(5.6f, 0.9f, 0.1f), Vector3.zero, false);
		GameObject sign = new GameObject("SignText");
		sign.transform.SetParent(dockGo.transform, false);
		sign.transform.localPosition = new Vector3(0, 2.4f, 2.3f);
		TextMesh signText = sign.AddComponent<TextMesh>();
		signText.text = "VILLAGE";
		signText.font = font;
		signText.fontSize = 64;
		signText.characterSize = 0.1f;
		signText.anchor = TextAnchor.MiddleCenter;
		signText.color = new Color(0.25f, 0.12f, 0.05f);
		sign.GetComponent<MeshRenderer>().sharedMaterial = font.material;
		GameObject dockPrefab = SavePrefab(dockGo);

		// ---------- Scene ----------
		var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

		RenderSettings.fog = true;
		RenderSettings.fogMode = FogMode.Linear;
		RenderSettings.fogStartDistance = 45f;
		RenderSettings.fogEndDistance = 100f;
		RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

		GameObject camGo = new GameObject("Main Camera");
		camGo.tag = "MainCamera";
		Camera cam = camGo.AddComponent<Camera>();
		cam.clearFlags = CameraClearFlags.SolidColor;
		cam.farClipPlane = 150f;
		cam.fieldOfView = 60f;
		camGo.AddComponent<AudioListener>();
		camGo.transform.position = new Vector3(0, 6.5f, -9f);
		camGo.transform.rotation = Quaternion.Euler(24f, 0, 0);

		GameObject sunGo = new GameObject("Sun");
		Light sun = sunGo.AddComponent<Light>();
		sun.type = LightType.Directional;
		sun.shadows = LightShadows.Soft;

		// River (a long cube like the book's Ground, rotated 180 so the scrolling goes toward the player)
		GameObject river = GameObject.CreatePrimitive(PrimitiveType.Cube);
		river.name = "River";
		Object.DestroyImmediate(river.GetComponent<Collider>());
		river.transform.position = new Vector3(0, -0.25f, 45f);
		river.transform.rotation = Quaternion.Euler(0, 180, 0);
		river.transform.localScale = new Vector3(13f, 0.5f, 120f);
		river.GetComponent<Renderer>().sharedMaterial = water;
		WaterScroller scroller = river.AddComponent<WaterScroller>();

		// River banks
		foreach (float side in new[] { -1f, 1f })
		{
			GameObject bank = GameObject.CreatePrimitive(PrimitiveType.Cube);
			bank.name = side < 0 ? "Left Bank" : "Right Bank";
			Object.DestroyImmediate(bank.GetComponent<Collider>());
			bank.transform.position = new Vector3(side * 11.5f, 0, 45f);
			bank.transform.localScale = new Vector3(10f, 0.8f, 120f);
			bank.GetComponent<Renderer>().sharedMaterial = grass;

			GameObject edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
			edge.name = bank.name + " Mud";
			Object.DestroyImmediate(edge.GetComponent<Collider>());
			edge.transform.position = new Vector3(side * 6.4f, 0.05f, 45f);
			edge.transform.localScale = new Vector3(0.6f, 0.4f, 120f);
			edge.GetComponent<Renderer>().sharedMaterial = mud;
		}

		// Canoe (the player)
		GameObject canoeGo = new GameObject("Canoe");
		BoxCollider hull = canoeGo.AddComponent<BoxCollider>();
		hull.center = new Vector3(0, 0.3f, 0);
		hull.size = new Vector3(1f, 0.8f, 2.6f);
		Rigidbody rb = canoeGo.AddComponent<Rigidbody>();
		rb.isKinematic = true;
		rb.useGravity = false;
		GameObject body = new GameObject("Body");
		body.transform.SetParent(canoeGo.transform, false);
		Part(body, PrimitiveType.Cube, wood, new Vector3(0, 0.15f, 0), new Vector3(1f, 0.4f, 2.4f), Vector3.zero, false);
		Part(body, PrimitiveType.Cube, darkWood, new Vector3(0, 0.3f, 0), new Vector3(0.8f, 0.2f, 2.2f), Vector3.zero, false);
		Part(body, PrimitiveType.Cube, wood, new Vector3(0, 0.15f, 1.2f), new Vector3(0.7f, 0.39f, 0.7f), new Vector3(0, 45, 0), false);
		Part(body, PrimitiveType.Cube, wood, new Vector3(0, 0.15f, -1.2f), new Vector3(0.7f, 0.39f, 0.7f), new Vector3(0, 45, 0), false);
		Part(body, PrimitiveType.Capsule, shirt, new Vector3(0, 0.75f, -0.45f), new Vector3(0.45f, 0.4f, 0.4f), Vector3.zero, false);
		Part(body, PrimitiveType.Sphere, skin, new Vector3(0, 1.3f, -0.45f), Vector3.one * 0.34f, Vector3.zero, false);
		Part(body, PrimitiveType.Cube, box, new Vector3(-0.15f, 0.5f, 0.5f), Vector3.one * 0.4f, new Vector3(0, 15, 0), false);
		Part(body, PrimitiveType.Cube, box, new Vector3(0.15f, 0.5f, 0.85f), Vector3.one * 0.35f, new Vector3(0, -10, 0), false);
		GameObject paddlePivot = new GameObject("Paddle");
		paddlePivot.transform.SetParent(body.transform, false);
		paddlePivot.transform.localPosition = new Vector3(0.35f, 0.95f, -0.35f);
		Part(paddlePivot, PrimitiveType.Cylinder, darkWood, new Vector3(0, -0.45f, 0), new Vector3(0.06f, 0.6f, 0.06f), Vector3.zero, false);
		Part(paddlePivot, PrimitiveType.Cube, darkWood, new Vector3(0, -1.05f, 0), new Vector3(0.05f, 0.4f, 0.22f), Vector3.zero, false);
		Canoe canoe = canoeGo.AddComponent<Canoe>();
		canoe.body = body.transform;
		canoe.paddle = paddlePivot.transform;

		// Game manager and spawner
		GameObject managerGo = new GameObject("Game Manager");
		RiverGameManager manager = managerGo.AddComponent<RiverGameManager>();
		RiverSpawner spawner = managerGo.AddComponent<RiverSpawner>();
		manager.canoe = canoe;
		manager.sun = sun;
		manager.cam = cam;
		manager.water = scroller;
		canoe.manager = manager;
		scroller.manager = manager;
		spawner.manager = manager;
		spawner.rockPrefab = rockPrefab;
		spawner.logPrefab = logPrefab;
		spawner.hippoPrefab = hippoPrefab;
		spawner.boxPrefab = boxPrefab;
		spawner.fastWaterPrefab = fastPrefab;
		spawner.treePrefab = treePrefab;
		spawner.villageDockPrefab = dockPrefab;

		BuildUI(manager);

		EditorSceneManager.SaveScene(scene, "Assets/Scenes/Main.unity");
		EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true) };
		PlayerSettings.productName = "River Run Congo";
		PlayerSettings.companyName = "Joel Kalala";
		AssetDatabase.SaveAssets();
		Debug.Log("RIVER-BUILD done");
	}

	// ---------- UI ----------
	static void BuildUI(RiverGameManager manager)
	{
		Sprite knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

		GameObject canvasGo = new GameObject("Canvas");
		Canvas canvas = canvasGo.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
		scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		scaler.referenceResolution = new Vector2(1280, 720);
		scaler.matchWidthOrHeight = 0.5f;
		canvasGo.AddComponent<GraphicRaycaster>();

		GameObject es = new GameObject("EventSystem");
		es.AddComponent<UnityEngine.EventSystems.EventSystem>();
		es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

		Color panelColor = new Color(0, 0, 0, 0.45f);

		// HUD
		RectTransform hud = Stretch(new GameObject("HUD", typeof(RectTransform)), canvasGo.transform);
		manager.hud = hud.gameObject;

		// Top left: supplies and canoe health
		RectTransform left = Box(hud, "Supplies Panel", new Vector2(0, 1), new Vector2(20, -20), new Vector2(290, 92), panelColor, null);
		Box(left, "Box Icon", new Vector2(0, 1), new Vector2(14, -14), new Vector2(26, 26), new Color(1f, 0.72f, 0.15f), null);
		manager.boxesText = MakeText(left, "Supplies Text", "Supplies: 0 / 10", 26, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(50, -10), new Vector2(230, 34));
		MakeText(left, "Canoe Label", "Canoe:", 24, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(14, -50), new Vector2(90, 32));
		manager.hearts = new Image[3];
		for (int i = 0; i < 3; i++)
			manager.hearts[i] = Box(left, "Health " + (i + 1), new Vector2(0, 1), new Vector2(105 + i * 34, -53), new Vector2(26, 26), Color.red, knob).GetComponent<Image>();

		// Top center: market to village progress
		RectTransform trip = Box(hud, "Trip Panel", new Vector2(0.5f, 1), new Vector2(-260, -20), new Vector2(520, 70), panelColor, null);
		MakeText(trip, "Market Label", "Market", 20, TextAnchor.MiddleLeft, new Vector2(0, 1), new Vector2(12, -6), new Vector2(120, 26));
		MakeText(trip, "Village Label", "Village", 20, TextAnchor.MiddleRight, new Vector2(1, 1), new Vector2(-132, -6), new Vector2(120, 26));
		RectTransform track = Box(trip, "Trip Track", new Vector2(0, 1), new Vector2(20, -44), new Vector2(480, 8), new Color(1, 1, 1, 0.5f), null);
		RectTransform marker = Box(track, "Canoe Marker", new Vector2(0, 0.5f), Vector2.zero, new Vector2(22, 22), new Color(0.6f, 0.38f, 0.2f), knob);
		marker.pivot = new Vector2(0.5f, 0.5f);
		marker.anchoredPosition = Vector2.zero;
		manager.progressTrack = track;
		manager.progressMarker = marker;

		// Top right: the sun going down (no number timer)
		RectTransform sunArea = Box(hud, "Sun Panel", new Vector2(1, 1), new Vector2(-360, -20), new Vector2(340, 110), panelColor, null);
		MakeText(sunArea, "Sun Label", "Sun", 18, TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(10, -6), new Vector2(100, 24));
		RectTransform horizon = Box(sunArea, "Horizon", new Vector2(0.5f, 0.5f), new Vector2(-160, -52), new Vector2(320, 3), new Color(0.3f, 0.6f, 0.3f), null);
		horizon.pivot = new Vector2(0, 0.5f);
		RectTransform sunIcon = Box(sunArea, "Sun Icon", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(36, 36), Color.yellow, knob);
		sunIcon.pivot = new Vector2(0.5f, 0.5f);
		sunIcon.anchoredPosition = Vector2.zero;
		manager.sunIcon = sunIcon;
		manager.sunIconImage = sunIcon.GetComponent<Image>();

		// Bottom: paddle energy
		RectTransform energyBg = Box(hud, "Energy Bar", new Vector2(0.5f, 0), new Vector2(-200, 30), new Vector2(400, 20), new Color(0, 0, 0, 0.55f), null);
		energyBg.pivot = new Vector2(0, 0);
		energyBg.anchoredPosition = new Vector2(-200, 30);
		RectTransform fill = Stretch(new GameObject("Energy Fill", typeof(RectTransform), typeof(Image)), energyBg);
		manager.energyFill = fill;
		manager.energyFillImage = fill.GetComponent<Image>();
		Text energyLabel = MakeText(hud, "Energy Label", "PADDLE ENERGY (Space)", 20, TextAnchor.MiddleCenter, new Vector2(0.5f, 0), new Vector2(-200, 82), new Vector2(400, 28));
		energyLabel.rectTransform.pivot = new Vector2(0, 1);
		energyLabel.rectTransform.anchoredPosition = new Vector2(-200, 82);
		Text help = MakeText(hud, "Controls", "A / D  steer      Space  paddle hard      S  slow down", 18, TextAnchor.MiddleLeft, new Vector2(0, 0), new Vector2(20, 40), new Vector2(520, 26));
		help.color = new Color(1, 1, 1, 0.85f);

		manager.warningText = MakeText(hud, "Warning", "", 32, TextAnchor.MiddleCenter, new Vector2(0.5f, 1), new Vector2(-400, -100), new Vector2(800, 50));

		// Title screen
		RectTransform title = Stretch(new GameObject("Title Panel", typeof(RectTransform), typeof(Image)), canvasGo.transform);
		title.GetComponent<Image>().color = new Color(0, 0, 0, 0.55f);
		manager.titlePanel = title.gameObject;
		Text t1 = MakeText(title, "Title", "RIVER RUN: CONGO", 72, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(-500, 230), new Vector2(1000, 100));
		t1.color = new Color(1f, 0.78f, 0.2f);
		t1.fontStyle = FontStyle.Bold;
		MakeText(title, "Goal", "Bring 10 supply boxes home to your village before the sun goes down.", 28, TextAnchor.MiddleCenter,
			new Vector2(0.5f, 0.5f), new Vector2(-560, 140), new Vector2(1120, 50));
		MakeText(title, "Rules",
			"A / D  or  arrows  -  steer the canoe\n" +
			"Space  -  paddle hard (faster, uses energy)\n" +
			"S  -  slow down for tight spots\n\n" +
			"Grab the yellow boxes. Rocks, logs and hippos break your canoe.\n" +
			"Fast water pushes you to the side. Watch the sun!",
			24, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(-560, 90), new Vector2(1120, 220));
		Text start = MakeText(title, "Press Enter", "Press ENTER to start", 34, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(-400, -200), new Vector2(800, 60));
		start.color = new Color(0.6f, 1f, 0.6f);

		// End screen
		RectTransform end = Stretch(new GameObject("End Panel", typeof(RectTransform), typeof(Image)), canvasGo.transform);
		end.GetComponent<Image>().color = new Color(0, 0, 0, 0.65f);
		manager.endPanel = end.gameObject;
		manager.endTitle = MakeText(end, "End Title", "", 64, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(-560, 170), new Vector2(1120, 100));
		manager.endTitle.fontStyle = FontStyle.Bold;
		manager.endText = MakeText(end, "End Text", "", 30, TextAnchor.UpperCenter, new Vector2(0.5f, 0.5f), new Vector2(-560, 80), new Vector2(1120, 300));
	}

	static RectTransform Stretch(GameObject go, Transform parent)
	{
		RectTransform rt = (RectTransform)go.transform;
		rt.SetParent(parent, false);
		rt.anchorMin = Vector2.zero;
		rt.anchorMax = Vector2.one;
		rt.offsetMin = Vector2.zero;
		rt.offsetMax = Vector2.zero;
		return rt;
	}

	// A colored rectangle. position = top-left corner from the anchor.
	static RectTransform Box(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size, Color color, Sprite sprite)
	{
		GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
		RectTransform rt = (RectTransform)go.transform;
		rt.SetParent(parent, false);
		rt.anchorMin = anchor;
		rt.anchorMax = anchor;
		rt.pivot = new Vector2(0, 1);
		rt.sizeDelta = size;
		rt.anchoredPosition = position;
		Image img = go.GetComponent<Image>();
		img.color = color;
		img.sprite = sprite;
		return rt;
	}

	static Text MakeText(Transform parent, string name, string value, int size, TextAnchor align, Vector2 anchor, Vector2 position, Vector2 box)
	{
		GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Outline));
		RectTransform rt = (RectTransform)go.transform;
		rt.SetParent(parent, false);
		rt.anchorMin = anchor;
		rt.anchorMax = anchor;
		rt.pivot = new Vector2(0, 1);
		rt.sizeDelta = box;
		rt.anchoredPosition = position;
		Text text = go.GetComponent<Text>();
		text.font = font;
		text.text = value;
		text.fontSize = size;
		text.alignment = align;
		text.color = Color.white;
		text.horizontalOverflow = HorizontalWrapMode.Overflow;
		text.verticalOverflow = VerticalWrapMode.Overflow;
		go.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0.8f);
		return text;
	}

	// ---------- Helpers ----------
	static void Folder(string parent, string name)
	{
		if (!AssetDatabase.IsValidFolder(parent + "/" + name))
			AssetDatabase.CreateFolder(parent, name);
	}

	static Material Mat(string name, Color color, float smooth)
	{
		Material m = new Material(Shader.Find("Standard"));
		m.color = color;
		m.SetFloat("_Glossiness", smooth);
		return SaveMat(m, name);
	}

	static Material SaveMat(Material m, string name)
	{
		string path = "Assets/Materials/" + name + ".mat";
		AssetDatabase.DeleteAsset(path);
		AssetDatabase.CreateAsset(m, path);
		return m;
	}

	static Texture2D SaveTexture(Texture2D tex, string path)
	{
		AssetDatabase.DeleteAsset(path);
		AssetDatabase.CreateAsset(tex, path);
		return tex;
	}

	static GameObject Root(string name, RiverObject.Kind kind)
	{
		GameObject go = new GameObject(name);
		RiverObject obj = go.AddComponent<RiverObject>();
		obj.kind = kind;
		return go;
	}

	// Adds a shape as a child. Only "hit" parts keep a (trigger) collider.
	static GameObject Part(GameObject parent, PrimitiveType type, Material mat, Vector3 pos, Vector3 scale, Vector3 rot, bool hit)
	{
		GameObject go = GameObject.CreatePrimitive(type);
		go.transform.SetParent(parent.transform, false);
		go.transform.localPosition = pos;
		go.transform.localScale = scale;
		go.transform.localEulerAngles = rot;
		go.GetComponent<Renderer>().sharedMaterial = mat;
		Collider c = go.GetComponent<Collider>();
		if (hit)
			c.isTrigger = true;
		else
			Object.DestroyImmediate(c);
		return go;
	}

	static GameObject SavePrefab(GameObject go)
	{
		string path = "Assets/Prefabs/" + go.name + ".prefab";
		GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
		Object.DestroyImmediate(go);
		return prefab;
	}

	// Brown-green Congo river water with light wave lines
	static Texture2D MakeWaterTexture()
	{
		const int size = 128;
		Texture2D tex = new Texture2D(size, size, TextureFormat.RGB24, true);
		tex.wrapMode = TextureWrapMode.Repeat;
		Color deep = new Color(0.32f, 0.36f, 0.3f);
		Color light = new Color(0.55f, 0.6f, 0.52f);
		Random.InitState(7);
		for (int y = 0; y < size; y++)
			for (int x = 0; x < size; x++)
			{
				float wave = Mathf.Sin((y + Mathf.Sin(x * 0.15f) * 6f) * Mathf.PI * 2f / 16f);
				float line = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.82f, 1f, wave));
				float noise = Mathf.PerlinNoise(x * 0.07f, y * 0.07f) * 0.15f;
				tex.SetPixel(x, y, Color.Lerp(deep, light, line * 0.8f + noise));
			}
		tex.Apply();
		return tex;
	}

	// Light blue strip with white arrows (>>) for fast water
	static Texture2D MakeArrowTexture()
	{
		const int size = 64;
		Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
		tex.wrapMode = TextureWrapMode.Repeat;
		for (int y = 0; y < size; y++)
			for (int x = 0; x < size; x++)
			{
				// Chevron: x distance from the line x = 20 + |y - 32|
				float d = Mathf.Abs(x - (20 + Mathf.Abs(y - 32) * 0.6f));
				bool arrow = d < 5f && Mathf.Abs(y - 32) < 24;
				Color c = arrow ? new Color(1f, 1f, 1f, 0.85f) : new Color(0.55f, 0.85f, 1f, 0.35f);
				tex.SetPixel(x, y, c);
			}
		tex.Apply();
		return tex;
	}
}
