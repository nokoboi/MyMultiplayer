using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Netcode;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public FixedString64Bytes playerName;
    public ulong clientId; 
    public int skinIndex;
    public Color color;
    public FixedString64Bytes playerId;
    public bool isPlayerReady;


    public bool Equals(PlayerData other)
    {
        return
            playerName == other.playerName &&
            skinIndex == other.skinIndex &&
            color == other.color &&
            playerId == other.playerId &&
            isPlayerReady == other.isPlayerReady;
    }


    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref clientId);        
        serializer.SerializeValue(ref skinIndex);   
        serializer.SerializeValue(ref color);
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref isPlayerReady);
    }
}
