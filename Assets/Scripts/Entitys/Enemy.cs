using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

	public float speed;
	public float jumpForce = 10;
	public float fallSpeed = 50;

	float lerpSpeed;



	[Header("Movement Speeds")]
	public float walkSpeed;
	public float runSpeed;
	public float sprintSpeed;
	public float acceleration;


	public float sightDistance;

	public GameObject model;

	public Item LeftHandItem;
	public Item RightHandItem;
	public Item TwoHandHandItem;

	//public Transform deformTransform;
	Transform player;


	Rigidbody rb;
	CapsuleCollider cc;
	Animator anim;


	bool isAlive = true;



	Vector3 moveDirection;
	float moveX;
	float moveZ;

	float preVelocityY;

	float runAnimSpeed;
	float lerpAnimSpeed;

	public float isGroundedDelay = 0;
	float isGroundedTimer;

	bool isTrackingPlayer;

	bool isWalking;
	bool isSprinting;
	bool isGrounded;
	bool wasGrounded;
	bool isJumping;

	//bool canMove = true;


	IEnumerator ResetMove;



	private void OnEnable()
	{

		rb = GetComponent<Rigidbody>();
		cc = GetComponent<CapsuleCollider>();

		player = FindFirstObjectByType<PlayerController>().transform;
		//cameraController = cameraContainer.GetComponent<CameraController>();
		//anim = playerModel.transform.GetComponent<Animator>();
		//inventory = GetComponent<PlayerInventory>();
	}

	void Update()
	{
		if (isAlive)
		{
			//float targetSpeed = 0;
			//
			//if (moveDirection.magnitude > 0)
			//{
			//	if (isSprinting)
			//	{
			//		targetSpeed = 1;
			//	}
			//	else if (isWalking)
			//	{
			//		targetSpeed = 0.33f;
			//	}
			//	else
			//	{
			//		targetSpeed = 0.66f;
			//	}
			//}
			//else
			//{
			//	targetSpeed = 0;
			//}
			//
			//if (!canMove)
			//{
			//	targetSpeed = 0;
			//}
			//
			//if (Mathf.Abs(lerpAnimSpeed - targetSpeed) < Time.deltaTime * 4)
			//{
			//	lerpAnimSpeed = targetSpeed;
			//}
			//else if (lerpAnimSpeed < targetSpeed)
			//{
			//	lerpAnimSpeed += Time.deltaTime * 3;
			//}
			//else if (lerpAnimSpeed > targetSpeed)
			//{
			//	lerpAnimSpeed -= Time.deltaTime * 3;
			//}
			//
			//
			//runAnimSpeed = Mathf.Lerp(0f, 1f, lerpAnimSpeed);
			//anim.SetFloat("Speed", runAnimSpeed);

			//if (inventory.heldObject1)
			//{
			//	Tool tool = inventory.heldObject1.GetComponent<Item>() as Tool;
			//	if (tool)
			//	{
			//		if (tool.anim) tool.anim.SetFloat("Speed", runAnimSpeed);
			//	}
			//}
			//if (inventory.heldObject2)
			//{
			//	Tool tool = inventory.heldObject2.GetComponent<Item>() as Tool;
			//	if (tool)
			//	{
			//		if (tool.anim) tool.anim.SetFloat("Speed", runAnimSpeed);
			//	}
			//}

			isGrounded = false;
			//anim.SetBool("isGrounded", false);

			foreach (Collider collider in Physics.OverlapCapsule(cc.bounds.center + (Vector3.down * ((cc.height + 0.1f) / 4)), cc.bounds.center + new Vector3(0, cc.height / 4, 0), cc.radius))
			{
				if (collider.CompareTag("Terrain"))
				{
					isGrounded = true;
					//anim.SetBool("isGrounded", true);
					//Debug.Log("is Grounded");
					break;
				}
			}


			if (isGrounded && !wasGrounded)
			{
				//float weight = 0;
				//if (preVelocityY < -50)
				//{
				//	weight = 1f;
				//}
				//else if (preVelocityY < -30)
				//{
				//	weight = 0.8f;
				//}
				//else if (preVelocityY < -15)
				//{
				//	weight = 0.6f;
				//}
				//else if (preVelocityY < 0)
				//{
				//	weight = 0.4f;
				//}
				//else
				//{
				//	weight = 0;
				//}

				//Debug.Log(preVelocityY);
				//anim.SetLayerWeight(1, weight);
				//anim.SetTrigger("JustLanded");
			}

			preVelocityY = rb.velocity.y;
			wasGrounded = isGrounded;


			if (isGrounded)
			{
				if (isTrackingPlayer && !isJumping && rb.velocity.magnitude < 0.5f)
                {
					Jump();
                }

				if (isJumping)
				{
					//anim.SetBool("isGrounded", false);
				}
				else
				{
					isGroundedTimer = isGroundedDelay;


					//anim.SetBool("isGrounded", true);
				}
			}
			else if (isGroundedTimer <= 0)
			{
				//anim.SetBool("isGrounded", false);
			}
			else
			{
				isGroundedTimer -= Time.deltaTime;
				//anim.SetBool("isGrounded", true);
			}

			//if (anim.GetBool("isGrounded") != isGrounded)
			//anim.SetBool("isGrounded", isGrounded);

			//if (!isGrounded && rb.velocity.y < 2)
			//	Physics.gravity = Vector3.down * fallSpeed;
			//else
			//	Physics.gravity = Vector3.down * 9.81f;

		}
	}

	private void FixedUpdate()
	{


		float maxSpeed;

		if (isSprinting) maxSpeed = sprintSpeed;
		else if (isWalking) maxSpeed = walkSpeed;
		else maxSpeed = runSpeed;


		//moveDirection = new Vector3(moveX, 0, moveZ).normalized;

		float distToPlayer = (transform.position - player.position).magnitude;

		if (distToPlayer < sightDistance)
        {

			moveDirection = (player.position - transform.position).normalized;

			isTrackingPlayer = true;
		}
        else
        {
			moveDirection = Vector3.zero;

			isTrackingPlayer = false;
		}

		moveDirection.y = 0;
		moveDirection.Normalize();


		//lerpSpeed = 1;
		//if (moveDirection.magnitude > 0)
		//{
		//	lerpSpeed += acceleration;
		//	if (lerpSpeed > 1)
		//    {
		//		lerpSpeed = 1;
		//    }
		//}
		//else
		//{
		//	lerpSpeed -= acceleration;
		//
		//	if (lerpSpeed < 0)
		//    {
		//		lerpSpeed = 0;
		//    }
		//}

		rb.velocity = Vector3.ProjectOnPlane(moveDirection * speed * maxSpeed, Vector3.up) + new Vector3(0, rb.velocity.y, 0);


		//if (canMove)
		//{
		//	rb.velocity = Vector3.ProjectOnPlane(moveDirection * speed * maxSpeed * lerpSpeed, Vector3.up) + new Vector3(0, rb.velocity.y, 0);
		//}
		//else
		//{
		//	rb.velocity = new Vector3(0, rb.velocity.y, 0);
		//}


		//if (isGrounded)
		//{
		//	if (rb.velocity.y > 0) rb.velocity = Vector3.ClampMagnitude(Vector3.ProjectOnPlane(rb.velocity, Vector3.up), maxSpeed) + new Vector3(0, rb.velocity.y, 0);
		//	else rb.velocity = Vector3.ClampMagnitude(Vector3.ProjectOnPlane(rb.velocity, Vector3.up), maxSpeed) + new Vector3(0, rb.velocity.y, 0);
		//}
		//else
		//{
		//	rb.velocity = Vector3.ClampMagnitude(Vector3.ProjectOnPlane(rb.velocity, Vector3.up), maxSpeed) + new Vector3(0, rb.velocity.y, 0);
		//}

		//rb.velocity = Vector3.ClampMagnitude(Vector3.ProjectOnPlane(rb.velocity, Vector3.up), maxSpeed) + new Vector3(0, rb.velocity.y, 0);



		if (moveDirection != Vector3.zero)
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(moveDirection), 0.2f);


		//model.transform.position = new Vector3(transform.position.x, Vector3.Lerp(model.transform.position, transform.position, 0.05f).y, transform.position.z);
		model.transform.rotation = transform.rotation;

		//RaycastHit hit;
		//if (!isGrounded)
		//{
		//	if (rb.velocity.y < 0
		//		&& Physics.Raycast(transform.position, rb.velocity.normalized, out hit, rb.velocity.magnitude * 0.5f)
		//		&& hit.transform.CompareTag("Terrain"))
		//	{
		//		if (anim.GetFloat("YSpeed") != 1f)
		//		{
		//			anim.SetFloat("YSpeed", 1f);
		//		}
		//	}
		//	else
		//	{
		//		anim.SetFloat("YSpeed", 0.5f);
		//	}
		//}
	}

	void Jump()
	{
		if (isGrounded)
		{
			isJumping = true;

			rb.constraints &= ~RigidbodyConstraints.FreezePositionY;

			rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);

			StartCoroutine(ResetJumpAfterTime());

		}
	}

	IEnumerator ResetJumpAfterTime()
	{
		yield return new WaitForSeconds(0.3f);

		isJumping = false;
		//anim.ResetTrigger("isJumpPressed");
	}
}
