using System.Collections.Generic;
using UnityEngine;

// Anything floating in the river. It moves toward the canoe at the river speed.
public class RiverObject : MonoBehaviour
{
	public enum Kind { Rock, Log, Hippo, SupplyBox, FastWater, Decoration, VillageDock }

	public Kind kind;
	public RiverGameManager manager;

	[Header("Hippo")]
	public float hippoRiseDistance = 26f;   // the hippo pops up when it gets this close
	public float hippoHiddenY = -2.2f;
	public float hippoUpY = 0f;
	public Transform ripple;

	[Header("Fast Water")]
	public float pushSpeed = 4f;      // + pushes right, - pushes left
	public Renderer arrows;

	[HideInInspector] public bool hasHit;

	public static readonly List<RiverObject> FastWaters = new List<RiverObject>();

	public bool IsDanger { get { return kind == Kind.Rock || kind == Kind.Log || kind == Kind.Hippo; } }
	public bool HippoIsUp { get; private set; }

	float bobTime;

	void OnEnable()
	{
		if (kind == Kind.FastWater) FastWaters.Add(this);
		bobTime = Random.value * 10f;
	}

	void OnDisable()
	{
		FastWaters.Remove(this);
	}

	void Start()
	{
		// Arrows point the way the water pushes
		if (kind == Kind.FastWater && arrows != null)
		{
			Vector2 scale = arrows.material.mainTextureScale;
			scale.x = Mathf.Abs(scale.x) * (pushSpeed > 0 ? 1f : -1f);
			arrows.material.mainTextureScale = scale;
		}

		if (kind == Kind.Hippo)
		{
			Vector3 p = transform.position;
			p.y = hippoHiddenY;
			transform.position = p;
		}
	}

	void Update()
	{
		if (manager == null)
			return;

		transform.Translate(0, 0, -manager.Speed * Time.deltaTime, Space.World);

		bobTime += Time.deltaTime;

		if (kind == Kind.SupplyBox)
		{
			// Bob and spin on the water so it is easy to see
			transform.Rotate(0, 60f * Time.deltaTime, 0, Space.World);
			Vector3 p = transform.position;
			p.y = 0.25f + Mathf.Sin(bobTime * 3f) * 0.12f;
			transform.position = p;
		}
		else if (kind == Kind.Hippo)
		{
			// Hidden under the water, then pops up in front of you
			if (!HippoIsUp && transform.position.z < hippoRiseDistance)
			{
				HippoIsUp = true;
				manager.sound.Splash();
			}
			Vector3 p = transform.position;
			p.y = Mathf.MoveTowards(p.y, HippoIsUp ? hippoUpY : hippoHiddenY, 5f * Time.deltaTime);
			transform.position = p;
			// Bubbles on top of the water warn you before it comes up
			if (ripple != null)
			{
				ripple.position = new Vector3(transform.position.x, 0.04f, transform.position.z);
				float size = 2.2f + Mathf.PingPong(bobTime * 1.5f, 0.8f);
				ripple.localScale = new Vector3(size, 0.01f, size);
			}
		}
		else if (kind == Kind.FastWater && arrows != null)
		{
			Vector2 offset = arrows.material.mainTextureOffset;
			offset.x += Time.deltaTime * 1.2f;
			arrows.material.mainTextureOffset = offset;
		}

		// Fast water pushes the canoe while it is inside the strip
		if (kind == Kind.FastWater && manager.IsPlaying)
		{
			Canoe canoe = manager.canoe;
			Vector3 size = transform.localScale;
			Vector3 c = canoe.transform.position;
			if (Mathf.Abs(c.z - transform.position.z) < size.z * 0.5f && Mathf.Abs(c.x - transform.position.x) < size.x * 0.5f + 0.4f)
				canoe.Push(pushSpeed);
		}

		if (transform.position.z < -15f)
			Destroy(gameObject);
	}
}
