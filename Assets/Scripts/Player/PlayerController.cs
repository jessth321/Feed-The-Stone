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

	public PhysicMaterial physicsMat;


	public TrailRenderer swordTrail;

	public GameObject heavyBlast;

	public Transform specialCameraTarget;

	[HideInInspector] public Vector2 mouseDelta;

	Rigidbody rb;
	CapsuleCollider cc;
	Animator anim;
	MarchingCubes mc;
	GameManager gm;
	DeckManager dm;

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

	bool isSliding;
	bool isSprinting;
	bool isGrounded = true;
	bool wasGrounded;
	bool isJumping;

	bool canMove = true;
	bool isTakingKeyInput = true;
	bool isLightAttacking = false;
	bool isHeavyAttacking = false;
	bool heavyToggle = false;
	bool doNextAtack = false;
	bool canExitAttack = false;
	bool isCasting = false;

	Vector3 terrainNormal;

	public float temper = 0f;
	float maxTemper = 5;


	//[HideInInspector]
	[SerializeField]
	public Deformation deformation;

	IEnumerator ResetMove;

	Vector3 originalCameraPos;
	Quaternion originalCameraRot;
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
		mc = FindFirstObjectByType<MarchingCubes>();
		gm = FindFirstObjectByType<GameManager>();
		dm = GetComponentInChildren<DeckManager>();
	}

	void Update()
	{

		if (isAlive)
		{


			if (isLightAttacking || isHeavyAttacking)
            {
				canMove = false;
            }

			float targetSpeed = 0;

			if (moveDirection.magnitude > 0)
			{
				if (isSprinting)
				{
					targetSpeed = 1;
				}
				//else if (isWalking)
                //{
				//	targetSpeed = 0.33f;
                //}
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


			terrainNormal = Vector3.zero;

			foreach (Collider collider in Physics.OverlapCapsule(cc.bounds.center + (Vector3.down * ((cc.height + 0.05f) / 4)), cc.bounds.center + new Vector3(0, cc.height / 4, 0), cc.radius))
			{
				if (collider.CompareTag("Terrain"))
				{
					isGrounded = true;
					//anim.SetBool("isGrounded", true);
					//Debug.Log("is Grounded");



					for (int i = 0; i < 9; i++)
                    {
						Vector3 startPoint = transform.position + Vector3.up;
						Vector3 endPoint = transform.position + new Vector3(i % 3 - 1,0, i / 3 % 3 - 1);


						RaycastHit hit;

						if (Physics.Raycast(startPoint, endPoint - startPoint, out hit, 1.5f))
						{
							if (hit.transform.CompareTag("Terrain"))
							{
								if (terrainNormal.y < hit.normal.y)
								{
									terrainNormal = hit.normal;
								}
							}
							Debug.DrawRay(startPoint, endPoint - startPoint, Color.green);
						}
					}

					terrainNormal.Normalize();



					break;
				}
			}
			if (isGrounded && Vector3.Dot(terrainNormal, Vector3.up) < 0.3f && terrainNormal != Vector3.zero)
            {
				physicsMat.dynamicFriction = 0f;
				physicsMat.staticFriction = 0f;
				physicsMat.frictionCombine = PhysicMaterialCombine.Minimum;
			
				rb.AddForce(Vector3.down * 50 * Time.deltaTime, ForceMode.Force);
			
				Debug.Log(Mathf.Abs(Vector3.Dot(terrainNormal, Vector3.up)));
			}
			//else if (moveDirection.magnitude > 0.01f)
			//{
			//	physicsMat.dynamicFriction = 0.2f;
			//	physicsMat.staticFriction = 0.2f;
			//
			//	physicsMat.frictionCombine = PhysicsMaterialCombine.Average;
			//}
			
			else
			{
				physicsMat.dynamicFriction = 1f;
				physicsMat.staticFriction = 1f;
			
				physicsMat.frictionCombine = PhysicMaterialCombine.Average;
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
				//anim.SetLayerWeight(1, weight);
				//anim.SetTrigger("JustLanded");
            }

			preVelocityY = rb.velocity.y;
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
			else if (!isJumping)
			{
				isGroundedTimer -= Time.deltaTime;
				anim.SetBool("isGrounded", true);
			}
            else
			{
				isGroundedTimer -= Time.deltaTime;
			}

			if (!isLightAttacking && !isHeavyAttacking)
            {
				swordTrail.emitting = false;
            }


			//if no

			//if (isGrounded)
            //{
			//	isTakingKeyInput = true;
			//}
            //else
			//{
			//	isTakingKeyInput = false;
			//}

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
		//else if (isWalking) maxSpeed = walkSpeed;
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

		Vector3 tempTerrainNormal = terrainNormal;

		if (canMove)
		{
			rb.velocity = Vector3.ProjectOnPlane(moveDirection * speed * maxSpeed * lerpSpeed, Vector3.up) + new Vector3(0, rb.velocity.y, 0);

			//rb.AddForce((moveDirection * speed * maxSpeed + Vector3.ProjectOnPlane(terrainNormal, Vector3.up) * speed));
		}
        else
        {
			rb.velocity = new Vector3(0, rb.velocity.y, 0);
		}


		if (terrainNormal != Vector3.zero && Vector3.Dot(terrainNormal, Vector3.up) < 0.4f)
		{
			tempTerrainNormal = Vector3.ProjectOnPlane(tempTerrainNormal, Vector3.up);
			Vector3 velocity = Vector3.ProjectOnPlane(new Vector3(rb.velocity.x, 0, rb.velocity.z), tempTerrainNormal);
			rb.velocity = new Vector3 (velocity.x, rb.velocity.y, velocity.z);
			//rb.AddForce(Vector3.down * 5f);
			Debug.DrawRay(transform.position, rb.velocity, Color.yellow);
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
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(moveDirection), 0.4f);


		playerModel.transform.position = new Vector3(transform.position.x, Vector3.Lerp(playerModel.transform.position, transform.position, 0.05f).y, transform.position.z);
		playerModel.transform.rotation = transform.rotation;

		RaycastHit hit;
		if (!isGrounded)
        {
			if (rb.velocity.y < 0
				&& Physics.Raycast(transform.position, rb.velocity.normalized, out hit, rb.velocity.magnitude * 0.5f) 
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
		if (isGrounded && isTakingKeyInput)
		{
			isJumping = true;

			rb.constraints &= ~RigidbodyConstraints.FreezePositionY;
		
			rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);

			StartCoroutine(ResetJumpAfterTime());

		}
	}

	IEnumerator ShuffleForward()
	{
		yield return new WaitForSeconds(0.12f);
		rb.AddForce(transform.forward * 25, ForceMode.VelocityChange);
	}

	public void AddTemper(float amount)
    {
		temper += amount;

		if (temper < 0)
        {
			temper = 0;
        }
		if (temper > maxTemper)
        {
			temper = maxTemper;
        }
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
		//if (inventory.heldItem2 == null)
        //{
		//	if (isGrounded)
		//	{
		//		anim.SetTrigger("RightPunch");
		//		//StartCoroutine(ShuffleForward());
		//
		//		canMove = false;
		//
		//		if (ResetMove != null)
		//		{
		//			StopCoroutine(ResetMove);
		//		}
		//		ResetMove = ResetCanMoveAfterTime(0.36f);
		//		StartCoroutine(ResetMove);
		//	}
		//}
        //else if (isGrounded)
        //{
		//	switch (inventory.heldItem2.itemType) 
		//	{
		//		case ItemType.TOOL:
		//
		//			if (canMove)
		//			{
		//				anim.SetTrigger("UseTool");
		//				anim.SetFloat("ToolType", (inventory.heldItem2 as Tool).typeAnimID);
		//				inventory.heldObject2.GetComponent<Tool>().PlayAnimation();
		//				canMove = false;
		//
		//				if (ResetMove != null)
		//				{
		//					StopCoroutine(ResetMove);
		//				}
		//
		//				ResetMove = ResetCanMoveAfterTime(0.6f);
		//				StartCoroutine(ResetMove);
		//			}
		//
		//			break;
		//			//StartCoroutine(ShuffleForward());
		//
		//
		//	}
        //}

		if (isLightAttacking && !heavyToggle)
        {
			doNextAtack = true;
        }
        else
        {
			if (heavyToggle)
            {
				if (isGrounded && isTakingKeyInput)
				{
					anim.SetTrigger("DoHeavyAttack");
					isLightAttacking = false;
					doNextAtack = false;
					isHeavyAttacking = true;
					ResetCanMoveAfterTime(2.25f);
					ExitHeavyAttackAfterTime();
				}
			}
            else
            {
				if (isGrounded && isTakingKeyInput)
				{
					anim.SetTrigger("DoLightAttack");
					isLightAttacking = true;
					//CheckExitLightAttack();
				}
			}
			
		}
	}

	public void SpawnHeavyBlast()
    {

		if (temper > 1f || Mathf.Approximately(temper, 1f))
        {
			Instantiate(heavyBlast, swordTrail.transform.position, Quaternion.LookRotation(transform.forward));

			AddTemper(-1f);
		}
		
    }

	public void CheckExitLightAttack()
    {
		if  (isLightAttacking)
        {
			if (doNextAtack)
            {
				isLightAttacking = true;
            }
            else
            {
				anim.SetTrigger("ExitLightAttack");
				isLightAttacking = false;
				canMove = true;
			}
			doNextAtack = false;
        }
    }

	async void ExitHeavyAttackAfterTime()
    {
		await Task.Delay(2250);

		isHeavyAttacking = false;
		anim.SetTrigger("ExitHeavyAttack");
		isTakingKeyInput = true;
	}


	void OnUseLHItem()
	{
		//if (inventory.heldItem1 == null)
		//{
		//	if (isGrounded)
		//	{
		//		anim.SetTrigger("LeftPunch");
		//		//StartCoroutine(ShuffleForward());
		//
		//		canMove = false;
		//
		//		if (ResetMove != null)
		//		{
		//			StopCoroutine(ResetMove);
		//		}
		//		ResetMove = ResetCanMoveAfterTime(0.32f);
		//		StartCoroutine(ResetMove);
		//	}
		//}
		//else if (isGrounded)
		//{
		//	switch (inventory.heldItem1.itemType)
		//	{
		//		case ItemType.TOOL:
		//
		//			if (canMove)
		//			{
		//				anim.SetTrigger("UseTool");
		//				anim.SetFloat("ToolType", (inventory.heldItem1 as Tool).typeAnimID);
		//				inventory.heldObject1.GetComponent<Tool>().PlayAnimation();
		//				canMove = false;
		//
		//				if (ResetMove != null)
		//				{
		//					StopCoroutine(ResetMove);
		//				}
		//
		//				ResetMove = ResetCanMoveAfterTime(0.6f);
		//				StartCoroutine(ResetMove);
		//			}
		//			break;
		//	}
		//}

		if (isGrounded && isTakingKeyInput && !isLightAttacking && !isHeavyAttacking)
		{
			anim.ResetTrigger("ExitCast");
			anim.SetTrigger("DoCast");
			isTakingKeyInput = false;
			canMove = false;
		
		
			DeformAfterTime();
			ResetCanMoveAfterTime(0.7f);
			ExitCastAfterTime(0.7f);
		}



	}
	async void DeformAfterTime()
	{


		Vector3 deformPos = deformTransform.position; ;
		RaycastHit hit;
		if (Physics.Raycast(transform.position + (Vector3.up * 2f), (deformPos - (transform.position + Vector3.up * 2f)).normalized, out hit, 10f))
		{
			if (hit.transform.CompareTag("Terrain"))
			{
				deformPos = hit.point;
			}
		}

		List<Vector3> points = new List<Vector3>();

		int deformSize = Mathf.CeilToInt(deformation.radius * 2);

		for (int x = 0; x < deformSize; x++)
		{
			for (int y = 0; y < deformSize; y++)
			{
				for (int z = 0; z < deformSize; z++)
				{
					int halfDeformSize = Mathf.CeilToInt(deformation.radius);
					points.Add(deformPos + new Vector3(x - halfDeformSize, y - halfDeformSize, z - halfDeformSize));
				}
			}
		}

		Dictionary<Vector3Int, PointData> preDeformPointData = mc.SamplePoints(points);
		//Debug.Log(pointdata[0].terrainType);

		//LogTerrainType(preDeformPointData);

		
		await Task.Delay(130);

		mc.DeformChunks(new Deformation(deformPos, deformation.radius, deformation.blend, deformation.noiseFreq, deformation.noiseAmp, (float)deformation.noiseOct, deformation.noiseLac, deformation.noisePers), 0.1f);


		await Task.Delay(170);

		Dictionary<Vector3Int, PointData> pointdata = mc.SamplePoints(points);
		//Debug.Log(pointdata[0].terrainType);

		//LogTerrainType(pointdata);
		//Debug.Log(pointdata[0].density);

		Dictionary<Vector3Int, PointData> pointDataToSpawn = new Dictionary<Vector3Int, PointData>();


		foreach (Vector3Int coord in pointdata.Keys)
		{

			//if (preDeformPointData[i].density < mc.isoLevel && pointdata[i].density >= mc.isoLevel)
			if (preDeformPointData[coord].density < mc.isoLevel && pointdata[coord].density >= mc.isoLevel)
			{
				if (true)
				{
					pointDataToSpawn.Add(coord, pointdata[coord]);
				}
				//GameObject spawned = Instantiate(toSpawn, coord, Quaternion.identity);
				//GameObject objToSpawn;

				//if (preDeformPointData[coord].terrainType > 1f)

				//GameObject spawned = Instantiate(toSpawn), coord, Quaternion.identity);

				//spawned.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = coord.ToString();
				//spawned.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = pointdata[coord].terrainType.ToString();
			}
		}

		gm.SpawnItems(pointDataToSpawn, deformPos, 5000, 500, 5, deformation.radius * 2);
	}

	void OnFDown()
    {
		if (isGrounded && isTakingKeyInput)
		{
			EnterSpecial();
		}
    }

	async void EnterSpecial()
	{
		canMove = false;
		isTakingKeyInput = false;
		cameraController.inSpecialState = true;

		GameObject cam = cameraContainer.GetChild(0).GetChild(0).gameObject;

		originalCameraPos = cam.transform.position;
		originalCameraRot = cam.transform.rotation;

		anim.SetTrigger("InitiateSpecial");
		float timer = 0f;
		float lerp = 0f;

		while (timer < 0.5f)
        {
			timer += Time.deltaTime;

			lerp += Time.deltaTime * 5f;

			cam.transform.position = Vector3.Lerp(originalCameraPos, specialCameraTarget.position, Mathf.SmoothStep(0, 1, lerp));
			cam.transform.rotation = Quaternion.Lerp(originalCameraRot, specialCameraTarget.rotation, Mathf.SmoothStep(0, 1, lerp));

			await Task.Yield();
		}

		anim.speed = 0f;

		dm.DrawCards(8);

		while (!dm.areCardsDrawn)
        {
			await Task.Yield();
        }

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		//Time.timeScale = 0f;

		await Task.Delay(5000);

		//ExitSpecial();
    }
    async void ExitSpecial()
	{        
		anim.speed = 1f;
		Time.timeScale = 1f;

		GameObject cam = cameraContainer.GetChild(0).GetChild(0).gameObject;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		float timer = 0f;
		float lerp = 0f;

		while (timer < 1.25f)
		{
			timer += Time.deltaTime;

			lerp += Time.deltaTime * 5f;

			cam.transform.position = Vector3.Lerp(specialCameraTarget.position, originalCameraPos,  Mathf.SmoothStep(0, 1, lerp));
			cam.transform.rotation = Quaternion.Lerp(specialCameraTarget.rotation, originalCameraRot, Mathf.SmoothStep(0, 1, lerp));

			await Task.Yield();
		}


		canMove = true;
		//Time.timeScale = 1f;
		isTakingKeyInput = true;
		anim.SetTrigger("ExitSpecial");
	}

	async void ExitCastAfterTime(float time)
	{
		await Task.Delay((int) (time * 1000));
		anim.SetTrigger("ExitCast");
		isTakingKeyInput = true;
	}

	void OnSpacebarDown()
	{
		//anim.SetTrigger("isJumpPressed");
		//StartCoroutine(ResetJumpAfterTime());

		anim.SetTrigger("isJumpPressed");
		Jump();



	}

	public void SetCanExitAttack(bool state)
    {
		isLightAttacking = state;
    }

	public void EnterCast()
    {

    }


	void OnShiftDown()
	{
		isSprinting = !isSprinting;
		heavyToggle = !heavyToggle;

	}
	void OnControlDown()
	{
		isSliding = !isSliding;
		anim.SetBool("isSliding", isSliding);
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
		brb.velocity = cameraController.transform.GetChild(0).GetChild(0).forward * 50;
	}

	void OnThrowStickyLight()
	{

		GameObject bomb = Instantiate(stickyBombBig, transform.position + Vector3.up * 3, Quaternion.identity);
		Rigidbody brb = bomb.GetComponent<Rigidbody>();
		brb.velocity = cameraController.transform.GetChild(0).GetChild(0).forward * 50;
	}

	IEnumerator ResetJumpAfterTime()
	{
		yield return new WaitForSeconds(0.3f);

		isJumping = false;
		anim.ResetTrigger("isJumpPressed");
	}
	async void ResetCanMoveAfterTime(float time)
	{
		await Task.Delay((int)(time * 1000));

		canMove = true;
	}
}
