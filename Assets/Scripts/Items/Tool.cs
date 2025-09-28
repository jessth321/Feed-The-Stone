using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using TMPro;

public class Tool : Item
{
    public ToolType toolType;
    public bool isTwoHanded;
    public float effect;
    public float effectVariation;
    public float effectRoughness;
    public float speed;

    MarchingCubes mc;
    GameManager gm;

    public Animator anim;
    public float typeAnimID;

    Transform player;

    public ParentConstraint parentConstraint;

    Vector3 restPosition;
    Quaternion restRotation;


    [Header("")]
    public Transform deformTransform;
    [Range(0f, 500f)]
    public float deformationRadius;
    [Range(0f, 1f)]
    public float deformationBlend;
    [Range(0f, 1f)]
    public float deformationFrequency;
    [Range(0f, 1f)]
    public float deformationAmp;
    [Range(0, 12)]
    public int deformationOct;
    [Range(0f, 20f)]
    public float deformationLac;
    [Range(0f, 1f)]
    public float deformationPers;



    public bool isSwinging;


    [Header("")]

    public GameObject toSpawn;


    private void Start()
    {
        mc = FindFirstObjectByType<MarchingCubes>();
        gm = FindFirstObjectByType<GameManager>();
        anim = transform.GetComponent<Animator>();
        anim.enabled = true;

        player = transform.parent.parent;

        parentConstraint = transform.parent.GetComponent<ParentConstraint>();
        deformTransform = transform.parent.parent.GetComponent<PlayerController>().deformTransform;

        restPosition = transform.parent.localPosition;
        restRotation = transform.parent.localRotation;

    }

    public void PlayAnimation()
    {
        anim.SetTrigger("UseTool");

        parentConstraint.constraintActive = false;
        transform.parent.localPosition = restPosition + new Vector3(0, -0.2f, -0.6f);
        transform.parent.localRotation = restRotation * Quaternion.Euler(6, 180,0);

        StartCoroutine(DeformAfterTime());

    }

    // needs to be redone with proper async operations
    IEnumerator DeformAfterTime()
    {


        yield return new WaitForSeconds(0.25f);



        Vector3 deformPos = deformTransform.position; ;
        RaycastHit hit;
        if (Physics.Raycast(player.position + (Vector3.up * 2f), (deformPos - (player.position + Vector3.up * 2f)).normalized, out hit, 10f))
        {
            if (hit.transform.CompareTag("Terrain"))
            {
                deformPos = hit.point;
            }
        }

        List<Vector3> points = new List<Vector3>();

        int deformSize = Mathf.CeilToInt(deformationRadius * 2);

        for (int x = 0; x < deformSize; x++)
        {
            for (int y = 0; y < deformSize; y++)
            {
                for (int z = 0; z < deformSize; z++)
                {
                    int halfDeformSize = Mathf.CeilToInt(deformationRadius);
                    points.Add(deformPos + new Vector3(x - halfDeformSize, y - halfDeformSize, z - halfDeformSize));
                }
            }
        }

        Dictionary<Vector3Int, PointData> preDeformPointData = mc.SamplePoints(points);
        //Debug.Log(pointdata[0].terrainType);

        //LogTerrainType(preDeformPointData);


        yield return new WaitForSeconds(0.08f);

        mc.DeformChunks(new Deformation(deformPos, deformationRadius, deformationBlend, deformationFrequency, deformationAmp, (float)deformationOct, deformationLac, deformationPers), 0.1f);


        yield return new WaitForSeconds(0.17f);

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

        gm.SpawnItems(pointDataToSpawn, deformPos, 5000, 500, 5, deformationRadius * 2);


        yield return new WaitForSeconds(0.5f);

        parentConstraint.constraintActive = true;
    }


void LogTerrainType(List<PointData> pointData)
    {
        for (int i =0; i < pointData.Count; i++)
        {
            float oreType = 0f;
            float terrainType;
            //Debug.Log(pointdata[0].terrainType);

            if (pointData[i].terrainType > 1.0f && pointData[i].terrainType <= 2.0f)
            {

                oreType = 1f;

                terrainType = pointData[i].terrainType - 1.0f;
            }
            else if (pointData[i].terrainType > 2.0f && pointData[i].terrainType <= 3.0f)
            {

                oreType = 2f;

                terrainType = pointData[i].terrainType - 2.0f;
            }
            else
            {
                terrainType = pointData[i].terrainType;
            }


            Debug.Log("OreType " + oreType);
            Debug.Log("TerrainType: " + terrainType);
            Debug.Log("Density: " + pointData[i].density);
            //Debug.DrawRay();
        }
    }
}
