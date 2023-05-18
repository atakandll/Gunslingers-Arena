using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class PlayerController : NetworkBehaviour, IBeforeUpdate
{
    public bool AcceptAnyInput => PlayerIsAlive && !GameManager.MatchIsOver && !playerChatController.IsTyping;

    [SerializeField] private PlayerChatController playerChatController;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private GameObject cam;
    [SerializeField] private float moveSpeed = 6;
    [SerializeField] private float jumpForce = 1000;

    [Header("Grounded Vars")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundDetectionObj;
    [Networked] public TickTimer RespawnTimer { get; private set; }
    [Networked] public NetworkBool PlayerIsAlive { get; private set; }

    [Networked(OnChanged = nameof(OnNicknameChanged))]
    private NetworkString<_8> playerName { get; set; } // isim senkronizasyonu için yapıyoruz.

    [Networked] private NetworkButtons buttonsPrev { get; set; } //  hosta senkronize etmek için yapıyoruz.[network]
    [Networked] private Vector2 serverNextSpawnPoint { get; set; }
    [Networked] private NetworkBool isGrounded { get; set; }
    [Networked] private TickTimer respawnToNewPointTimer { get; set; }

    private float horizontal;
    private Rigidbody2D rb;


    private PlayerWeaponController playerWeaponController;
    private PlayerVisualController playerVisualController;
    private PlayerHealthController playerHealthController;

    public enum PlayerInputButtons
    {
        None,
        Jump,
        Shoot
    }

    public override void Spawned() // fusionun startı
    {
        rb = GetComponent<Rigidbody2D>();

        playerWeaponController = GetComponent<PlayerWeaponController>();
        playerVisualController = GetComponent<PlayerVisualController>();
        playerHealthController = GetComponent<PlayerHealthController>();


        SetLocalObjects();
        PlayerIsAlive = true; //en başta player hayatta true


    }
    private void SetLocalObjects()
    {
        if (Utils.IsLocalPlayer(Object)) // eğer local player isem
        {
            cam.transform.SetParent(null); // kamerayı parenttan çıkarıyoruz
            cam.SetActive(true);

            var nickName = playerName = GlobalManagers.instance.networkRunnerController.LocalPlayerNickname;
            RpcSetNickName(nickName);

        }
        else
        {
            //if this is not our ınputAuthority player(proxy)
            //we want to make sure set this networkrigidbody(nrb) interpolationdatasource to snapshots
            // as it will automatically set all nrb to be predicted regardless if it is a proxy or not, as we are doing full pyhsics prediction.
            //and setting it back to snapshots for proxies, will also make sure that lag compensation will workd properly + be more cost efficient.
            GetComponent<NetworkRigidbody2D>().InterpolationDataSource = InterpolationDataSources.Snapshots;
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
    public void KillPlayer() // ölme animasyonu geliyor ve rigidbody özellikleri gidiyor
    {
        if (Runner.IsServer) // now the host only is hoing the catche this random spawn point.
        {
            //todo catch new spawn point. Networked de olduğu için bütün playerlara sync olucak.
            serverNextSpawnPoint = GlobalManagers.instance.playerSpawnerController.GetRandomSpawnPoint();
            respawnToNewPointTimer = TickTimer.CreateFromSeconds(Runner, 4f); // respawn olurken respawn süresi bitmeden hemen spawn olsun diye süreyi 5 den küçük yaptık.
        }
        PlayerIsAlive = false;
        rb.simulated = false;
        playerVisualController.TriggerDieAnimation();
        RespawnTimer = TickTimer.CreateFromSeconds(Runner, 5f);

    }


    //Happens before anything else Fusion does, network application, reconlation vs.
    //Called at the start of the Fusion Update loop, before the Fusion simulation loop.
    //It fires before Fusion does ANY work, every screen refresh.
    public void BeforeUpdate() // fusionun yaptığı herşeyden önce buraya tanımladık.
    {
        //We are checking if we are the local player
        if (Utils.IsLocalPlayer(Object) && AcceptAnyInput) // eğer local playersak horizantalı kuruyoruz.
        {
            const string HORIZONTAL = "Horizontal";
            horizontal = Input.GetAxisRaw(HORIZONTAL);
        }
    }

    //F U N
    public override void FixedUpdateNetwork()
    {
        CheckRespawnTimer();
        // will return false if:
        // the client does not have state authority or input authoriy;
        // the request type of input does not exist in simulation.
        // Hareket ettirdiğimiz kısım.
        // "InputAuthority" is likely an object that manages input for a specific player.
        // So, the code is checking whether the input authority can provide input data for a specific player of type "PlayerData". If it can, the input data is assigned to the "input" variable.
        if (Runner.TryGetInputForPlayer<PlayerData>(Object.InputAuthority, out var input)) // // this out keyword will find PlayerData script and assign the value all the information from that script and put it into input variable.
        {
            if (AcceptAnyInput)
            {
                rb.velocity = new Vector2(input.HorizontalInput * moveSpeed, rb.velocity.y);

                CheckJumpInput(input);

                // without a network attribute, this will actually be set only locally and this condyion will never be true,
                buttonsPrev = input.NetworkButtons; // always update the buttons pressed to network buttons

            }
            else
            {
                rb.velocity = Vector2.zero;

            }


        }
        playerVisualController.UpdateScaleTrasform(rb.velocity); // character flip
    }
    private void CheckRespawnTimer() // timerın süresi bittiğinde animation ve respawn yapmak istiyoruz.
    {
        if (PlayerIsAlive) return;

        // Will only run on the server.
        if (respawnToNewPointTimer.Expired(Runner))
        {
            GetComponent<NetworkRigidbody2D>().TeleportToPosition(serverNextSpawnPoint);
            respawnToNewPointTimer = TickTimer.None;
        }

        if (RespawnTimer.Expired(Runner))
        {
            RespawnTimer = TickTimer.None;
            //todo respawn method
            RespawnPlayer();
        }

    }
    public void RespawnPlayer() // respawn animasyonu geliyor ve rigidbody özellikleri geri geliyor.
    {
        PlayerIsAlive = true;
        rb.simulated = true;

        playerVisualController.TriggerRespawnAnimation();
        playerHealthController.ResetHealthAmountToMax();


    }

    // Runs after all simulations have finished.
    // Use in place of Unity's Update when Fusion is handling Physics.
    // animasyonlar için güzel kullanım yeri
    public override void Render()
    {
        playerVisualController.RendererVisuals(rb.velocity, playerWeaponController.isHoldingShootingKey);

    }
    private void CheckJumpInput(PlayerData input)
    {
        isGrounded = (bool)Runner.GetPhysicsScene2D().OverlapBox(groundDetectionObj.transform.position, groundDetectionObj.transform.localScale, 0, groundLayer);

        if (isGrounded)
        {
            var pressed = input.NetworkButtons.GetPressed(buttonsPrev);

            if (pressed.WasPressed(buttonsPrev, PlayerInputButtons.Jump))
            {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Force);
            }

        }



    }
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        GlobalManagers.instance.objectPoolingManager.RemoveNetworkObjectFromDic(Object);
        Destroy(gameObject);
    }

    public PlayerData GetPlayerNetworkInput() // playerdataları işlediğimiz yer. Buraya değerleri gönderiyuz FUN da alıyoruz.
    {
        PlayerData data = new PlayerData();

        data.HorizontalInput = horizontal; // datadaki horizontol input equals to our local variable input
        data.GunPivotRotation = playerWeaponController.localQuaternionPivotRot;
        data.NetworkButtons.Set(PlayerInputButtons.Jump, Input.GetKey(KeyCode.Space)); // zıplama yerini kullandık ve burada data.set yaptık set içinde int ve bool tipinde değişkenler alıyor.
        data.NetworkButtons.Set(PlayerInputButtons.Shoot, Input.GetButton("Fire1"));
        return data; // then we will return the data.
    }

}
