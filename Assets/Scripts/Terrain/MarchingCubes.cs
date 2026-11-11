using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Threading;
using System.Threading.Tasks;
//using System.Diagnostics;

public class MarchingCubes : MonoBehaviour
{
	[Header("Chunk Settings")]
	public Material terrainMaterial;
	public int chunkSize;
	public int chunkRenderDist = 10;
	public int initChunkRenderDist = 10;

	[Header("Frequencies")]
	public float noiseFreq = 0.1f;
	[Range(-1f, 1f)]
	public float isoLevel = 0.5f;
	public float noiseFreqGround;
	public float noiseFreqCeiling;
	public float noiseFreqMask;
	public float noiseScale;

	[Header("FBM Settings")]
	public int octaves;//= 4;
	public float lacunarity;//= 1.8715;
	public float persistence;//= 0.65;
	public float amplitude;//= 1.0;

	[Header("Rooms")]
	public Transform RoomsContainer;
	public RoomData[] rooms;

	[Header("Height Settings")]
	public float groundLevel;
	public float groundBlend;
	public float groundVariance;
	public float ceiling;
	public float ceilingBlend;
	public float ceilingVariance;
	public Texture2D heightShapingTexture;
	public int heightEvaluationResolution;

	public bool isCavernous;

	[Header("Height Shaping Settings")]
	public AnimationCurve continentalnessCurve;
	public float continentalnessFrequency;
	public int continentalnessOctaves;
	public float continentalnessLacunarity;
	public float continentalnessPersistence;
	public float continentalnessAmplitude;

	[Header("")]
	public AnimationCurve erosionCurve;
	public float erosionFrequency;
	public int erosionOctaves;
	public float erosionLacunarity;
	public float erosionPersistence;
	public float erosionAmplitude;

	[Header("")]
	public AnimationCurve cliffsCurve;
	public float cliffsFrequency;
	public int cliffsOctaves;
	public float cliffsLacunarity;
	public float cliffsPersistence;
	public float cliffsAmplitude;

	[Header("")]
	public AnimationCurve otherCurve;
	public float otherFrequency;
	public int otherOctaves;
	public float otherLacunarity;
	public float otherPersistence;
	public float otherAmplitude;

	[Header("")]
	public ComputeShader cShader;
	public ComputeShader cShaderNoise;
	public ComputeShader cShaderHNoise;

	ComputeBuffer triangleBuffer;
	ComputeBuffer pointsBuffer;
	ComputeBuffer pointsBuffer2;
	ComputeBuffer triCountBuffer;
	ComputeBuffer sidesBuffer;
	ComputeBuffer heightEvaluationBuffer;
	ComputeBuffer deformationBuffer;
	ComputeBuffer roomBuffer;


	//ComputeBuffer hasHolesBuffer;
	//ComputeBuffer floorBuffer;
	//ComputeBuffer ceilingBuffer;
	//ComputeBuffer minMaxHeightBuffer;

	[Header("")]
	public GameObject chunkContainer;
	public GameObject emptyChunkContainer;

	List<Chunk> chunkPool = new List<Chunk>();
	HashSet<Vector3Int> allChunks = new HashSet<Vector3Int>();
	Queue<Chunk> chunkQueue = new Queue<Chunk>();

	Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
	HashSet<Chunk> borderChunks = new HashSet<Chunk>();
	Dictionary<Vector3Int, Dictionary<float, Chunk>> preComputedChunks = new Dictionary<Vector3Int, Dictionary<float, Chunk>>();
	float[] chunkPoints;
	List<List<int>> chunkSideIndices = new List<List<int>>();

	Dictionary<Vector3Int, List<Deformation>> chunkCoordDeformationDict = new Dictionary<Vector3Int, List<Deformation>>();

	VertexData[] vertexDataArray;


	Vector3 seedOffset;
	Vector2 seedOffsetFloor;
	Vector2 seedOffsetCeiling;
	Vector2 seedOffsetCont;
	Vector2 seedOffsetErosion;
	Vector2 seedOffsetCliffs;

	public Transform player;
	Vector3Int playerChunkPos;

	//public GameObject cloud;
	public GameObject geode;

	Matrix4x4[] goldMatrices;
	Matrix4x4[] ironMatrices;
	private MaterialPropertyBlock goldMaterialPropertyBlock;
	private MaterialPropertyBlock ironMaterialPropertyBlock;

	public Mesh oreMesh;
	public Material goldMaterial;
	public Material ironMaterial;


	bool isUpdatingChunks = false;
	public static bool isInitializingChunks = false;

	
	struct Triangle
	{
#pragma warning disable 649 // disable unassigned variable warning
		public Vector3 a;
		public Vector3 b;
		public Vector3 c;

		public Vector3 this[int i]
		{
			get
			{
				switch (i)
				{
					case 0:
						return a;
					case 1:
						return b;
					default:
						return c;
				}
			}
		}
	}


    private void Awake()
    {

		isInitializingChunks = true;
	}

    // Start is called before the first frame update
    void Start()
	{

		rooms = new RoomData[RoomsContainer.childCount];



		goldMaterialPropertyBlock = new MaterialPropertyBlock();
		ironMaterialPropertyBlock = new MaterialPropertyBlock();


		for (int i = 0; i < RoomsContainer.childCount; i++)
        {
			Room room;
			if (RoomsContainer.GetChild(i).TryGetComponent<Room>(out room))
            {
				rooms[i] = room.roomData;
				//Debug.Log(room.roomData.position);
            }
        }




		Vector4[] evalList = new Vector4[heightEvaluationResolution];
		
		for (int i = 0; i < heightEvaluationResolution; i++)
        {
			evalList[i] = new Vector4(
				continentalnessCurve.Evaluate((float)i / heightEvaluationResolution),
				erosionCurve.Evaluate((float)i / heightEvaluationResolution),
				cliffsCurve.Evaluate((float)i / heightEvaluationResolution),
				otherCurve.Evaluate((float)i / heightEvaluationResolution)
				);
        }
		//Debug.Log(continentalnessCurve.Evaluate(heightEvaluationResolution - 1));

		heightEvaluationBuffer = new ComputeBuffer(heightEvaluationResolution, sizeof(float) * 4);
		heightEvaluationBuffer.SetData(evalList);

		roomBuffer = new ComputeBuffer(rooms.Length, System.Runtime.InteropServices.Marshal.SizeOf(typeof(RoomData)));
		roomBuffer.SetData(rooms);


		playerChunkPos = new Vector3Int(((int)(player.position.x + chunkSize / 2) / chunkSize), ((int)(player.position.y + chunkSize / 2) / chunkSize), ((int)(player.position.z + chunkSize / 2) / chunkSize));
		InitializeChunks();
		//ChunkTest();


		isInitializingChunks = false;
	}

