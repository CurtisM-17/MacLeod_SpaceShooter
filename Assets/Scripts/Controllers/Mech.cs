using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Mech : MonoBehaviour
{
	public static float maxHealth = 100;
	public static float health = maxHealth;
	static Slider healthBar;
	GameObject arm;
	Rigidbody2D armRb;
	Transform playerTransform;
	public GameObject enemyPrefab;

	public float moveArmSpeed = 2f;

	bool currentlyPointing = false;
	Vector3 currentArmPointAt = Vector3.zero;
	string pointContext; // Current reason why the arm is pointing

	private void Start() {
		healthBar = GameObject.FindGameObjectWithTag("EnemyHealthMeter").GetComponent<Slider>();
		healthBar.maxValue = maxHealth;
		healthBar.value = health;

		arm = transform.Find("Arm").gameObject;
		armRb = arm.GetComponent<Rigidbody2D>();
		enemySpawnpoint = arm.transform.Find("EnemySpawnpoint");

		playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

		//NewAttackTime();
		currentAttackTime = 3;
		StartCoroutine(Attack());

		NewMoveInterval();
		StartCoroutine(MoveAfterDelay());

		targetYPosition = transform.position.y;
	}

	private void Update() {
		Move();
	}

	private void FixedUpdate() {
		if (health <= 0) return;

		if (currentlyPointing) PointAt(currentArmPointAt, "");
	}

	public static void IncrementHealth(float increment) {
		health = Mathf.Clamp(health + increment, 0, maxHealth);
		healthBar.value = health;
		if (health == 0) Die();
	}

	static void Die() {
		Destroy(GameObject.FindGameObjectWithTag("Mech"));
	}

	/// Move arm (point at)
	void PointAt(Vector3 position, string context) {
		if (!context.Equals("")) pointContext = context;

		currentlyPointing = true;
		currentArmPointAt = position;

		Vector3 diff = (position - armRb.transform.position);

		float rotationAngle = -(Mathf.Atan2(diff.x, diff.y) * Mathf.Rad2Deg) - 180;

		float progress = (rotationAngle - armRb.rotation) % 360;
		if (Mathf.Abs(progress) <= moveArmSpeed * Time.deltaTime) {
			currentlyPointing = false;
			armRb.rotation = rotationAngle % 360;
			FinishedPointing(pointContext);
			pointContext = "";
			return;
		}

		// Add speed if direction is negative, subtract if positive. Positive if throw arm 1 state
		else armRb.rotation += ((progress+180 < 0 || pointContext.Equals("ThrowArm_1")) ? moveArmSpeed : -moveArmSpeed) * Time.deltaTime;
	}

	/// Attacks
	public float minAttackTime, maxAttackTime;
	float currentAttackTime;
	Transform enemySpawnpoint;

	void NewAttackTime() {
		currentAttackTime = Random.Range(minAttackTime, maxAttackTime);
	}

	IEnumerator Attack() {
		yield return new WaitForSeconds(currentAttackTime);

		if (Random.Range(1, 3) == 1) DeployShipAttack_Start(); else ThrowArmAttack_Start(); // 50/50 chance for either attack
		//DeployShipAttack_Start();
		//ThrowArmAttack_Start();

		NewAttackTime();
	}

	/// DEPLOY SHIP ATTACK
	void DeployShipAttack_Start() { // Start
		PointAt(playerTransform.position, "Deploy Ship");
	}

	void DeployShipAttack_Deploy() { // Deploy a ship
		Instantiate(enemyPrefab, enemySpawnpoint.position, enemyPrefab.transform.rotation);

		StartCoroutine(DeployShipAttack_End());
	}

	IEnumerator DeployShipAttack_End() { // End the attack
		yield return new WaitForSeconds(2);

		PointAt(transform.position - (transform.up * 10), "Return to Side"); // Return arm to the side
	}

	/// THROW ARM ATTACK
	public GameObject armProjectilePrefab;

	void ThrowArmAttack_Start() {
		PointAt(transform.position + new Vector3(-3, 6), "ThrowArm_1");
	}

	IEnumerator ThrowArmAttack_Throw() {
		yield return new WaitForSeconds(1);

		arm.SetActive(false);
		Instantiate(armProjectilePrefab, arm.transform.position, arm.transform.rotation);
	}

	public void ArmReturned() {
		Vector3 pointAtPlr = (playerTransform.position - armRb.transform.position);
		float rotationAngle = -(Mathf.Atan2(pointAtPlr.x, pointAtPlr.y) * Mathf.Rad2Deg) - 180;
		
		arm.SetActive(true);
		armRb.rotation = rotationAngle % 360;

		PointAt(transform.position - (transform.up * 10), "Return to Side"); // Return arm to the side
	}

	/// Point end events
	void FinishedPointing(string context) {
		if (context == "Deploy Ship") DeployShipAttack_Deploy();
		else if (context == "Return to Side") StartCoroutine(Attack()); // Next attack
		else if (context == "ThrowArm_1") StartCoroutine(ThrowArmAttack_Throw());
	}

	/// Moving up and down the screen
	public float minMoveInterval, maxMoveInterval;
	float currentMoveInterval;
	float targetYPosition;
	public float moveSpeed = 5f;
	bool frozen = false;

	void NewMoveInterval() {
		currentMoveInterval = Random.Range(minMoveInterval, maxMoveInterval);
	}

	void MoveToNewYPos() {
		float yPos = Random.Range(0.0f, 6.0f);
		yPos = Random.Range(1, 3) == 2 ? yPos : -yPos;

		if (Mathf.Abs(transform.position.y - yPos) <= 1) {
			// Make sure movement isn't too small
			MoveToNewYPos();
			return;
		}

		targetYPosition = yPos;
		frozen = false;
	}

	IEnumerator MoveAfterDelay() {
		yield return new WaitForSeconds(currentMoveInterval);

		MoveToNewYPos();
	}

	void Move() {
		if (frozen) return;

		float direction = targetYPosition - transform.position.y;

		float movementScale = moveSpeed * Time.deltaTime;
		transform.position += movementScale * new Vector3(0, (direction < 0) ? -1 : 1);

		if (Mathf.Abs(direction) <= movementScale * 1.5f) {
			frozen = true;
			NewMoveInterval();
			StartCoroutine(MoveAfterDelay());
		}
	}
}
