using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations.Rigging;
using TMPro;

public class PlayerInventory : MonoBehaviour
{

	public Item itemToEquip;
	public Item itemToEquip2;

	public List<Item> inventoryList = new List<Item>();

	public GameObject inventoryCanvas;
	public GameObject inventorySlotContainer;
	List<Image> inventoryImages = new List<Image>();
	List<TextMeshProUGUI> inventoryTMPs = new List<TextMeshProUGUI>();
	List<TextMeshProUGUI> inventoryCountTMPs = new List<TextMeshProUGUI>();

	public Item[] handSlots = new Item[6];

	public Image[] slotImages = new Image[6];

	public Sprite transparentSprite;

	public Transform toolContainer;

	public GameObject heldObject1;
	public GameObject heldObject2;
	public Item heldItem1;
	public Item heldItem2;

	public RigBuilder rigBuilder;
	public TwoBoneIKConstraint leftIK;
	public TwoBoneIKConstraint rightIK;

	Item draggedItem;

	bool isUsingSecondary;

	void Start()
	{
		for (int i = 0; i < 18; i++)
		{
			inventoryList.Add(null);
			inventoryImages.Add(null);
			inventoryCountTMPs.Add(null);
		}

		for (int i = 0; i < inventoryList.Count; i++)
		{
			inventoryImages[i] = inventorySlotContainer.transform.GetChild(i).GetComponent<Image>();
			inventoryCountTMPs[i] = inventorySlotContainer.transform.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>();
		}

		//EquipToHand(itemToEquip2, 0);
		//EquipToHand(itemToEquip2, 1);
		//EquipToHand(itemToEquip2, 2);
		//EquipToHand(itemToEquip2, 3);
		EquipToHand(itemToEquip, 0);
		//EquipToHand(itemToEquip, 0);
		//EquipToHand(itemToEquip2, 2);
		//EquipToHand(itemToEquip2, 3);


		UpdateToolSprites();

		UpdateHeldObjects();

		UpdateInventory();
	}

	// Update is called once per frame
	void Update()
	{
		
	}

	void UpdateToolSprites()
	{


		for (int i = 0; i < 6; i++)
		{
			if (handSlots[i])
			{
				Weapon weapon = handSlots[i] as Weapon;
				Tool tool = handSlots[i] as Tool;

				if (weapon)
				{

					slotImages[i].transform.parent.gameObject.SetActive(true);

					slotImages[i].sprite = weapon.spriteUI;

					if (i == 4)
					{
						slotImages[0].transform.parent.gameObject.SetActive(false);
						slotImages[1].transform.parent.gameObject.SetActive(false);
					}
					if (i == 5)
					{
						slotImages[2].transform.parent.gameObject.SetActive(false);
						slotImages[3].transform.parent.gameObject.SetActive(false);
					}
				}

				if (tool)
				{

					slotImages[i].transform.parent.gameObject.SetActive(true);

					slotImages[i].sprite = tool.spriteUI;

					if (i == 4)
					{
						slotImages[0].transform.parent.gameObject.SetActive(false);
						slotImages[1].transform.parent.gameObject.SetActive(false);
					}
					if (i == 5)
					{
						slotImages[2].transform.parent.gameObject.SetActive(false);
						slotImages[3].transform.parent.gameObject.SetActive(false);
					}
				}
			}
			else
			{
				slotImages[i].transform.parent.gameObject.SetActive(false);
			}
		}
	}

