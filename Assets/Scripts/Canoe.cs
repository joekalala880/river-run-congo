using UnityEngine;

// The player's canoe: steer left and right, paddle hard (Space) or slow down (S).
public class Canoe : MonoBehaviour
{
	[Header("References")]
	public RiverGameManager manager;
	public Transform paddle;
	public Transform body;   // the visible canoe (tilts and blinks)

	[Header("Steering")]
	public float bounds = 4.5f;
	public float strafeSpeed = 6f;

	[Header("River Speed")]
	public float normalSpeed = 15f;
	public float boostSpeed = 25f;
	public float slowSpeed = 7f;

	[Header("Paddle Energy")]
	public float maxEnergy = 100f;
	public float drainPerSecond = 30f;
	public float refillPerSecond = 15f;
	public float energyToRestart = 25f;   // after running out, wait until the bar refills this much

	[Header("Hits")]
	public float safeTimeAfterHit = 1.5f;

	float energy;
	bool tired;          // ran out of energy, waiting to refill
	float safeTimer;
	float pushSpeed;     // sideways push from fast water this frame
	float paddleAngle;
	Renderer[] renderers;

	public float Energy { get { return energy; } }
	public bool CanBoost { get { return !tired && energy > 0; } }
	public bool IsBoosting { get; private set; }
	public bool IsSlowing { get; private set; }
	public bool IsSafe { get { return safeTimer > 0; } }
	public float ForwardSpeed { get { return IsBoosting ? boostSpeed : IsSlowing ? slowSpeed : normalSpeed; } }

	// Lets the checks (and fast water) push the canoe
	[HideInInspector] public float testSteer;
	[HideInInspector] public bool testBoost;
	[HideInInspector] public bool testSlow;

	void Start()
	{
		energy = maxEnergy;
		renderers = body.GetComponentsInChildren<Renderer>();
	}

	void Update()
	{
		if (!manager.IsPlaying)
		{
			IsBoosting = false;
			IsSlowing = false;
			AnimatePaddle(0.6f);
			return;
		}

		// Steer
		float steer = Mathf.Clamp(Input.GetAxis("Horizontal") + testSteer, -1f, 1f);
		Vector3 position = transform.position;
		position.x += (steer * strafeSpeed + pushSpeed) * Time.deltaTime;
		position.x = Mathf.Clamp(position.x, -bounds, bounds);
		transform.position = position;
		pushSpeed = 0;

		// Paddle hard (Space) uses energy, slow down (S) to get through tight spots
		bool wantBoost = Input.GetKey(KeyCode.Space) || testBoost;
		IsSlowing = !wantBoost && (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || testSlow);
		IsBoosting = wantBoost && CanBoost;

		if (IsBoosting)
		{
			energy -= drainPerSecond * Time.deltaTime;
			if (energy <= 0)
			{
				energy = 0;
				tired = true;
			}
		}
		else
		{
			energy = Mathf.Min(maxEnergy, energy + refillPerSecond * Time.deltaTime);
			if (tired && energy >= energyToRestart)
				tired = false;
		}

		// Lean into turns
		body.localRotation = Quaternion.Euler(0, steer * 8f, -steer * 10f);

		AnimatePaddle(IsBoosting ? 3f : IsSlowing ? 0.5f : 1.4f);

		// Blink while safe after a hit
		if (safeTimer > 0)
		{
			safeTimer -= Time.deltaTime;
			bool show = safeTimer <= 0 || Mathf.Repeat(safeTimer, 0.2f) > 0.1f;
			foreach (Renderer r in renderers)
				r.enabled = show;
		}
	}

	void AnimatePaddle(float strokesPerSecond)
	{
		paddleAngle += strokesPerSecond * 360f * Time.deltaTime;
		paddle.localRotation = Quaternion.Euler(Mathf.Sin(paddleAngle * Mathf.Deg2Rad) * 35f, 0, 20f);
	}

	// Fast water pushes the canoe sideways
	public void Push(float speed)
	{
		pushSpeed += speed;
	}

	void OnTriggerEnter(Collider other)
	{
		RiverObject thing = other.GetComponentInParent<RiverObject>();
		if (thing == null || !manager.IsPlaying)
			return;

		if (thing.kind == RiverObject.Kind.SupplyBox)
		{
			manager.CollectBox();
			Destroy(thing.gameObject);
		}
		else if (thing.IsDanger && !IsSafe && !thing.hasHit)
		{
			thing.hasHit = true;
			safeTimer = safeTimeAfterHit;
			manager.Damage();
		}
	}
}
