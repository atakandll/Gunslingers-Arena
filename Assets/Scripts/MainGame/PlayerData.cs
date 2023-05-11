using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct PlayerData : INetworkInput // this class will contain any player ınput data we need to sync.
{

    public float HorizontalInput;
    public Quaternion GunPivotRotation;
    public NetworkButtons NetworkButtons;
}
