using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {
	Rigidbody2D rb;
	public List<Transform> asteroidTransforms;
	public GameObject powerupPrefab;

	// Controls
	readonly KeyCode UP = KeyCode.W;
	readonly KeyCode DOWN = KeyCode.S;
	readonly KeyCode LEFT = KeyCode.A;
	readonly KeyCode RIGHT = KeyCode.D;

	// Motion
	public float moveSpeed, accelerationTime, decelerationTime;
	Vector3 moveVelocity;

	public int circlePoints = 8;
	public float circleRadius = 2f;
	public int powerupsToSpawn = 5;

	void Update() {
		PlayerMovement(); // Invoke every frame

		if (Input.GetMouseButton(0)) Shoot();

		RechargeMeter();
	}

	float Clamp(float change) {
		return Mathf.Clamp(change, -1, 1);
	}

	float Decelerate(float currentValue) {
		// If the value is below 0, just be instant. Otherwise divide the distance by the time (1 unit, 3000 milliseconds for example)
		float decelerationSpeed = (decelerationTime > 0) ? (1.0f / decelerationTime) : 1.0f;

		// Add up to 0 if negative, subtract down to 0 if positive
		return (currentValue < 0) ?
			Mathf.Clamp(currentValue + decelerationSpeed * Time.deltaTime, -1, 0)
			:
			Mathf.Clamp(currentValue - decelerationSpeed * Time.deltaTime, 0, 1);
	}

	Vector3 intendedMoveDir = Vector3.zero;
	Vector3 constantMoveDir = Vector3.zero;
	Vector3 moveVector;

	public float rotationSpeed = 1f;
	public void PlayerMovement() {
		// Re-calculate the speed in case the accelerationTime value changes during runtime
		float accelerationSpeed = (accelerationTime > 0) ? (1.0f / accelerationTime) : 1.0f; // Make sure the value is above 

		// Up and down arrows cannot be held at the same time; prioritize one or the other
		if (Input.GetKey(UP)) {
			intendedMoveDir.y = 1;
			moveVelocity.y = Clamp(moveVelocity.y + accelerationSpeed * Time.deltaTime); // (?, 1)
		} else if (Input.GetKey(DOWN)) {
			intendedMoveDir.y = -1;
			moveVelocity.y = Clamp(moveVelocity.y - accelerationSpeed * Time.deltaTime); // (?, -1)
		} else {
			intendedMoveDir.y = 0;
			moveVelocity.y = Decelerate(moveVelocity.y); // (?, 0) (neither being held down)
		}

		if (Input.GetKey(LEFT)) {
			intendedMoveDir.x = -1;
			moveVelocity.x = Clamp(moveVelocity.x - accelerationSpeed * Time.deltaTime); // (-1, ?)
		} else if (Input.GetKey(RIGHT)) {
			intendedMoveDir.x = 1;
			moveVelocity.x = Clamp(moveVelocity.x + accelerationSpeed * Time.deltaTime); // (1, ?)
		} else {
			intendedMoveDir.x = 0;
			moveVelocity.x = Decelerate(moveVelocity.x); // (0, ?)
		}
	}

	private void FixedUpdate() {
		moveVector = (moveSpeed * Time.fixedDeltaTime * moveVelocity); // Where we want the position to be
		Vector3 intendedPosOnScreen = Camera.main.WorldToViewportPoint(transform.position + moveVector); // Check if this position is within the viewport

		if (!intendedMoveDir.Equals(Vector3.zero)) constantMoveDir = (moveVector.normalized * 2);

		// Keep on the screen
		if (intendedPosOnScreen.x < 0.3 || intendedPosOnScreen.x > 1.0) moveVector.x = 0; // This position is to the left or right of the viewport; freeze X
		if (intendedPosOnScreen.y < 0.0 || intendedPosOnScreen.y > 1.0) moveVector.y = 0; // Same but for Y. Overwrites moveVector

		float rotationAngle = -(Mathf.Atan2(constantMoveDir.x, constantMoveDir.y) * Mathf.Rad2Deg);

		rb.rotation = rotationAngle;
		rb.position = rb.transform.position + moveVector;
	}

	//////////////////////////////////////
	/////////////// Week 4 ///////////////
	//////////////////////////////////////
	Vector3 GetPoint(int index, float spaceBetweenPoints, float radius) {
		float thisAngle = (spaceBetweenPoints * index) * Mathf.Deg2Rad; // Multiply how much space between each point by the current index and convert to radians
		Vector3 thisPoint = transform.position + (new Vector3(Mathf.Cos(thisAngle), Mathf.Sin(thisAngle)) * radius); // P = ( cos(Θ), sin(Θ) ) * radius
		// May as well add to player's position since all uses of this method need to do it anyways

		return thisPoint; // Coordinate of the point on the circle relative to the circle's origin
	}

	public void SpawnPowerups(float radius, int numOfPowerups) {
		float spaceBetweenPowerups = 360.0f / numOfPowerups; // Divide 360 degrees into the number of powerups

        for (int index = 0; index < numOfPowerups; index++){ // For each powerup
			// Create the powerup at the appropriate point
			GameObject powerup = Instantiate(powerupPrefab, GetPoint(index, spaceBetweenPowerups, radius), powerupPrefab.transform.rotation);
			Destroy(powerup, 5); // Destruct after 5 seconds to avoid cluttering the scene
        }
    }

	///////////////////////////////////////
	////////// Week 7 Assignment //////////
	///////////////////////////////////////
	public float fireRate = 0.2f;
	public float maxBulletCharge = 25f;
	float bulletCharge;
	bool shotCooldown = false;
	bool rechargeCooldown = false; // Longer recharge if charge hits 0
	public GameObject bulletPrefab;
	readonly Transform[] shotPoints = new Transform[2];
	int lastBulletShotPoint = 0; // 0 or 1
	Slider ammoMeter;
	static Slider healthMeter;
	Image ammoMeterFill;
	public float meterRechargeSpeed = 0.1f;
	public static float maxHealth = 100;
	static float health;

	private void Start() { // Yes I'm putting Start all the way down here, too bad
		rb = GetComponent<Rigidbody2D>();
		bulletCharge = maxBulletCharge;

		health = maxHealth;
		healthMeter = GameObject.FindGameObjectWithTag("HealthMeter").GetComponent<Slider>();
		healthMeter.maxValue = maxHealth;
		healthMeter.value = health;

		ammoMeter = GameObject.FindGameObjectWithTag("AmmoMeter").GetComponent<Slider>();
		ammoMeter.maxValue = maxBulletCharge;
		ammoMeter.value = bulletCharge;
		ammoMeterFill = ammoMeter.fillRect.GetComponent<Image>();

		shotPoints[0] = transform.Find("BulletPoint1");
		shotPoints[1] = transform.Find("BulletPoint2");
	}

	void Shoot() {
		if (shotCooldown) return;
		if (rechargeCooldown) return;
		if (bulletCharge < 1) return;

		shotCooldown = true;
		StartCoroutine(DisableShotCooldown());

		bulletCharge -= 1;
		ammoMeter.value = bulletCharge;
		
		if (bulletCharge < 1) {
			rechargeCooldown = true;
			StartCoroutine(DisableRechargeCooldown());
		}

		Transform shotPoint = shotPoints[lastBulletShotPoint];
		Instantiate(bulletPrefab, shotPoint.position, shotPoint.rotation);

		lastBulletShotPoint = (lastBulletShotPoint + 1) % 2;
	}

	void RechargeMeter() {
		if (shotCooldown) return;
		if (bulletCharge >= maxBulletCharge) return;

		bulletCharge = Mathf.Clamp(bulletCharge + meterRechargeSpeed * Time.deltaTime, 0, maxBulletCharge);
		ammoMeter.value = bulletCharge;
	}

	IEnumerator DisableShotCooldown() {
		yield return new WaitForSeconds(fireRate);

		shotCooldown = false;
	}

	IEnumerator DisableRechargeCooldown() {
		ammoMeterFill.color = new Color(130f/255, 0, 0);

		yield return new WaitForSeconds(4);

		rechargeCooldown = false;
		ammoMeterFill.color = new Color(88f/255, 105f/255, 1);
	}

	public static void IncrementHealth(float increment) {
		health = Mathf.Clamp(health + increment, 0, maxHealth);
		healthMeter.value = health;
	}
}

