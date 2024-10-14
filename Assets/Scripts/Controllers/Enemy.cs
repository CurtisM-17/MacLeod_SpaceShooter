using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
	Transform plrTransform;
	public float detectionRange = 4f;
	public float minWanderRange, maxWanderRange;
	public float moveSpeed = 1.5f;
	float timer;

	bool currentlyChasing = false;

	private void Start() {
		plrTransform = GameObject.FindGameObjectWithTag("Player").transform;

		StartCoroutine(StopAndWait());
		health = maxHealth;
	}

	private void Update() {
		timer += Time.deltaTime;
		EnemyMovement();
	}

	public void EnemyMovement() {
		float distanceFromPlr = (transform.position - plrTransform.position).magnitude;

		if (distanceFromPlr > detectionRange) Wander();
		else ChasePlayer();
	}

	public float MoveTo(Vector3 pos) {
		Vector3 diff = pos - transform.position;
		Vector3 dir = diff.normalized;

		transform.position += moveSpeed * Time.deltaTime * dir;

		float rotationAngle = -(Mathf.Atan2(diff.x, diff.y) * Mathf.Rad2Deg);
		transform.rotation = Quaternion.Euler(0, 0, rotationAngle);

		return (pos - transform.position).magnitude; // Re-calculate after movement
	}

	/// Wander
	Vector3 currentRandomSpot;
	bool stoppingAndWaiting = true; // STart with a delay

	void Wander() {
		if (stoppingAndWaiting) return;

		currentlyChasing = false;

		float distFromGoal = MoveTo(currentRandomSpot);
		if (distFromGoal <= 0.1f) StartCoroutine(StopAndWait());
	}

	IEnumerator StopAndWait() {
		stoppingAndWaiting = true;
		yield return new WaitForSeconds(Random.Range(2, 5));

		NewRandomSpot();
		stoppingAndWaiting = false;
	}

	void NewRandomSpot() {
		// Not too close to the same spot but also 50% chance of positive or negative
		float xDiff = Random.Range(minWanderRange, maxWanderRange);
		float yDiff = Random.Range(minWanderRange, maxWanderRange);

		xDiff = (Random.Range(1, 3) == 1) ? xDiff : -xDiff;
		yDiff = (Random.Range(1, 3) == 1) ? yDiff : -yDiff;

		currentRandomSpot = transform.position + new Vector3(xDiff, yDiff, 0);

		Vector3 posOnScreen = Camera.main.WorldToViewportPoint(currentRandomSpot);
		if (posOnScreen.x < 0.0 || posOnScreen.x > 1.0 || posOnScreen.y < 0.0 || posOnScreen.y > 1.0) NewRandomSpot(); // Force within screen bounds
	}

	/// Chase player
	void ChasePlayer() {
		if (currentlyChasing == false) {
			lastShotTime = timer + 1; // Transitional period, don't shoot instantly
			NewShotRate();
		}
		currentlyChasing = true;

		MoveTo(plrTransform.position);
		Shooting();
	}

	// Collisions
	public float collisionDamage = 35;
	public GameObject explosionPrefab;

	private void OnTriggerEnter2D(Collider2D collision) {
		if (!collision.gameObject.CompareTag("Player")) return;

		Player.IncrementHealth(-collisionDamage);
		Die();
	}

	// Shooting
	public GameObject bulletPrefab;
	public float minShotRate, maxShotRate;
	float currentShotRate, lastShotTime;

	void NewShotRate() {
		currentShotRate = Random.Range(minShotRate, maxShotRate);
	}

	void Shooting() {
		if (timer - lastShotTime < currentShotRate) return;

		print(timer + " | " + lastShotTime + " | " + currentShotRate);

		Instantiate(bulletPrefab, transform.position, transform.rotation);

		lastShotTime = timer;
		NewShotRate();
	}

	public float maxHealth = 10f;
	float health;

	public void Damage(float damage) {
		health = Mathf.Clamp(health - damage, 0, maxHealth);

		print(health);

		if (health == 0) Die();
	}

	void Die() {
		Destroy(gameObject);

		GameObject explosion = Instantiate(explosionPrefab, transform.position, explosionPrefab.transform.rotation);
		Destroy(explosion, 1.9f);
	}
}
