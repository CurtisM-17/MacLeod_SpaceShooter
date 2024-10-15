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
		else armRb.rotation += (
			(progress+180 < 0 || pointContext.Equals("ThrowArm_1"))
			? moveArmSpeed : -moveArmSpeed) * Time.deltaTime;
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

		//if (Random.Range(1, 3) == 1) DeployShipAttack(); else ThrowArmAttack(); // 50/50 chance for either attack
		//DeployShipAttack_Start();
		ThrowArmAttack_Start();

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
		armRb.rotation = rotationAngle % 360;

		arm.SetActive(true);

		//PointAt(transform.position - (transform.up * 10), "Return to Side"); // Return arm to the side
	}

	/// Point end events
	void FinishedPointing(string context) {
		if (context == "Deploy Ship") DeployShipAttack_Deploy();
		else if (context == "Return to Side") StartCoroutine(Attack()); // Next attack
		else if (context == "ThrowArm_1") StartCoroutine(ThrowArmAttack_Throw());
	}
}
