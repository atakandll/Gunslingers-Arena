using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using Random = UnityEngine.Random;

public class PlayerSpawnerController : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private NetworkPrefabRef playerNetworkPrefab = NetworkPrefabRef.Empty;
    [SerializeField] private Transform[] spawnPoints;

    private void Awake()
    {
        if (GlobalManagers.instance != null)
        {
            GlobalManagers.instance.playerSpawnerController = this;
        }
    }

    public override void Spawned()
    {
        if (Runner.IsServer) // burası host için yapıldı bir kere olucak
        {
            foreach (var item in Runner.ActivePlayers)
            {
                SpawnPlayer(item);

            }
        }
    }
    public Vector2 GetRandomSpawnPoint()
    {
        var index = Random.Range(0, spawnPoints.Length - 1);
        return spawnPoints[index].position;

    }

    // aşağıdaki spawnPlayerlar hosttan sonra gelicek playerları oluşturacak.

    private void SpawnPlayer(PlayerRef playerRef) // I want to instantiate the object only if I am the server in order to check if I am the server or not.
    {
        if (Runner.IsServer)
        {
            var index = playerRef % spawnPoints.Length; // 4 oyuncu varken 5. gelirken hata olmaması için

            var spawnPoint = spawnPoints[index].transform.position;

            var playerObject = Runner.Spawn(playerNetworkPrefab, spawnPoint, Quaternion.identity, playerRef);

            //playerı serverde yaptık.
            Runner.SetPlayerObject(playerRef, playerObject); // local machine player olucak
        }

    }

    void IPlayerJoined.PlayerJoined(PlayerRef player)
    {
        SpawnPlayer(player);
    }

    void IPlayerLeft.PlayerLeft(PlayerRef player)
    {
        DespawnPlayer(player);
    }

    private void DespawnPlayer(PlayerRef playerRef)
    {
        if (Runner.IsServer)
        {
            if (Runner.TryGetPlayerObject(playerRef, out var playerNetworkObject))
            {
                Runner.Despawn(playerNetworkObject);
            }

            // reset player object
            Runner.SetPlayerObject(playerRef, null);
        }
    }
}

