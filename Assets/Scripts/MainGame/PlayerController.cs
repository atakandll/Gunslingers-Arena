using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class PlayerController : NetworkBehaviour, IBeforeUpdate
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private GameObject cam;
    [SerializeField] private float moveSpeed = 6;
    [SerializeField] private float jumpForce = 1000;

    [Networked(OnChanged = nameof(OnNicknameChanged))] private NetworkString<_8> playerName { get; set; } // isim senkronizasyonu için yapıyoruz.

    [Networked] private NetworkButtons buttonsPrev { get; set; } //  hosta senkronize etmek için yapıyoruz.[network]

    private float horizontal;
    private Rigidbody2D rigid;

    private enum PlayerInputButtons
    {
        None,
        Jump
    }

    public override void Spawned() // fusionun startı
    {
        rigid = GetComponent<Rigidbody2D>();

        SetLocalObjects();
    }
    private void SetLocalObjects()
    {
        if (Runner.LocalPlayer == Object.HasInputAuthority) // eğer local player isem
        {
            cam.SetActive(true);
            var nickName = playerName = GlobalManagers.instance.networkRunnerController.LocalPlayerNickname;
            RpcSetNickName(nickName);

        }

    }
    // sends RPC to the Host ( from a client)
    // sources define which PEER can send the rpc
    // The RpcTargets defines on which it is executed!
    [Rpc(sources: RpcSources.InputAuthority, RpcTargets.StateAuthority)] // clientteki nickname hatasını düzeltmek için böyle yapıyoruz.
    private void RpcSetNickName(NetworkString<_8> nickname)
    {
        playerName = nickname;
    }
    private static void OnNicknameChanged(Changed<PlayerController> changed) // static olmalı fusion hata veriyor
    {
        var nickName = changed.Behaviour.playerName;
        changed.Behaviour.SetPlayerNickname(nickName); // nickname variable is the current updated one

    }
    private void SetPlayerNickname(NetworkString<_8> nickname)
    {
        playerNameText.text = nickname + " " + Object.InputAuthority.PlayerId;
    }


    //Happens before anything else Fusion does, network application, reconlation vs.
    //Called at the start of the Fusion Update loop, before the Fusion simulation loop.
    //It fires before Fusion does ANY work, every screen refresh.
    public void BeforeUpdate() // fusionun yaptığı herşeyden önce buraya tnaımladık.
    {
        //We are checking if we are the local player
        if (Runner.LocalPlayer == Object.HasInputAuthority) // eğer local playersak horizantalı kuruyoruz.
        {
            const string HORIZONTAL = "Horizontal";
            horizontal = Input.GetAxisRaw(HORIZONTAL);
        }
    }

    //F U N
    public override void FixedUpdateNetwork()
    {
        // will return false if:
        // the client does not have state authority or input authoriy;
        // the request type of input does not exist in simulation.
        // Hareket ettirdiğimiz kısım.
        if (Runner.TryGetInputForPlayer<PlayerData>(Object.InputAuthority, out var input)) // // this out keyword will find PlayerData script and assign the value all the information from that script and put it into input variable.
        {
            rigid.velocity = new Vector2(input.HorizontalInput * moveSpeed, rigid.velocity.y);

            CheckJumpInput(input);
        }
    }
    private void CheckJumpInput(PlayerData input)
    {
        var pressed = input.NetworkButtons.GetPressed(buttonsPrev);

        if (pressed.WasPressed(buttonsPrev, PlayerInputButtons.Jump))
        {
            rigid.AddForce(Vector2.up * jumpForce, ForceMode2D.Force);
        }

        // without a network attribute, this will actually be set only locally and this condyion will never be true,
        buttonsPrev = input.NetworkButtons; // always update the buttons pressed to network buttons

    }

    public PlayerData GetPlayerNetworkInput()
    {
        PlayerData data = new PlayerData();
        data.HorizontalInput = horizontal; // datadaki horizontol input equals to our local variable input
        data.NetworkButtons.Set(PlayerInputButtons.Jump, Input.GetKey(KeyCode.Space)); // zıplama yerini kullandık ve burada data.set yaptık set içinde int ve bool tipinde değişkenler alıyor.
        return data; // then we will return the data.
    }

}