    void Update()
    {
		Vector3 playerChunkPosition = new Vector3(((player.position.x + chunkSize / 2) / chunkSize), ((player.position.y + chunkSize / 2) / chunkSize), ((player.position.z + chunkSize / 2) / chunkSize));
		
		
		if (!isUpdatingChunks)
        {
			//if (Mathf.Abs(playerChunkPos.x - playerChunkPosition.x) > 1.25f)
			//{
			//	playerChunkPos = new Vector3Int(((int)(player.position.x + chunkSize / 2) / chunkSize), playerChunkPos.y, playerChunkPos.z);
			//
			//
			//	UpdateChunks();
			//}
			//else if (Mathf.Abs(playerChunkPos.y - playerChunkPosition.y) > 1.25f)
			//{
			//	playerChunkPos = new Vector3Int(playerChunkPos.x, ((int)(player.position.y + chunkSize / 2) / chunkSize), playerChunkPos.z);
			//
			//
			//	UpdateChunks();
			//}
			//else if (Mathf.Abs(playerChunkPos.z - playerChunkPosition.z) > 1.25f)
			//{
			//	playerChunkPos = new Vector3Int(playerChunkPos.x, playerChunkPos.y, ((int)(player.position.z + chunkSize / 2) / chunkSize));
			//
			//
			//	UpdateChunks();
			//}

			if (Mathf.Abs(playerChunkPos.x - playerChunkPosition.x) > 1.25f || Mathf.Abs(playerChunkPos.y - playerChunkPosition.y) > 1.25f || Mathf.Abs(playerChunkPos.z - playerChunkPosition.z) > 1.25f)
			{
				playerChunkPos = new Vector3Int(((int)(player.position.x + chunkSize / 2) / chunkSize), (int)(player.position.y + chunkSize / 2) / chunkSize, (int)(player.position.z + chunkSize / 2) / chunkSize);


				UpdateChunks();
			}
		}



		//foreach ( Chunk chunk in chunks.Values)
		//{
		//	if ((Vector3.ProjectOnPlane(chunk.coord - playerChunkPos, Vector3.up)).magnitude < chunkRenderDist * 0.5f)
		//    {
		//		chunk.meshCollider.enabled = true;
		//    }
		//    else
		//    {
		//		chunk.meshCollider.enabled = false;
		//	}
		//}

		RenderOres();
	}

	int IndexFromCoord(int x, int y, int z)
    {
		return x + y * chunkSize + z * chunkSize * chunkSize;
	}

	void RenderOres()
	{

		if (oreMesh != null && goldMaterial != null && goldMatrices != null && goldMatrices.Length > 0)
		{
			Graphics.RenderMeshInstanced(
			   new RenderParams(goldMaterial)
			   {
				   matProps = goldMaterialPropertyBlock,
				   shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
				   receiveShadows = true
			   },
			   oreMesh,
			   0,
			   goldMatrices,
			   goldMatrices.Length
		   );
		}

		if (oreMesh != null && ironMaterial != null && ironMatrices != null && ironMatrices.Length > 0)
		{
			Graphics.RenderMeshInstanced(
			   new RenderParams(ironMaterial)
			   {
				   matProps = ironMaterialPropertyBlock,
				   shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
				   receiveShadows = true
			   },
			   oreMesh,
			   0,
			   ironMatrices,
			   ironMatrices.Length
		   );
		}


	}

	void UpdateOreMatrices()
    {
		List<Matrix4x4> goldMatricesList = new List<Matrix4x4>();
		List<Matrix4x4> ironMatricesList = new List<Matrix4x4>();


		int goldIndex = 0;
		int ironIndex = 0;

		foreach (Chunk chunk in chunks.Values)
		{
			//if (chunk.goldMatrices)


			if (chunk.goldMatrices != null && chunk.goldMatrices.Length > 0) 
			{ 
				goldMatricesList.AddRange(chunk.goldMatrices);
				goldIndex += chunk.goldMatrices.Length;
			}
			if (chunk.ironMatrices != null && chunk.ironMatrices.Length > 0) 
			{
				ironMatricesList.AddRange(chunk.ironMatrices);
				ironIndex += chunk.ironMatrices.Length;
			}

		}

		goldMatrices = goldMatricesList.ToArray();
		ironMatrices = ironMatricesList.ToArray();
	}


