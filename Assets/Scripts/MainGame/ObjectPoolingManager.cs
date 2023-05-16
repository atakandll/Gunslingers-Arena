using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ObjectPoolingManager : MonoBehaviour, INetworkObjectPool // olan bişeyi geridönüştürmeye yarıyor(recyling).
{
    //for example bullet prefab is key
    //bullet spawned object ("bullet(clone)) is part of the list itseld(the value)
    private Dictionary<NetworkObject, List<NetworkObject>> prefabsThatHadBeenInstantiated = new(); // first key and is going to be prefab itself, second value(the list) is going to be the actual game object that we are being instantiated during runtime

    private void Start()
    {
        if (GlobalManagers.instance != null)
        {
            GlobalManagers.instance.objectPoolingManager = this;
        }

    }


    // the idea is to actually keep track on all of the prefabs we already instantiaded in the past , prefabları takip edicez.
    public NetworkObject AcquireInstance(NetworkRunner runner, NetworkPrefabInfo info) // is going to be called to be every time a runner.spawn is called in general.
    {
        NetworkObject networkObject = null;

        NetworkProjectConfig.Global.PrefabTable.TryGetPrefab(info.Prefab, out var prefab);

        prefabsThatHadBeenInstantiated.TryGetValue(prefab, out var networkObjects);

        bool foundMatch = false;

        if (networkObjects?.Count > 0) // ? means it is not null.
        {
            foreach (var item in networkObjects)
            {
                if (item != null & item.gameObject.activeSelf == false) // if it not null and item is turned off.
                {
                    //todo ObjectPooling aka recycle
                    networkObject = item;

                    foundMatch = true;
                    break;

                }


            }
        }
        //stays false when a complete new data that is ot in our dc OR
        //When the function is getting called too fast and no object is ready to be recyled.
        if (foundMatch == false) // we know that we want to create new one
        {
            //todo create new object(spawning) and add it our dic.
            networkObject = CreateObjectInstance(prefab);
        }
        return networkObject;

    }
    private NetworkObject CreateObjectInstance(NetworkObject prefab)
    {
        var obj = Instantiate(prefab);

        if (prefabsThatHadBeenInstantiated.TryGetValue(prefab, out var instanceData)) // if the condition is true it means that it found the same key in our dictionary
        {
            //todo update the list itself
            instanceData.Add(obj);

        }
        else // it means that it's a complete new data and add dic.
        {
            var list = new List<NetworkObject>();
            list.Add(obj);

            prefabsThatHadBeenInstantiated.Add(prefab, list); // dictionary ekledik, burada key prefab ona dikkat et.

        }
        return obj;
    }


    public void ReleaseInstance(NetworkRunner runner, NetworkObject instance, bool isSceneObject)  // called once runner.despawn is called, cuurently player leaves the game or when we do runner that this spawn for the bullets.
    {
        instance.gameObject.SetActive(false);
    }

    public void RemoveNetworkObjectFromDic(NetworkObject obj)
    {
        if (prefabsThatHadBeenInstantiated.Count > 0)
        {
            foreach (var item in prefabsThatHadBeenInstantiated)
            {
                foreach (var networkObject in item.Value)
                {
                    if (networkObject == obj)
                    {
                        item.Value.Remove(networkObject);
                        break;
                    }

                }

            }
        }
    }


}
