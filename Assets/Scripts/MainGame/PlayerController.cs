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
    private Rigidbody2D rb;

    private PlayerWeaponController playerWeaponController;
    private PlayerVisualController playerVisualController;

    private enum PlayerInputButtons
    {
        None,
        Jump
    }

    public override void Spawned() // fusionun startı
    {
        rb = GetComponent<Rigidbody2D>();

        playerWeaponController = GetComponent<PlayerWeaponController>();
        playerVisualController = GetComponent<PlayerVisualController>();

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
    // rpc are usually to be used so the clients could actually tell the server something and in our case
    // for instance telling the server to change our name

    [Rpc(sources: RpcSources.InputAuthority, RpcTargets.StateAuthority)] // clientteki nickname hatasını düzeltmek için böyle yapıyoruz.Çünkü ismi değiştirmiyor
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
        // "InputAuthority" is likely an object that manages input for a specific player.
        // So, the code is checking whether the input authority can provide input data for a specific player of type "PlayerData". If it can, the input data is assigned to the "input" variable.
        if (Runner.TryGetInputForPlayer<PlayerData>(Object.InputAuthority, out var input)) // // this out keyword will find PlayerData script and assign the value all the information from that script and put it into input variable.
        {
            rb.velocity = new Vector2(input.HorizontalInput * moveSpeed, rb.velocity.y);

            CheckJumpInput(input);
        }
    }

    // Runs after all simulations have finished.
    // Use in place of Unity's Update when Fusion is handling Physics.
    // animasyonlar için güzel kullanım yeri
    public override void Render()
    {
        playerVisualController.RendererVisuals(rb.velocity);

    }
    private void CheckJumpInput(PlayerData input)
    {
        var pressed = input.NetworkButtons.GetPressed(buttonsPrev);

        if (pressed.WasPressed(buttonsPrev, PlayerInputButtons.Jump))
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Force);
        }

        // without a network attribute, this will actually be set only locally and this condyion will never be true,
        buttonsPrev = input.NetworkButtons; // always update the buttons pressed to network buttons

    }

    public PlayerData GetPlayerNetworkInput() // playerdataları işlediğimiz yer.
    {
        PlayerData data = new PlayerData();

        data.HorizontalInput = horizontal; // datadaki horizontol input equals to our local variable input
        data.GunPivotRotation = playerWeaponController.localQuaternionPivotRot;
        data.NetworkButtons.Set(PlayerInputButtons.Jump, Input.GetKey(KeyCode.Space)); // zıplama yerini kullandık ve burada data.set yaptık set içinde int ve bool tipinde değişkenler alıyor.

        return data; // then we will return the data.
    }

}