	void InitializeChunks()
	{
		chunkPoints = new float[chunkSize * chunkSize * chunkSize];


		// Make chunk pool
		//for (int z = -initChunkRenderDist + playerChunkPos.z; z < initChunkRenderDist + playerChunkPos.z; z++)
		//{
		//	for (int x = -initChunkRenderDist + playerChunkPos.x; x < initChunkRenderDist + playerChunkPos.x; x++)
		//	{
		//		for (int y = -initChunkRenderDist + playerChunkPos.y; y < initChunkRenderDist + playerChunkPos.y; y++)
		//		{
		//			GameObject chunkObj = new GameObject("");
		//			chunkObj.transform.SetParent(chunkContainer.transform);
		//			Chunk chunk = chunkObj.AddComponent<Chunk>();
		//			chunkObj.SetActive(false);
		//			//chunk.cloud = cloud;
		//			//chunk.rock = rock;
		//			chunk.SetUp(terrainMaterial);
		//
		//			chunkPool.Add(chunk);
		//		}
		//	}
		//}

		seedOffset = new Vector3(Random.Range(0, 99999), Random.Range(0, 99999), Random.Range(0, 99999));
		seedOffsetFloor = new Vector2(Random.Range(0, 99999), Random.Range(0, 99999));
		seedOffsetCeiling = new Vector2(Random.Range(0, 99999), Random.Range(0, 99999));
		seedOffsetCont = new Vector2(Random.Range(0, 99999), Random.Range(0, 99999));
		seedOffsetErosion = new Vector2(Random.Range(0, 99999), Random.Range(0, 99999));
		seedOffsetCliffs = new Vector2(Random.Range(0, 99999), Random.Range(0, 99999));


		// Make first chunk

		Vector3Int coord000 = new Vector3Int(0, 0, 0);
		chunks.Add(coord000, CreateChunk(coord000));

		Queue<Chunk> chunkQueue = new Queue<Chunk>();

		chunkQueue.Enqueue(chunks[coord000]);

		bool hasStartedChain = false;

		while (chunkQueue.Count > 0)
		{
			if (chunkCoordDeformationDict.ContainsKey(chunkQueue.Peek().coord))
			{
				chunkQueue.Peek().deformations = chunkCoordDeformationDict[chunkQueue.Peek().coord];
			}

			uint[] shouldGenerateToSidesUint = new uint[12];
			sidesBuffer = new ComputeBuffer(12, sizeof(uint));
			sidesBuffer.SetData(shouldGenerateToSidesUint);

			GenerateChunkDensity(chunkQueue.Peek());
			GenerateChunk(chunkQueue.Peek());

			bool[] shouldGenerateToSides = new bool[12];
			sidesBuffer.GetData(shouldGenerateToSidesUint);

			sidesBuffer.Release();
			for (int i = 0; i < 12; i++)
			{
				if (shouldGenerateToSidesUint[i] == 0) shouldGenerateToSides[i] = false;
				if (shouldGenerateToSidesUint[i] == 1) shouldGenerateToSides[i] = true;
			}

			chunkQueue.Peek().shouldGenerateToSides = shouldGenerateToSides;

			Vector3Int qCoord = chunkQueue.Peek().coord;
			Vector3Int[] newCoords = { 
				qCoord + Vector3Int.right,
				qCoord + Vector3Int.left,
				qCoord + Vector3Int.up,
				qCoord + Vector3Int.down,
				qCoord + Vector3Int.forward,
				qCoord + Vector3Int.back, 
			};

			for (int i = 0; i < 6; i++)
            {
				if (shouldGenerateToSides[i] && shouldGenerateToSides[i + 6] &&
				!chunks.ContainsKey(newCoords[i]))
				{
					if ((newCoords[i] - playerChunkPos).magnitude < initChunkRenderDist)
					{
						Chunk newChunk = CreateChunk(newCoords[i]);
						chunkQueue.Enqueue(newChunk);
						chunks.Add(newCoords[i], newChunk);
					}
					else if (!borderChunks.Contains(chunkQueue.Peek()))
					{
						borderChunks.Add(chunkQueue.Peek());
					}

					hasStartedChain = true;
				}
			}

			//if (!borderChunks.Contains(chunkQueue.Peek()) && (chunkQueue.Peek().coord - playerChunkPos) )
			//{
			//	borderChunks.Add(chunkQueue.Peek());
			//}

			if (!hasStartedChain)
			{
				Vector3Int newCoord = qCoord + Vector3Int.up;
				Chunk newChunk = CreateChunk(newCoord);
				chunkQueue.Enqueue(newChunk);
				chunks.Add(newCoord, newChunk);

				player.position = Vector3.up * chunkSize * newCoord.y + Vector3.up * chunkSize * 8;
			}

			chunkQueue.Dequeue();
		}


		HashSet<Vector3Int> chunkCoordsToRemove = new HashSet<Vector3Int>();

		foreach (Vector3Int coord in chunks.Keys)
		{
			if ((coord - playerChunkPos).magnitude > chunkRenderDist)
			{
				chunkCoordsToRemove.Add(coord);
			}
		}
		foreach (Vector3Int coord in chunkCoordsToRemove)
		{

			EmptyChunk(coord);
			//chunks.Remove(coord);
		}

		//for (int i = 0; i < allChunks.Count; i++)
		//{
		//	if (chunks.ContainsKey(allChunks[i]))
		//    {
		//		if (allChunks[i])
		//        {
		//
		//        }
		//    }
		//}

		UpdateOreMatrices();
	}

	Chunk CreateChunk(Vector3Int coord)
    {
		if (!allChunks.Contains(coord))
		{
			allChunks.Add(coord);
		}


		GameObject chunkObj = new GameObject("");
		chunkObj.transform.SetParent(chunkContainer.transform);
		Chunk chunk = chunkObj.AddComponent<Chunk>();
		//chunkObj.SetActive(false);
		chunk.SetUp(terrainMaterial, oreMesh, goldMaterial, ironMaterial);
		chunk.geode = geode;
		//Chunk chunk = chunkPool[0];

		chunk.gameObject.SetActive(true);

		//chunkPool.RemoveAt(0);
		chunk.gameObject.name = coord.x.ToString() + ", " + coord.y.ToString() + ", " + coord.z.ToString();
		//chunkObj.transform.parent = chunkContainer.transform;

		//chunk.gameObject.GetComponent<MeshFilter>().mesh = new Mesh();

		//chunk.gameObject.GetComponent<MeshFilter>().mesh.Clear();
		//chunk.gameObject.GetComponent<MeshFilter>().mesh.vertices = new Vector3[0];
		//chunk.gameObject.GetComponent<MeshFilter>().mesh.triangles = new int[0];
		chunk.mesh = chunk.gameObject.GetComponent<MeshFilter>().mesh;

		chunk.coord = coord;
		chunk.deformations = new List<Deformation>();
		chunk.deformations.Add(new Deformation(Vector3.zero, 0, 0, 0, 0, 0, 0, 0));
		return chunk;
	}

	void EmptyChunk(Vector3Int coord)
    {
		if (chunks[coord] != null)
        {
			Chunk chunk = chunks[coord];

			//chunk.coord = new Vector3Int(0, 0, 0);

			//chunkObj.transform.parent = emptyChunkContainer.transform;
			//chunkPool.Add(chunk);
			//chunk.ClearChunk();
			//chunk.gameObject.SetActive(false);
			Destroy(chunk.gameObject);
			chunks.Remove(coord);
		}
	}

