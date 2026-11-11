using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

[RequireComponent(typeof(Interactable))]
public class Geode : MonoBehaviour
{
    [System.Serializable]
    public struct RarityItemPair
    {
        public int rarityIndex;
        public GameObject[] items;
    }

    Material material;

    Interactable interactible;

    GameManager gm;

    public GameObject sparkleRays;
    public GameObject sparkleExplosion;
    public GameObject chunkParticles;

    [SerializeField]
    public List<RarityItemPair> rarityItemList = new List<RarityItemPair>();
    public List<Color> raritySparkleColors = new List<Color>();
    public List<float> crackTimings = new List<float>();
    public AnimationCurve shinyCrackCurve;
    int numCracks;
    GameObject itemToSpawn;

    float transparencyTimer;
    float roll;
    float sparkleSize;
    float rarityLifetimeOffset;
    Color sparkleColor;

    private void OnEnable()
    {
        gm = FindFirstObjectByType<GameManager>();

        interactible = GetComponent<Interactable>();
        interactible.onInteract += BreakOpen;

        material = transform.GetChild(0).GetComponent<MeshRenderer>().material;

        roll = Random.Range(0f, 1f);

        if (roll < 0.4f)
        {
            itemToSpawn = rarityItemList[0].items[Random.Range(0, rarityItemList[0].items.Length - 1)];
            sparkleSize = 0.15f;
            sparkleColor = raritySparkleColors[0];
            rarityLifetimeOffset = -1.25f;
            numCracks = 1;
        }
        else if (roll < 0.75f)
        {
            itemToSpawn = rarityItemList[1].items[Random.Range(0, rarityItemList[1].items.Length - 1)];
            sparkleSize = 0.35f;
            sparkleColor = raritySparkleColors[1];
            rarityLifetimeOffset = -1f;
            numCracks = 2;
        }
        else if (roll < 0.9f)
        {
            itemToSpawn = rarityItemList[2].items[Random.Range(0, rarityItemList[2].items.Length - 1)];
            sparkleSize = 0.69f;
            sparkleColor = raritySparkleColors[2];
            rarityLifetimeOffset = -0.75f;
            numCracks = 3;
        }
        else if (roll < 0.98f)
        {
            itemToSpawn = rarityItemList[3].items[Random.Range(0, rarityItemList[3].items.Length - 1)];
            sparkleSize = 1.2f;
            sparkleColor = raritySparkleColors[3];
            rarityLifetimeOffset = -0.4f;
            numCracks = 4;
        }
        else if (roll < 1f)
        {
            itemToSpawn = rarityItemList[4].items[Random.Range(0, rarityItemList[4].items.Length - 1)];
            sparkleSize = 1.75f;
            sparkleColor = raritySparkleColors[4];
            rarityLifetimeOffset = 0f;
            numCracks = 5;
        }
    }

    async public void BreakOpen()
    {

        interactible.onInteract -= BreakOpen;

        interactible.enabled = false;

        //transform.GetChild(0).gameObject.SetActive(false);
        

        float timer = 0;

        GameObject rays = Instantiate(sparkleRays, transform.position + Vector3.up * 1, Quaternion.identity);

        List<Vector3> shiniesScales = new List<Vector3>();
        for (int i = 0; i < rays.transform.childCount; i++)
        {
            shiniesScales.Add(rays.transform.GetChild(i).localScale);
            rays.transform.GetChild(i).gameObject.SetActive(false);
        }

        for (int i = 0; i < numCracks; i++)
        {
            timer = 0f;

            List<Transform> shinies = new List<Transform>();

            for (int j = 0; j <= i; j++)
            {
                shinies.Add(rays.transform.GetChild(j));
                shinies[j].GetComponent<MeshRenderer>().material.SetColor("_Color", raritySparkleColors[i] * 0.5f + Color.white * 0.5f);
                shinies[j].gameObject.SetActive(true);
            }
            while (timer < crackTimings[i])
            {
                for (int j = 0; j < shinies.Count; j++)
                {

                    shinies[j].localScale = Vector3.Lerp(Vector3.zero, shiniesScales[j], shinyCrackCurve.Evaluate(Mathf.Clamp(timer / crackTimings[j], 0f, 1f)));
                }
                timer += Time.deltaTime;
                await Task.Yield();
            }
        }
        


        gm.SpawnItem(itemToSpawn.GetComponent<Item>(), transform.position + Vector3.up * 1);

        Transform sparkleTransform = Instantiate(sparkleExplosion, transform.position + Vector3.up * 1, Quaternion.identity).transform;
        Instantiate(chunkParticles, transform.position + Vector3.up * 1, Quaternion.Euler(-90, 0, 0));
        sparkleTransform.GetComponent<SparkleExplosion>().sparkleColor = sparkleColor;
        sparkleTransform.GetComponent<SparkleExplosion>().lifetime += rarityLifetimeOffset;
        sparkleTransform.localScale = transform.localScale * (sparkleSize);
        sparkleTransform.GetComponentInChildren<Light>().intensity = 20 + (roll * roll * roll * roll) * 100;

        sparkleTransform.GetComponentInChildren<FadeLight>().time = sparkleTransform.GetComponent<SparkleExplosion>().lifetime;

        timer = 0;

        gameObject.SetActive(false);

        Vector3 raysMaxScale = rays.transform.localScale; ;

        while (timer < 0.5f)
        {
            rays.transform.localScale = Vector3.Lerp(raysMaxScale,  Vector3.zero, Mathf.SmoothStep(0f, 1f, timer / 0.5f));
            timer += Time.deltaTime;
            await Task.Yield();
        }

        Destroy(rays);
        //FadeTransparency(0.3f);


        Destroy(gameObject);
    }

    async void FadeTransparency(float time)
    {
        transparencyTimer = 0;

        while (transparencyTimer < 0.2f)
        {
            transparencyTimer += Time.deltaTime;
            await Task.Yield();
        }

        transparencyTimer = 0;
        Color baseColor = material.color;


        while (transparencyTimer < time)
        {
            transparencyTimer += Time.deltaTime;
            material.SetColor("_BaseColor", baseColor * new Color(1, 1, 1, Mathf.SmoothStep(1, 0, transparencyTimer / time)));
            await Task.Yield();
        }
    }



}
