using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using System;

public class GameManager : NetworkBehaviour
{
    public event Action OnGameIsOver;

    [field: SerializeField] public Collider2D CameraBounds { get; private set; }
    public static bool MatchIsOver { get; private set; } // playercontrollerdan ulaşmak için staic yaptık.
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float matchTimerAmount = 10;
    [SerializeField] private Camera cam;
    [Networked] TickTimer matchTimer { get; set; }

    private void Awake()
    {
        if (GlobalManagers.instance != null)
        {
            GlobalManagers.instance.gameManager = this;
        }

    }
    public override void Spawned()
    {
        //reset this var
        MatchIsOver = false; //static olduğu için başta bi false yapmak daha doğru

        cam.gameObject.SetActive(false);

        matchTimer = TickTimer.CreateFromSeconds(Runner, matchTimerAmount); // ticktimera ekleyeceğimiz saniye.

    }

    public override void FixedUpdateNetwork()
    {
        if (matchTimer.Expired(Runner) == false && matchTimer.RemainingTime(Runner).HasValue)
        {
            //from seconds saniye ekliyor.
            var timeSpan = TimeSpan.FromSeconds(matchTimer.RemainingTime(Runner).Value); // elimizdeki bir değere göre süre tutmamızı sağlıyor timespan.

            var outPut = $"{timeSpan.Minutes:D2}: {timeSpan.Seconds:D2}";

            timerText.text = outPut;
        }
        else if (matchTimer.Expired(Runner))
        {
            MatchIsOver = true;
            matchTimer = TickTimer.None;
            OnGameIsOver?.Invoke();
            Debug.Log("Match timer is over");
        }
    }

}