	void UpdateHeldObjects()
    {

		if (heldObject1)
		{
			Destroy(heldObject1);
			heldObject1 = null;
		}
		if (heldObject1)
		{
			Destroy(heldObject2);
			heldObject2 = null;
		}

		if (isUsingSecondary)
        {
			if (handSlots[5] != null)
			{
				heldObject1 = Instantiate(handSlots[5].prefab, toolContainer);
				heldObject2 = heldObject1;
				heldItem1 = handSlots[2];
				heldItem2 = handSlots[2];

				leftIK.data.target = heldObject1.transform.GetChild(1);
				rightIK.data.target = heldObject1.transform.GetChild(2);
			}
			else
			{
				if (handSlots[2] != null && handSlots[3] != null)
				{

					heldObject1 = Instantiate(handSlots[2].prefab, toolContainer);
					heldObject2 = Instantiate(handSlots[3].prefab, toolContainer);

					heldItem1 = handSlots[2];
					heldItem2 = handSlots[3];

					leftIK.data.target = heldObject1.transform.GetChild(1);
					rightIK.data.target = heldObject2.transform.GetChild(1);

				}
				else if (handSlots[2] != null)
				{

					heldObject1 = Instantiate(handSlots[2].prefab, toolContainer);
					heldItem1 = handSlots[2];

					rightIK.data.target = heldObject1.transform.GetChild(1);
				}
				else if (handSlots[3] != null)
				{
					heldObject2 = Instantiate(handSlots[3].prefab, toolContainer);
					heldItem2 = handSlots[3];

					leftIK.data.target = heldObject2.transform.GetChild(1);
				}
                else
				{
					rightIK.data.target = null;
					leftIK.data.target = null;
					heldItem1 = null;
					heldItem2 = null;
				}
			}
		}
        else
        {
			if (handSlots[4] != null)
			{
				heldObject1 = Instantiate(handSlots[4].prefab, toolContainer);
				heldObject2 = heldObject1;
				heldItem1 = handSlots[4];
				heldItem2 = handSlots[4];

				leftIK.data.target = heldObject1.transform.GetChild(1);
				rightIK.data.target = heldObject1.transform.GetChild(2);
			}
			else
			{
				if (handSlots[0] != null && handSlots[1] != null)
				{

					heldObject1 = Instantiate(handSlots[0].prefab, toolContainer);
					heldObject2 = Instantiate(handSlots[1].prefab, toolContainer);

					heldItem1 = handSlots[0];
					heldItem2 = handSlots[1];

					leftIK.data.target = heldObject1.transform.GetChild(1);
					rightIK.data.target = heldObject2.transform.GetChild(1);

				}
				else if (handSlots[0] != null)
				{

					heldObject1 = Instantiate(handSlots[0].prefab, toolContainer);
					heldItem1 = handSlots[0];

					rightIK.data.target = heldObject1.transform.GetChild(1);
				}
				else if (handSlots[1] != null)
				{
					heldObject2 = Instantiate(handSlots[1].prefab, toolContainer);
					heldItem2 = handSlots[1];

					leftIK.data.target = heldObject2.transform.GetChild(1);
				}
                else
                {
					rightIK.data.target = null;
					leftIK.data.target = null;
					heldItem1 = null;
					heldItem2 = null;
				}
			}
		}
		rigBuilder.Build();
		
	}

	void UpdateInventory()
    {
		for (int i = 0; i < inventoryList.Count; i++)
        {
			if (inventoryList[i] != null)
            {
				inventoryImages[i].sprite = inventoryList[i].spriteUI;
				inventoryImages[i].color = Color.white;
				//inventoryTMPs[i].text = inventoryList[i].itemName;
				inventoryCountTMPs[i].text = inventoryList[i].amountHeld.ToString();
            }
            else
			{
				inventoryImages[i].sprite = transparentSprite;

				inventoryImages[i].color = new Color(0.25f, 0.25f, 0.3f, 1);
				
				//inventoryTMPs[i].text = "";
				inventoryCountTMPs[i].text = "";
			}

        }
    }

	public bool CanStoreItem(Item item)
    {
		for (int i = 0; i < inventoryList.Count; i++)
        {
			if (inventoryList[i] == null)
            {
				return true;
            }
			if (inventoryList[i] != null && item.id == inventoryList[i].id && inventoryList[i].amountHeld < inventoryList[i].maxStack)
            {
				return true;
            }
        }
		return false;
    }

	public void CollectItem(Item item)
    {
		int itemIndex = ContainsItem(item);
		//Debug.Log(itemIndex);
		if (itemIndex != -1)
        {
			inventoryList[itemIndex].amountHeld += 1;
        }
        else
		{
			for (int i = 0; i < inventoryList.Count; i++)
			{
				if (inventoryList[i] == null)
				{
					inventoryList[i] = item;
					break;
				}
			}
		}
		UpdateInventory();
    }

	int ContainsItem(Item item)
    {
		for (int i = 0; i < inventoryList.Count; i++)
        {
			if (inventoryList[i] != null && item.id == inventoryList[i].id && inventoryList[i].amountHeld < inventoryList[i].maxStack)
            {
				return i;
            }
        }
		return -1;
    }

	public Item GetItemFromHand(int slot)
	{
		return handSlots[slot];
	}

	public void EquipToHand(Item item, int slot)
	{

		Weapon weapon = item as Weapon;
		Tool tool = item as Tool;



		
		if (weapon)
		{
			if (weapon.isTwoHanded)
			{
				slot = (slot/2) + 4;

				handSlots[slot] = item;
				
			}
			else
			{
				handSlots[slot] = item;
			}
		}


		if (tool)
		{
			if (tool.isTwoHanded)
			{
				slot = (slot / 2) + 4;

				handSlots[slot] = item;
			}
			else
			{
				handSlots[slot] = item;
			}
		}


		UpdateToolSprites();

		UpdateHeldObjects();

	}


	public void OnToggleInventory()
    {
		if (inventoryCanvas.activeInHierarchy)
        {
			inventoryCanvas.SetActive(false);
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;

			//Time.timeScale = 1;
        }
        else
        {
			inventoryCanvas.SetActive(true);
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.Confined;
			//Time.timeScale = 0;
		}
    }

	public void OnSwapHeld()
    {
		isUsingSecondary = !isUsingSecondary;
		UpdateToolSprites();
		UpdateHeldObjects();

	}

	public void SelectItem()
    {

    }
}