	async void UpdateChunks()
    {
		isUpdatingChunks = true;
		//HashSet<Chunk> newBorderChunks = new HashSet<Chunk>();

		HashSet<Vector3Int> chunkCoordsToRemove = new HashSet<Vector3Int>();
		
		foreach (Vector3Int coord in chunks.Keys)
		{
			if ((coord - playerChunkPos).magnitude > chunkRenderDist)
			{
				chunkCoordsToRemove.Add(coord);
			}
		}
		foreach (Vector3Int coord in chunkCoordsToRemove)
		{
			//Vector3Int[] borderingCoords = {
			//	coord + Vector3Int.up,
			//	coord + Vector3Int.down,
			//	coord + Vector3Int.right,
			//	coord + Vector3Int.left,
			//	coord + Vector3Int.forward,
			//	coord + Vector3Int.back,
			//	};

			//foreach (Vector3Int borderingCoord in borderingCoords)
            //{
			//	if (chunks.ContainsKey(borderingCoord) && !chunkCoordsToRemove.Contains(borderingCoord))
            //    {
			//		newBorderChunks.Add(chunks[borderingCoord]);
            //    }
            //}

			EmptyChunk(coord);
			//chunks.Remove(coord);
		}
		await Task.Yield();


		//Vector3Int tempPlayerChunkPos = playerChunkPos;



		//foreach ( Vector3Int chunkCoord in allChunks)
        //{
		//	if (!Application.isPlaying) return;
		//
		//
		//	if (!chunks.ContainsKey(chunkCoord) && (chunkCoord - playerChunkPos).magnitude < chunkRenderDist)
        //    {
		//		Chunk newChunk = CreateChunk(chunkCoord);
		//		chunkQueue.Enqueue(newChunk);
		//		chunks.Add(chunkCoord, newChunk);
		//	}
		//}

		for (int z = -chunkRenderDist + playerChunkPos.z; z < chunkRenderDist + playerChunkPos.z; z++)
		{
			for (int x = -chunkRenderDist + playerChunkPos.x; x < chunkRenderDist + playerChunkPos.x; x++)
			{
				for (int y = -chunkRenderDist + playerChunkPos.y; y < chunkRenderDist + playerChunkPos.y; y++)
				{
					Vector3Int chunkCoord = new Vector3Int(x, y, z);
					if (allChunks.Contains(chunkCoord) && !chunks.ContainsKey(chunkCoord) && (chunkCoord - playerChunkPos).magnitude < chunkRenderDist)
					{
						Chunk newChunk = CreateChunk(chunkCoord);
						chunkQueue.Enqueue(newChunk);
						chunks.Add(chunkCoord, newChunk);
					}
				}
			}
		}

		while (chunkQueue.Count > 0)
		{

			if (!Application.isPlaying) return;

			if (chunkQueue.Peek() != null)
            {
				if (chunkCoordDeformationDict.ContainsKey(chunkQueue.Peek().coord))
				{
					chunkQueue.Peek().deformations = chunkCoordDeformationDict[chunkQueue.Peek().coord];
				}


				uint[] shouldGenerateToSidesUint = new uint[12];
				sidesBuffer = new ComputeBuffer(12, sizeof(uint));
				sidesBuffer.SetData(shouldGenerateToSidesUint);

				GenerateChunkDensity(chunkQueue.Peek());
				GenerateChunk(chunkQueue.Peek());


				sidesBuffer.Release();

				chunkQueue.Dequeue();
			}
			else
            {
				Debug.Log("oh no");

				chunkQueue.Dequeue();
			}

			

			await Task.Yield();
		}



		UpdateOreMatrices();

		//borderChunks = newBorderChunks;

		isUpdatingChunks = false;
	}

	bool ShouldBeBorderChunk(Chunk chunk, Vector3Int playerChunkPosition, HashSet<Chunk> borderChunkSet)
    {
		return (chunk.coord - playerChunkPosition).magnitude > chunkRenderDist - chunkSize * 1.6f && !borderChunkSet.Contains(chunk);

	}

