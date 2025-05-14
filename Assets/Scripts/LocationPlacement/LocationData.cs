using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LocationData
{
    public string Id;            // �������� "forest1"
    public string Biome;         // �������� "Forest"

    [Header("Roads from this location")]
    public List<RoadConnection> Roads = new List<RoadConnection>();

    // ��� ���� ����� ���������, �� �� ���������� � ����������
    [NonSerialized] public Dictionary<LocationData, float> DesiredRoads = new();
    [NonSerialized] public List<ChunkManager.Chunk> CandidateChunks = new();
    [NonSerialized] public ChunkManager.Chunk AssignedChunk;
}

[Serializable]
public class RoadConnection
{
    [Tooltip("Target location � �������� �� ������ ����")]
    public LocationData target;

    [Tooltip("�������� ����� ������ � �������� �����")]
    public float distance;
}
