using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private Camera cam;
    public override void Spawned()
    {
        base.Spawned();
        cam.gameObject.SetActive(false);

    }

}
