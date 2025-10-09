using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	PlayerController pc;
	PlayerInventory pi;

	public List<GameObject> itemIDList = new List<GameObject>();

	public List<GameObject> spawnedItems = new List<GameObject>();

	public static Transform geodeContainer;
	public static List<GameObject> interactables = new List<GameObject>();
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

		
	}

	// Update is called once per frame
	void Update()
	{
		for (int i = 0; i < spawnedItems.Count; i++)
        {
			if (pi.CanStoreItem(spawnedItems[i].GetComponent<Item>()) && (spawnedItems[i].transform.position - pc.transform.position).magnitude < pc.magnetDistance)
            {
				spawnedItems[i].transform.position = Vector3.MoveTowards(spawnedItems[i].transform.position, 
					pc.transform.position, 
					pc.magnetSpeed * Time.deltaTime * (pc.magnetDistance - (spawnedItems[i].transform.position - pc.transform.position).magnitude));

				if ((spawnedItems[i].transform.position - pc.transform.position).magnitude < 1)
                {
					//Debug.Log("Collected");
					pi.CollectItem(spawnedItems[i].GetComponent<Item>());
					spawnedItems[i].SetActive(false);
					Destroy(spawnedItems[i]);
					spawnedItems.RemoveAt(i);
                }
            }
        }

		float closestDistance = Mathf.Infinity;

		if (pc.currentInteractible != null)
        {
			pc.currentInteractible.interactTextCanvas.SetActive(false);
        }

		pc.currentInteractible = null;

		List<int> indicesToRemove = new List<int>();

		for (int i = 0;  i < interactables.Count; i++)
		{
			if (interactables[i] != null)
            {
				float distance = (pc.transform.position - interactables[i].transform.position).magnitude;
				if (distance < 5
					)
				{
					if (distance < closestDistance)
					{
						closestDistance = distance;
						pc.currentInteractible = interactables[i].GetComponent<Interactable>();

						pc.currentInteractible.interactTextCanvas.SetActive(true);
					}
				}
			}
            else
            {
				indicesToRemove.Add(i);
            }
			
        }
		for (int i = indicesToRemove.Count - 1; i > 0; i--)
        {
			interactables.RemoveAt(indicesToRemove[i]);
        }
	}

	public void SpawnItems(Dictionary<Vector3Int, PointData> itemsToSpawn, Vector3 deformCenter, float itemLifeTime,  float explosionForce = 0, float explosionVariation = 0, float explosionRadius = 10)
	{
		foreach (Vector3Int point in itemsToSpawn.Keys)
		{
			if (Random.Range(0, 10) < 3)
            {
				GameObject item = Instantiate(LookupItemObject(itemsToSpawn[point]), point, Quaternion.identity);
				

				Rigidbody rb;

				if (item.TryGetComponent<Rigidbody>(out rb))
				{
					rb.AddExplosionForce(explosionForce, deformCenter, explosionRadius, explosionForce * 0.1f);
				}

				spawnedItems.Add(item);

				
			}
			Item itemItem = LookupItemObject(itemsToSpawn[point]).GetComponent<Item>();
			if (itemItem.particlePrefab != null)
			{
				GameObject particles = Instantiate(itemItem.particlePrefab, point, Quaternion.identity);
				Destroy(particles, 2f);
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
}