	void GenerateChunkDensity(Chunk chunk)
    {
		Vector3 chunkOffset = chunk.coord * (chunkSize - 1);

		chunk.transform.position = new Vector3(chunkOffset.x, chunkOffset.y, chunkOffset.z);

		// Populates point list with positions xyz and values w

		//for (int i = 0; i < chunkSize * chunkSize * chunkSize; i++)
		//{
		//	Vector3 voxelPoint = new Vector3(i % chunkSize, i / (chunkSize * chunkSize) % chunkSize, i / chunkSize % chunkSize);
		//
		//	chunkPoints[i] = new Vector4(voxelPoint.x, voxelPoint.y, voxelPoint.z, 0);
		//}

		chunkPoints = new float[chunkSize * chunkSize * chunkSize];


		//await Task.Yield();

		//chunk.SetUp(terrainMaterial);

		pointsBuffer = new ComputeBuffer(chunkSize * chunkSize * chunkSize, System.Runtime.InteropServices.Marshal.SizeOf(typeof(PointData)));

		if (chunk.deformations.Count > 0)
		{
			deformationBuffer = new ComputeBuffer(chunk.deformations.Count, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Deformation)));
			deformationBuffer.SetData(chunk.deformations);
		}
		//hasHolesBuffer = new ComputeBuffer(2, sizeof(int));

		//Vector4[] array = new Vector4[chunkSize * chunkSize * chunkSize];



		//--------------------------------------------------------------------------------------------------------

		//pointsBuffer.SetData(chunkPoints);
		pointsBuffer = GenerateNoiseValues(pointsBuffer, chunkOffset);

		//pointsBuffer.GetData(chunkPoints);

		//foreach (Vector4 point in chunkPoints)
		//{
		//	Debug.Log(point.w);
		//}
	}

	void GenerateChunk(Chunk chunk)
    {

		int numThreadsPerAxis = Mathf.CeilToInt((chunkSize - 1) / (float)8);
		int numVoxels = (chunkSize - 1) * (chunkSize - 1) * (chunkSize - 1);
		int maxTriangleCount = numVoxels * 5;
		int maxVertexCount = maxTriangleCount * 3;

		triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3 + sizeof(float) * 3, ComputeBufferType.Append);
		triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
		

		triangleBuffer.SetCounterValue(0);


		cShader.SetBuffer(0, "points", pointsBuffer);
		cShader.SetBuffer(0, "triangles", triangleBuffer);
		cShader.SetInt("chunkSize", chunkSize);
		cShader.SetFloat("isoLevel", isoLevel);


		cShader.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

		ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
		int[] triCountArray = { 0 };
		triCountBuffer.GetData(triCountArray);
		int numTris = triCountArray[0];

		// Get triangle data from shader
		vertexDataArray = new VertexData[numTris * 3];
		triangleBuffer.GetData(vertexDataArray, 0, 0, numTris * 3);


		ReleaseBuffers();

		chunk.CreateMesh(vertexDataArray, numTris * 3, false);


	}

	public ComputeBuffer GenerateNoiseValues(ComputeBuffer cPointBuffer, Vector3 chunkOffset)
    {

		int numThreadsPerAxis = Mathf.CeilToInt((chunkSize) / (float)8);

		cShaderNoise.SetInt("chunkSize", chunkSize);
		cShaderNoise.SetVector("seedOffset", seedOffset);
		cShaderNoise.SetVector("seedOffsetGround", seedOffsetFloor);
		cShaderNoise.SetVector("seedOffsetCeiling", seedOffsetCeiling);
		cShaderNoise.SetVector("seedOffsetCont", seedOffsetCont);
		cShaderNoise.SetVector("seedOffsetErosion", seedOffsetErosion);
		cShaderNoise.SetVector("seedOffsetCliffs", seedOffsetCliffs);


		cShaderNoise.SetVector("chunkOffset", chunkOffset);
		cShaderNoise.SetFloat("noiseFreq", noiseFreq * noiseScale);
		cShaderNoise.SetFloat("noiseFreqGround", noiseFreqGround * noiseScale);
		cShaderNoise.SetFloat("noiseFreqCeiling", noiseFreqCeiling * noiseScale);
		cShaderNoise.SetFloat("noiseFreqMask", noiseFreqMask * noiseScale);


		//cShaderNoise.SetFloat("contFreq", continentalnessFrequency * noiseScale);
		//cShaderNoise.SetInt("contOct", continentalnessOctaves);
		//cShaderNoise.SetFloat("contLac", continentalnessLacunarity);
		//cShaderNoise.SetFloat("contPers", continentalnessPersistence);
		//cShaderNoise.SetFloat("contAmp", continentalnessAmplitude);
		//
		//cShaderNoise.SetFloat("erosionFreq", erosionFrequency * noiseScale);
		//cShaderNoise.SetInt("erosionOct", erosionOctaves);
		//cShaderNoise.SetFloat("erosionLac", erosionLacunarity);
		//cShaderNoise.SetFloat("erosionPers", erosionPersistence);
		//cShaderNoise.SetFloat("erosionAmp", erosionAmplitude);
		//
		//cShaderNoise.SetFloat("cliffsFreq", cliffsFrequency * noiseScale);
		//cShaderNoise.SetInt("cliffsOct", cliffsOctaves);
		//cShaderNoise.SetFloat("cliffsLac", cliffsLacunarity);
		//cShaderNoise.SetFloat("cliffsPers", cliffsPersistence);
		//cShaderNoise.SetFloat("cliffsAmp", cliffsAmplitude);


		cShaderNoise.SetFloat("isoLevel", isoLevel);

		cShaderNoise.SetInt("octaves", octaves);
		cShaderNoise.SetFloat("lacunarity", lacunarity);
		cShaderNoise.SetFloat("persistence", persistence);
		cShaderNoise.SetFloat("amplitude", amplitude);

		cShaderNoise.SetFloat("groundLevel", groundLevel / noiseScale);
		cShaderNoise.SetFloat("groundBlend", groundBlend / noiseScale);
		cShaderNoise.SetFloat("groundVariance", groundVariance / noiseScale);
		cShaderNoise.SetFloat("ceiling", ceiling / noiseScale);
		cShaderNoise.SetFloat("ceilingBlend", ceilingBlend / noiseScale);
		cShaderNoise.SetFloat("ceilingVariance", ceilingVariance / noiseScale);
		cShaderNoise.SetBool("isCavernous", isCavernous);

		cShaderNoise.SetFloat("evaluationLength", heightEvaluationResolution);

		cShaderNoise.SetBuffer(0, "points", cPointBuffer);
		cShaderNoise.SetBuffer(0, "sides", sidesBuffer);
		cShaderNoise.SetBuffer(0, "heightEvaluation", heightEvaluationBuffer);
		cShaderNoise.SetBuffer(0, "deformations", deformationBuffer);
		cShaderNoise.SetBuffer(0, "rooms", roomBuffer);



		cShaderNoise.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

		return cPointBuffer;
	}

	public async void DeformChunks(Deformation deformation, float timeOffsetToApply = 0)
    {

		float time = Time.realtimeSinceStartup;

		Vector3Int chunkCoord = new Vector3Int( (int)Mathf.Floor(deformation.position.x / (chunkSize - 1)), (int)Mathf.Floor(deformation.position.y / (chunkSize - 1)), (int)Mathf.Floor(deformation.position.z / (chunkSize - 1)));


		List<Vector3Int> intersectingCoords = new List<Vector3Int>();
		//intersectingCoords.Add(chunkCoord);

		Vector3 chunkCenterPos = (Vector3)chunkCoord * (chunkSize - 1) + Vector3.one * (chunkSize - 1) * 0.5f;
		List<Vector3> chunkCenters = new List<Vector3>();


		Vector3 sphereCenter = new Vector3(deformation.position.x, deformation.position.y, deformation.position.z);
		float sphereRadius = deformation.radius;
		Vector3 cubeHalfExtents = Vector3.one * (chunkSize - 1) * 0.5f;

		int maxChunkReach = Mathf.CeilToInt(sphereRadius / (chunkSize - 1));

		for (int x = - maxChunkReach; x <= maxChunkReach; x++)
        {
			for (int y = -maxChunkReach; y <= maxChunkReach; y++)
			{
				for (int z = -maxChunkReach; z <= maxChunkReach; z++)
				{
					chunkCenters.Add(new Vector3(chunkCenterPos.x + (chunkSize - 1) * x, chunkCenterPos.y + (chunkSize - 1) * y, chunkCenterPos.z + (chunkSize - 1) * z));
				}
			}
		}
		
		foreach ( Vector3 chunkCenter in chunkCenters)
        {
		
			Vector3 closestPointOnCube = new Vector3(
					Mathf.Clamp(sphereCenter.x, chunkCenter.x - cubeHalfExtents.x, chunkCenter.x + cubeHalfExtents.x),
					Mathf.Clamp(sphereCenter.y, chunkCenter.y - cubeHalfExtents.y, chunkCenter.y + cubeHalfExtents.y),
					Mathf.Clamp(sphereCenter.z, chunkCenter.z - cubeHalfExtents.z, chunkCenter.z + cubeHalfExtents.z)
				);
		
			float distToChunk = Vector3.Distance(sphereCenter, closestPointOnCube);

		
			if (distToChunk <= sphereRadius)
            {
				intersectingCoords.Add(new Vector3Int(Mathf.FloorToInt(chunkCenter.x / (chunkSize - 1)), Mathf.FloorToInt(chunkCenter.y / (chunkSize - 1)), Mathf.FloorToInt(chunkCenter.z / (chunkSize - 1))));
				//Debug.DrawLine(sphereCenter, closestPointOnCube, Color.green, 10000);
			}
        }

		//List<Chunk> intersectingChunks = new List<Chunk>();




		float timeToApply = Time.realtimeSinceStartup + timeOffsetToApply;

		foreach (Vector3Int coord in intersectingCoords)
        {
			if (!allChunks.Contains(coord))
            {
				allChunks.Add(coord);
            }

			if (chunks.ContainsKey(coord) && !preComputedChunks.ContainsKey(coord))
            {
				Chunk intersectingChunk = CreateChunk(coord);
				//intersectingChunks.Add(intersectingChunk);
				intersectingChunk.isRegenerated = true;

				intersectingChunk.deformations = new List<Deformation>(chunks[coord].deformations);
				intersectingChunk.deformations.Add(deformation);

				//intersectingChunk.interacableIndices = new List<int>(chunks[coord].interacableIndices);



				uint[] shouldGenerateToSidesUint = new uint[12];
				sidesBuffer = new ComputeBuffer(12, sizeof(uint));
				sidesBuffer.SetData(shouldGenerateToSidesUint);

				GenerateChunkDensity(intersectingChunk);
				GenerateChunk(intersectingChunk);

				//sidesBuffer.GetData(shouldGenerateToSidesUint);

				sidesBuffer.Release();

				//bool[] shouldGenerateToSides = new bool[12];
				//
				//for (int i = 0; i < 12; i++)
				//{
				//	if (shouldGenerateToSidesUint[i] == 0) shouldGenerateToSides[i] = false;
				//	if (shouldGenerateToSidesUint[i] == 1) shouldGenerateToSides[i] = true;
				//}
				//intersectingChunk.shouldGenerateToSides = shouldGenerateToSides;

				intersectingChunk.gameObject.SetActive(false);


				preComputedChunks.Add(coord, new Dictionary<float, Chunk>());
				preComputedChunks[coord].Add(timeToApply, intersectingChunk);
			}
			else if (chunks.ContainsKey(coord) && preComputedChunks.ContainsKey(coord))
            {
				Chunk intersectingChunk = CreateChunk(coord);
				//intersectingChunks.Add(intersectingChunk);

				List<float> timesOfApplyToRegen = new List<float>();

				float closestTimeOfApply = 0;
                foreach (float timeOfApply in preComputedChunks[coord].Keys)
                {
                    if (timeOfApply < timeToApply && timeOfApply > closestTimeOfApply)
                    {
                        closestTimeOfApply = timeOfApply;
                    }
					else if (timeOfApply > timeToApply)
                    {
						preComputedChunks[coord][timeOfApply].deformations.Add(deformation);
						timesOfApplyToRegen.Add(timeOfApply);
                    }
                }

				foreach(float timeOfApply in timesOfApplyToRegen)
                {
					if (preComputedChunks[coord].ContainsKey(timeOfApply))
					{
						RegeneratePreComputedChunk(coord, timeOfApply);
					}
					await Task.Yield();
				}

                if (closestTimeOfApply != 0 && preComputedChunks[coord].ContainsKey(closestTimeOfApply))
                {
					intersectingChunk.deformations = new List<Deformation>(preComputedChunks[coord][closestTimeOfApply].deformations);
                }
                else
                {
					intersectingChunk.deformations = new List<Deformation>(chunks[coord].deformations);
                }

				intersectingChunk.deformations.Add(deformation);

				uint[] shouldGenerateToSidesUint = new uint[12];
				sidesBuffer = new ComputeBuffer(12, sizeof(uint));
				sidesBuffer.SetData(shouldGenerateToSidesUint);


				GenerateChunkDensity(intersectingChunk);
				GenerateChunk(intersectingChunk);

				//sidesBuffer.GetData(shouldGenerateToSidesUint);

				sidesBuffer.Release();


				//bool[] shouldGenerateToSides = new bool[12];
				//
				//for (int i = 0; i < 12; i++)
				//{
				//	if (shouldGenerateToSidesUint[i] == 0) shouldGenerateToSides[i] = false;
				//	if (shouldGenerateToSidesUint[i] == 1) shouldGenerateToSides[i] = true;
				//}
				//intersectingChunk.shouldGenerateToSides = shouldGenerateToSides;

				intersectingChunk.gameObject.SetActive(false);

				if (preComputedChunks.ContainsKey(coord))
				{
					preComputedChunks[coord].Add(timeToApply, intersectingChunk);
				}
			}
            else if (!chunks.ContainsKey(coord) && !preComputedChunks.ContainsKey(coord))
            {
				//chunks.Add(coord, CreateChunk(coord));
				Chunk intersectingChunk = CreateChunk(coord);

				intersectingChunk.deformations.Add(deformation);

				uint[] shouldGenerateToSidesUint = new uint[12];
				sidesBuffer = new ComputeBuffer(12, sizeof(uint));
				sidesBuffer.SetData(shouldGenerateToSidesUint);

				GenerateChunkDensity(intersectingChunk);
				GenerateChunk(intersectingChunk);

				//sidesBuffer.GetData(shouldGenerateToSidesUint); 

				sidesBuffer.Release();

				//bool[] shouldGenerateToSides = new bool[12];
				//
				//for (int i = 0; i < 12; i++)
				//{
				//	if (shouldGenerateToSidesUint[i] == 0) shouldGenerateToSides[i] = false;
				//	if (shouldGenerateToSidesUint[i] == 1) shouldGenerateToSides[i] = true;
				//}
				//intersectingChunk.shouldGenerateToSides = shouldGenerateToSides;

				intersectingChunk.gameObject.SetActive(false);

				preComputedChunks.Add(coord, new Dictionary<float, Chunk>());
				preComputedChunks[coord].Add(timeToApply, intersectingChunk);
			}
			else if (!chunks.ContainsKey(coord) && preComputedChunks.ContainsKey(coord))
            {
				Chunk intersectingChunk = CreateChunk(coord);
				List<float> timesOfApplyToRegen = new List<float>();

				float closestTimeOfApply = 0;
				foreach (float timeOfApply in preComputedChunks[coord].Keys)
				{
					if (timeOfApply < timeToApply && timeOfApply > closestTimeOfApply)
					{
						closestTimeOfApply = timeOfApply;
					}
					else if (timeOfApply > timeToApply)
					{
						preComputedChunks[coord][timeOfApply].deformations.Add(deformation);
						timesOfApplyToRegen.Add(timeOfApply);
					}
				}

				foreach (float timeOfApply in timesOfApplyToRegen)
				{
					if (preComputedChunks[coord].ContainsKey(timeOfApply))
					{
						RegeneratePreComputedChunk(coord, timeOfApply);
					}
					await Task.Yield();
				}

				if (closestTimeOfApply != 0 && preComputedChunks[coord].ContainsKey(closestTimeOfApply))
				{
					intersectingChunk.deformations = new List<Deformation>(preComputedChunks[coord][closestTimeOfApply].deformations);
				}

				intersectingChunk.deformations.Add(deformation);

				uint[] shouldGenerateToSidesUint = new uint[12];
				sidesBuffer = new ComputeBuffer(12, sizeof(uint));
				sidesBuffer.SetData(shouldGenerateToSidesUint);

				GenerateChunkDensity(intersectingChunk);
				GenerateChunk(intersectingChunk);

				//sidesBuffer.GetData(shouldGenerateToSidesUint);

				sidesBuffer.Release();

				//bool[] shouldGenerateToSides = new bool[12];
				//
				//for (int i = 0; i < 12; i++)
				//{
				//	if (shouldGenerateToSidesUint[i] == 0) shouldGenerateToSides[i] = false;
				//	if (shouldGenerateToSidesUint[i] == 1) shouldGenerateToSides[i] = true;
				//}
				//intersectingChunk.shouldGenerateToSides = shouldGenerateToSides;

				intersectingChunk.gameObject.SetActive(false);

				if (preComputedChunks[coord] != null)
				{
					preComputedChunks[coord].Add(timeToApply, intersectingChunk);
				}
			}
			await Task.Yield();
        }

		while (Time.realtimeSinceStartup < timeToApply)
        {
			await Task.Yield();
        }
		

		foreach(Vector3Int coord in intersectingCoords)
        {
			if (preComputedChunks.ContainsKey(coord) && preComputedChunks[coord].ContainsKey(timeToApply))
            {
				if (chunks.ContainsKey(coord))
                {
					//bool isQueued = false;
					//if(chunkQueue.Contains(chunks[coord]))
                    //{
					//	//Debug.Log("aight");
					//	//isQueued = true;
                    //}
					EmptyChunk(coord);
					//chunks.Remove(coord);
                }
				chunks.Add(coord, preComputedChunks[coord][timeToApply]);
				chunks[coord].gameObject.SetActive(true);

				//if (isQueued)
                //{
				//
                //}
				//Debug.Log(chunks[coord].deformations.Count);

				preComputedChunks[coord].Remove(timeToApply);
				if (preComputedChunks[coord].Count == 0)
                {
					preComputedChunks.Remove(coord);
                }


				if (!chunkCoordDeformationDict.ContainsKey(coord))
				{
					chunkCoordDeformationDict.Add(coord, chunks[coord].deformations);
				}
				else
				{
					chunkCoordDeformationDict[coord] = chunks[coord].deformations;
				}

				//if (ShouldBeBorderChunk(chunks[coord], playerChunkPos, borderChunks))
                //{
				//	borderChunks.Add(chunks[coord]);
                //}
			}

			//Vector3Int coord = intersectingChunk.coord;
			//if (chunks.ContainsKey(coord))
			//{
			//	EmptyChunk(coord);
			//	chunks.Remove(coord);
			//}
			//
			//chunks.Add(coord, intersectingChunk);
			//chunks[coord].gameObject.SetActive(true);
			//
			//if (!chunkCoordDeformationDict.ContainsKey(coord))
			//{
			//	chunkCoordDeformationDict.Add(coord, intersectingChunk.deformations);
			//}
			//else
			//{
			//	chunkCoordDeformationDict[coord] = intersectingChunk.deformations;
			//}
		}

		UpdateOreMatrices();

	}


	void RegeneratePreComputedChunk(Vector3Int coord, float timeToApply)
    {
		Chunk updatedChunk = CreateChunk(coord);
		updatedChunk.deformations = preComputedChunks[coord][timeToApply].deformations;


		uint[] shouldGenerateToSidesUint = new uint[12];
		sidesBuffer = new ComputeBuffer(12, sizeof(uint));
		sidesBuffer.SetData(shouldGenerateToSidesUint);

		GenerateChunkDensity(updatedChunk);
		GenerateChunk(updatedChunk);

		//sidesBuffer.GetData(shouldGenerateToSidesUint);

		sidesBuffer.Release();

		//bool[] shouldGenerateToSides = new bool[12];
		//
		//for (int i = 0; i < 12; i++)
		//{
		//	if (shouldGenerateToSidesUint[i] == 0) shouldGenerateToSides[i] = false;
		//	if (shouldGenerateToSidesUint[i] == 1) shouldGenerateToSides[i] = true;
		//}
		//updatedChunk.shouldGenerateToSides = shouldGenerateToSides;

		updatedChunk.gameObject.SetActive(false);

		GameObject oldChunk = preComputedChunks[coord][timeToApply].gameObject;
		preComputedChunks[coord][timeToApply] = updatedChunk;
		Destroy(oldChunk);
	}



	public Dictionary<Vector3Int, PointData> SamplePoints(List<Vector3> points)
    {
		Dictionary<Vector3Int, List<Vector3Int>> sampledChunks = new Dictionary<Vector3Int, List<Vector3Int>>();
		Dictionary<Vector3Int, PointData> pointValues = new Dictionary<Vector3Int, PointData>();



		for (int i = 0; i < points.Count; i++)
        {
			Vector3Int chunkCoord = Vector3Int.FloorToInt(points[i] / (chunkSize - 1));

			Vector3Int chunkPoint = Vector3Int.FloorToInt(points[i]) - (chunkCoord * (chunkSize - 1));

			if (!sampledChunks.ContainsKey(chunkCoord))
            {
				List<Vector3Int> chunkPoints = new List<Vector3Int>();
				chunkPoints.Add(chunkPoint);
				sampledChunks.Add(chunkCoord, chunkPoints);
            }
            else
            {
				sampledChunks[chunkCoord].Add(chunkPoint);
            }
        }

		foreach (Vector3Int chunkCoord in sampledChunks.Keys)
        {
			pointsBuffer2 = new ComputeBuffer(chunkSize * chunkSize * chunkSize, System.Runtime.InteropServices.Marshal.SizeOf(typeof(PointData)));



			uint[] shouldGenerateToSidesUint = new uint[12];
			sidesBuffer = new ComputeBuffer(12, sizeof(uint));
			sidesBuffer.SetData(shouldGenerateToSidesUint);

			if (chunks.ContainsKey(chunkCoord) && chunks[chunkCoord].deformations.Count > 0)
			{
				deformationBuffer = new ComputeBuffer(chunks[chunkCoord].deformations.Count, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Deformation)));
				deformationBuffer.SetData(chunks[chunkCoord].deformations);
			}
            else
            {
				deformationBuffer = new ComputeBuffer(1, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Deformation)));
			}

			//Debug.Log(chunkCoord);
			//Debug.Log(sampledChunks[chunkCoord]);

			Vector3 chunkOffset = chunkCoord * (chunkSize - 1);


			//fgireosgfjnriojfvmngiosfv-----------------------------------------------------------------------------------------

			pointsBuffer2 = GenerateNoiseValues(pointsBuffer2, chunkOffset);

			PointData[] allData = new PointData[chunkSize * chunkSize * chunkSize];
			pointsBuffer2.GetData(allData);
			//Debug.Log(allData[Random.Range(0, chunkSize * chunkSize * chunkSize)].density);


			for (int i = 0; i < sampledChunks[chunkCoord].Count; i++)
            {
				PointData[] chunkPointValue = new PointData[1];

				//Debug.Log(sampledChunks[chunk][i]);
				//pointsBuffer2.GetData(chunkPointValue, 0, IndexFromCoord(sampledChunks[chunkCoord][i].x, sampledChunks[chunkCoord][i].y, sampledChunks[chunkCoord][i].z), 1);
				//pointsBuffer2.GetData(chunkPointValue);

				//Debug.DrawRay(chunkCoord * (chunkSize - 1) + sampledChunks[chunkCoord][i], Vector3.up * 0.2f, Color.red, 5000);

				if (!pointValues.ContainsKey(chunkCoord))
				{
					pointValues.Add(chunkCoord * (chunkSize - 1) + sampledChunks[chunkCoord][i], allData[IndexFromCoord(sampledChunks[chunkCoord][i].x, sampledChunks[chunkCoord][i].y, sampledChunks[chunkCoord][i].z)]);
				}
			}

			

			sidesBuffer.Release();
			deformationBuffer.Release();
			pointsBuffer2.Dispose();
			//roomBuffer.Release();
		}

		//Debug.Log(sampledChunks.Count);

		return pointValues;

	}


	void CreateBuffers()
    {
		int numThreadsPerAxis = Mathf.CeilToInt((chunkSize - 1) / (float)8);
		int numVoxels = (chunkSize - 1) * (chunkSize - 1) * (chunkSize - 1);
		int maxTriangleCount = numVoxels * 5;
		int maxVertexCount = maxTriangleCount * 3;

		triangleBuffer = new ComputeBuffer(maxTriangleCount, System.Runtime.InteropServices.Marshal.SizeOf(typeof(VertexData)), ComputeBufferType.Append);
		triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
		vertexDataArray = new VertexData[maxVertexCount];
	}

	void ReleaseBuffers()
	{
		if (triangleBuffer != null)
		{
			triangleBuffer.Release();
			pointsBuffer.Release();
			triCountBuffer.Release();
			deformationBuffer.Release();
			//hasHolesBuffer.Release();
		}
	}


	public List<Chunk> GetNearbyChunks()
    {
		List<Chunk> nearbyChunks = new List<Chunk>();

		//nearbyChunks.Add(chunks[playerChunkPos]);

		for (int x = -2; x < 3; x++)
		{
			for (int y = -2; y < 3; y++)
			{
				for (int z = -2; z < 3; z++)
				{
					if (chunks.ContainsKey(playerChunkPos + new Vector3Int(x, y, z)))
					{
						nearbyChunks.Add(chunks[playerChunkPos + new Vector3Int(x, y, z)]);
					}
				}
			}
		}
		return nearbyChunks;

	}


    private void OnApplicationQuit()
    {
		ReleaseBuffers();
		heightEvaluationBuffer.Release();

		roomBuffer.Release();
	}
}
