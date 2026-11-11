using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
	PlayerController pc;
	PlayerInventory pi;
	MarchingCubes mc;

	public Canvas geodeInteractCanvas;

	public List<GameObject> itemIDList = new List<GameObject>();

	public List<Item> spawnedItems = new List<Item>();

	public static Transform geodeContainer;
	public static List<Interactable> interactables = new List<Interactable>();
	public List<GameObject> chests = new List<GameObject>();

    private void Awake()
    {
		geodeContainer = new GameObject("Geode Container").transform;

	}


    // Start is called before the first frame update
    void Start()
	{
		pc = FindFirstObjectByType<PlayerController>();
		pi = FindFirstObjectByType<PlayerInventory>();
		mc = FindFirstObjectByType<MarchingCubes>();


		//geodeInteractCanvas.SetActive(true);
	}

	// Update is called once per frame
	void Update()
	{
		for (int i = spawnedItems.Count - 1; i >= 0; i--)
        {
			
			if (pi.CanStoreItem(spawnedItems[i]) && (spawnedItems[i].transform.position - pc.transform.position).magnitude < pc.magnetDistance)
            {
				spawnedItems[i].transform.position = Vector3.MoveTowards(spawnedItems[i].transform.position, 
					pc.transform.position, 
					pc.magnetSpeed * Time.deltaTime * (pc.magnetDistance - (spawnedItems[i].transform.position - pc.transform.position).magnitude));

				if ((spawnedItems[i].transform.position - pc.transform.position).magnitude < 1)
                {
					//Debug.Log("Collected");
					pi.CollectItem(spawnedItems[i]);
					Destroy(spawnedItems[i].gameObject);
					spawnedItems.RemoveAt(i);
                }
            }
        }

		float closestDistance = Mathf.Infinity;


		pc.currentInteractible = null;

		List<int> indicesToRemove = new List<int>();

		//List<Chunk> nearbyChunks = mc.GetNearbyChunks();
		//
		//for (int c = 0; c < nearbyChunks.Count; c++)
        //{
		//	
		//}

		for (int i = 0; i < interactables.Count; i++)
		{
			if (interactables[i] != null && interactables[i].enabled)
			{
				float distance = (pc.transform.position - interactables[i].transform.position).magnitude;
				if (distance < 5)
				{
					if (distance < closestDistance)
					{
						closestDistance = distance;
						pc.currentInteractible = interactables[i];
					}
				}
			}
			else
			{
				indicesToRemove.Add(i);
			}

		}

		if (pc.currentInteractible != null)
        {
			geodeInteractCanvas.enabled = true;

			geodeInteractCanvas.transform.position = pc.currentInteractible.transform.position;

			geodeInteractCanvas.transform.GetChild(0).LookAt(Camera.main.transform);
			geodeInteractCanvas.transform.GetChild(0).Rotate(0, 180, 0);
        }
        else
		{
			geodeInteractCanvas.enabled = false;
		}

		
		for (int i = indicesToRemove.Count - 1; i >= 0; i--)
        {
			interactables.RemoveAt(indicesToRemove[i]);

        }
	}

	public void SpawnItems(Dictionary<Vector3Int, PointData> itemsToSpawn, Vector3 deformCenter, float itemLifeTime,  float explosionForce = 0, float explosionVariation = 0, float explosionRadius = 10)
	{
		foreach (Vector3Int point in itemsToSpawn.Keys)
		{

			GameObject itemObject = LookupItemObject(itemsToSpawn[point]);

			if ((itemObject.name == "Gold Ore" && Random.Range(0, 10) < 6) || (itemObject.name == "Iron Ore" && Random.Range(0, 10) < 6) || Random.Range(0, 10) < 1)
            {

				//Rigidbody rb;
				//
				//if (item.TryGetComponent<Rigidbody>(out rb))
				//{
				//	rb.AddExplosionForce(explosionForce, deformCenter, explosionRadius, explosionForce * 0.1f);
				//}


				spawnedItems.Add(Instantiate(itemObject, point, Quaternion.identity).GetComponent<Item>());

				Item item = itemObject.GetComponent<Item>();
				if (item.particlePrefab != null)
				{
					GameObject particles = Instantiate(item.particlePrefab, point, Quaternion.identity);
					Destroy(particles, 2f);
				}

			}

		}
	}

	GameObject LookupItemObject(PointData pointData)
	{
		int itemID = 0;
		float terrainType;


		//if (pointData.terrainType != 1) Debug.Log(pointData.terrainType);

		if (pointData.terrainType > 1.0f && pointData.terrainType <= 2.001f)
		{

			itemID = 1;

			terrainType = pointData.terrainType - 1.0f;
		}
		else if (pointData.terrainType > 2.0f && pointData.terrainType <= 3.001f)
		{

			itemID = 2;

			terrainType = pointData.terrainType - 2.0f;
		}
		else
		{
			terrainType = pointData.terrainType;
		}

		return itemIDList[itemID];
	}

	public void SpawnItem(Item item, Vector3 position)
    {
		GameObject itemObj = Instantiate(item.prefab, position, Random.rotation);
		spawnedItems.Add(itemObj.GetComponent<Item>());

	}
}
