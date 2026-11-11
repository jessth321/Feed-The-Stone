using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;


public struct VertexData
{
	public Vector3 position;
	public float terrainType;

}
public struct PointData
{
    public float density;
    public float terrainType;
};

[System.Serializable]
public struct Deformation
{
	public Vector3 position;
	public float radius;
	public float blend;
	public float noiseFreq;
	public float noiseOct;
	public float noiseAmp;
	public float noiseLac;
	public float noisePers;


	public Deformation(Vector3 _position, float _radius, float _blend, float _noiseFreq, float _noiseAmp, float _noiseOct, float _noiseLac, float _noisePers)
	{
		position = _position;
		radius = _radius;
		blend = _blend;
		noiseFreq = _noiseFreq;
		noiseAmp = _noiseAmp;
		noiseOct = _noiseOct;
		noiseLac = _noiseLac;
		noisePers = _noisePers;
	}
}

[System.Serializable]
public struct RoomData
{
    public uint id;
    public Vector3 position;
    public float height;
    public float heightBlend;
    public float radius;
    public float radiusOuter;
    public float radiusBlend;
    public uint connectedID1;
    public uint connectedID2;
    public uint connectedID3;
    public uint connectedID4;

    public RoomData(uint _id, Vector3 _position, float _height, float _heightBlend,
                   float _radius, float _radiusOuter, float _radiusBlend,
                   uint _connectedID1, uint _connectedID2, uint _connectedID3, uint _connectedID4)
    {
        id = _id;
        position = _position;
        height = _height;
        heightBlend = _heightBlend;
        radius = _radius;
        radiusOuter = _radiusOuter;
        radiusBlend = _radiusBlend;
        connectedID1 = _connectedID1;
        connectedID2 = _connectedID2;
        connectedID3 = _connectedID3;
        connectedID4 = _connectedID4;
    }

    // Helper method to get connected IDs as array
    public uint[] GetConnectedIDs()
    {
        return new uint[] { connectedID1, connectedID2, connectedID3, connectedID4 };
    }

    // Helper method to set connected IDs from array
    public void SetConnectedIDs(uint[] connectedIDs)
    {
        if (connectedIDs == null || connectedIDs.Length != 4)
            throw new System.ArgumentException("ConnectedIDs array must have exactly 4 elements");

        connectedID1 = connectedIDs[0];
        connectedID2 = connectedIDs[1];
        connectedID3 = connectedIDs[2];
        connectedID4 = connectedIDs[3];
    }

    // Indexer to access connected IDs by index
    public uint this[int index]
    {
        get
        {
            return index switch
            {
                0 => connectedID1,
                1 => connectedID2,
                2 => connectedID3,
                3 => connectedID4,
                _ => throw new System.IndexOutOfRangeException("Index must be between 0 and 3")
            };
        }
        set
        {
            switch (index)
            {
                case 0: connectedID1 = value; break;
                case 1: connectedID2 = value; break;
                case 2: connectedID3 = value; break;
                case 3: connectedID4 = value; break;
                default: throw new System.IndexOutOfRangeException("Index must be between 0 and 3");
            }
        }
    }

    // Property to get the number of connections
    public int ConnectionCount => 4;
}
