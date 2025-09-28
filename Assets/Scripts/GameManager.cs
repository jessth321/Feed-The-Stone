using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	PlayerController pc;
	PlayerInventory pi;

	public List<GameObject> itemIDList = new List<GameObject>();

	public List<GameObject> spawnedItems = new List<GameObject>();

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
					//Destroy(spawnedItems[i]);
					spawnedItems[i].SetActive(false);
					spawnedItems.RemoveAt(i);
                }
            }
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
