using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	//[SerializeField] GameObject quickAttackPrefab;
	public GameObject stickyBombSmall;
	public GameObject stickyBombBig;

	public float speed;
	public float jumpForce = 10;
	public float fallSpeed = 50;

	float lerpSpeed;

[Header("Movement Speeds")]
	public float walkSpeed;
	public float runSpeed;
	public float sprintSpeed;
	public float magnetDistance;
	public float magnetSpeed;

	public GameObject playerModel;

	public Item LeftHandItem;
	public Item RightHandItem;
	public Item TwoHandHandItem;

	public Transform toolContainer;
	public Transform deformTransform;

	[HideInInspector] public Vector2 mouseDelta;

	Rigidbody rb;
	CapsuleCollider cc;
	Animator anim;

	PlayerInventory inventory;

	bool isAlive = true;

	Vector3 moveDirection;
	float moveX;
	float moveZ;

	float preVelocityY;

	float runAnimSpeed;	
	float lerpAnimSpeed;

	public float isGroundedDelay = 0;
	float isGroundedTimer;

	bool isWalking;
	bool isSprinting;
	bool isGrounded = true;
	bool wasGrounded;
	bool isJumping;

	bool canMove = true;


	IEnumerator ResetMove;


	[SerializeField] Transform cameraContainer;
	CameraController cameraController;

	[HideInInspector] public Interactable currentInteractible;


	private void OnEnable()
	{
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;

		rb = GetComponent<Rigidbody>();
		cc = GetComponent<CapsuleCollider>();
		cameraController = cameraContainer.GetComponent<CameraController>();
		anim = playerModel.transform.GetComponent<Animator>();
		inventory = GetComponent<PlayerInventory>();
	}

	void Update()
	{
		if (isAlive)
		{


			float targetSpeed = 0;

			if (moveDirection.magnitude > 0)
			{
				if (isSprinting)
				{
					targetSpeed = 1;
				}
				else if (isWalking)
                {
					targetSpeed = 0.33f;
                }
                else
                {
					targetSpeed = 0.66f;
                }
			}
            else
            {
				targetSpeed = 0;
            }

			if (!canMove)
            {
				targetSpeed = 0;
            }

			if (Mathf.Abs(lerpAnimSpeed - targetSpeed) < Time.deltaTime * 4)
			{
				lerpAnimSpeed = targetSpeed;
			}
			else if (lerpAnimSpeed < targetSpeed)
			{
				lerpAnimSpeed += Time.deltaTime * 3;
			}
			else if (lerpAnimSpeed > targetSpeed)
			{
				lerpAnimSpeed -= Time.deltaTime * 3;
			}
			

			runAnimSpeed = Mathf.Lerp(0f, 1f, lerpAnimSpeed);
			anim.SetFloat("Speed", runAnimSpeed);

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
			anim.SetBool("isGrounded", false);

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
				float weight = 0;
				if (preVelocityY < -50)
                {
					weight = 1f;
				}
				else if (preVelocityY < -30)
                {
					weight = 0.8f;
				}
				else if (preVelocityY < -15)
                {
					weight = 0.6f;
				}
				else if (preVelocityY < 0)
                {
					weight = 0.4f;
				}
                else
                {
					weight = 0;
				}

				//Debug.Log(preVelocityY);
				anim.SetLayerWeight(1, weight);
				anim.SetTrigger("JustLanded");
            }

			preVelocityY = rb.linearVelocity.y;
			wasGrounded = isGrounded;

			
			if (isGrounded)
			{
				if (isJumping)
                {
					anim.SetBool("isGrounded", false);
				}
                else
				{
					isGroundedTimer = isGroundedDelay;
					anim.SetBool("isGrounded", true);
				}
			}
			else if (isGroundedTimer <= 0)
			{
				anim.SetBool("isGrounded", false);
			}
			else
			{
				isGroundedTimer -= Time.deltaTime;
				anim.SetBool("isGrounded", true);
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


		moveDirection = new Vector3(moveX, 0, moveZ).normalized;

		moveDirection = cameraContainer.TransformDirection(moveDirection);
		moveDirection.y = 0;
		moveDirection.Normalize();


		lerpSpeed = 1;
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

		if (canMove)
		{
			rb.linearVelocity = Vector3.ProjectOnPlane(moveDirection * speed * maxSpeed * lerpSpeed, Vector3.up) + new Vector3(0, rb.linearVelocity.y, 0);
		}
        else
        {
			rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
		}

		
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


		playerModel.transform.position = new Vector3(transform.position.x, Vector3.Lerp(playerModel.transform.position, transform.position, 0.05f).y, transform.position.z);
		playerModel.transform.rotation = transform.rotation;

		RaycastHit hit;
		if (!isGrounded)
        {
			if (rb.linearVelocity.y < 0
				&& Physics.Raycast(transform.position, rb.linearVelocity.normalized, out hit, rb.linearVelocity.magnitude * 0.5f) 
				&& hit.transform.CompareTag("Terrain"))
			{
				if (anim.GetFloat("YSpeed") != 1f)
				{
					anim.SetFloat("YSpeed", 1f);
				}
			}
            else
			{
				anim.SetFloat("YSpeed", 0.5f);
			}
		}
	}

	void Jump()
	{
		if (isGrounded)
		{
			isJumping = true;

			rb.constraints &= ~RigidbodyConstraints.FreezePositionY;
		
			rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);

			StartCoroutine(ResetJumpAfterTime());

		}
	}

	IEnumerator ShuffleForward()
	{
		yield return new WaitForSeconds(0.12f);
		rb.AddForce(transform.forward * 25, ForceMode.VelocityChange);
	}

	void OnMoveX(InputValue val)
	{
		moveX = val.Get<float>();
	}
	void OnMoveZ(InputValue val)
	{
		moveZ = val.Get<float>();
	}

	void OnMouseDelta(InputValue val)
	{
		mouseDelta = val.Get<Vector2>();
	}

	void OnUseRHItem()
	{
		if (inventory.heldItem2 == null)
        {
			if (isGrounded)
			{
				anim.SetTrigger("RightPunch");
				//StartCoroutine(ShuffleForward());

				canMove = false;

				if (ResetMove != null)
				{
					StopCoroutine(ResetMove);
				}
				ResetMove = ResetCanMoveAfterTime(0.36f);
				StartCoroutine(ResetMove);
			}
		}
        else if (isGrounded)
        {
			switch (inventory.heldItem2.itemType) 
			{
				case ItemType.TOOL:

					if (canMove)
					{
						anim.SetTrigger("UseTool");
						anim.SetFloat("ToolType", (inventory.heldItem2 as Tool).typeAnimID);
						inventory.heldObject2.GetComponent<Tool>().PlayAnimation();
						canMove = false;

						if (ResetMove != null)
						{
							StopCoroutine(ResetMove);
						}

						ResetMove = ResetCanMoveAfterTime(0.6f);
						StartCoroutine(ResetMove);
					}

					break;
					//StartCoroutine(ShuffleForward());


			}
        }
		
		
	}
	void OnUseLHItem()
	{
		if (inventory.heldItem1 == null)
        {
			if (isGrounded)
			{
				anim.SetTrigger("LeftPunch");
				//StartCoroutine(ShuffleForward());

				canMove = false;

				if (ResetMove != null)
				{
					StopCoroutine(ResetMove);
				}
				ResetMove = ResetCanMoveAfterTime(0.32f);
				StartCoroutine(ResetMove);
			}
		}
		else if (isGrounded)
		{
			switch (inventory.heldItem1.itemType)
			{
				case ItemType.TOOL:

					if (canMove)
					{
						anim.SetTrigger("UseTool");
						anim.SetFloat("ToolType", (inventory.heldItem1 as Tool).typeAnimID);
						inventory.heldObject1.GetComponent<Tool>().PlayAnimation();
						canMove = false;

						if (ResetMove != null)
						{
							StopCoroutine(ResetMove);
						}

						ResetMove = ResetCanMoveAfterTime(0.6f);
						StartCoroutine(ResetMove);
					}
					break;
			}
		}


	}

	void OnSpacebarDown()
	{
		//anim.SetTrigger("isJumpPressed");
		//StartCoroutine(ResetJumpAfterTime());

		anim.SetTrigger("isJumpPressed");
		Jump();

	}



	void OnShiftDown()
	{
		isSprinting = !isSprinting;
	}
	void OnControlDown()
	{
		isWalking = !isWalking;
	}

	void OnInteract()
    {
		if (currentInteractible)
        {
			currentInteractible.Invoke("OnInteract", 0);
        }
    }


	void OnThrowStickyBomb()
	{
		GameObject bomb = Instantiate(stickyBombSmall, transform.position + Vector3.up * 3, Quaternion.identity);
		Rigidbody brb = bomb.GetComponent<Rigidbody>();
		brb.linearVelocity = cameraController.transform.GetChild(0).GetChild(0).forward * 50;
	}

	void OnThrowStickyLight()
	{

		GameObject bomb = Instantiate(stickyBombBig, transform.position + Vector3.up * 3, Quaternion.identity);
		Rigidbody brb = bomb.GetComponent<Rigidbody>();
		brb.linearVelocity = cameraController.transform.GetChild(0).GetChild(0).forward * 50;
	}

	IEnumerator ResetJumpAfterTime()
	{
		yield return new WaitForSeconds(0.3f);

		isJumping = false;
		anim.ResetTrigger("isJumpPressed");
	}
	IEnumerator ResetCanMoveAfterTime(float time)
	{
		yield return new WaitForSeconds(time);

		canMove = true;
	}
}
