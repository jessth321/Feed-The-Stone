using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Interactable))]
public class Geode : MonoBehaviour
{
    [System.Serializable]
    public struct RarityItemPair
    {
        public int rarityIndex;
        public GameObject[] items;
    }


    Interactable interactible;

    [SerializeField]
    public List<RarityItemPair> rarityItemList = new List<RarityItemPair>();
    GameObject itemToSpawn;

    private void OnEnable()
    {
        interactible = GetComponent<Interactable>();
        interactible.onInteract += BreakOpen;


        float roll = Random.Range(0f, 1f);

        if (roll < 0.5f)
        {
            itemToSpawn = rarityItemList[0].items[Random.Range(0, rarityItemList[0].items.Length - 1)];
        }
        else if (roll < 0.8f)
        {
            itemToSpawn = rarityItemList[1].items[Random.Range(0, rarityItemList[1].items.Length - 1)];
        }
        else if (roll < 0.93f)
        {
            itemToSpawn = rarityItemList[2].items[Random.Range(0, rarityItemList[2].items.Length - 1)];
        }
        else if (roll < 0.98f)
        {
            itemToSpawn = rarityItemList[3].items[Random.Range(0, rarityItemList[3].items.Length - 1)];
        }
        else if (roll < 1f)
        {
            itemToSpawn = rarityItemList[4].items[Random.Range(0, rarityItemList[4].items.Length - 1)];
        }
    }

    public void BreakOpen()
    {
        Instantiate(itemToSpawn, transform.position + Vector3.up * 1, Quaternion.identity);

        Destroy(gameObject);
    }



}
