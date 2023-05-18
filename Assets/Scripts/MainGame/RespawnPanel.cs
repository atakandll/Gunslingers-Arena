using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class RespawnPanel : SimulationBehaviour // network behoviour şeyleri kullanmamıza gerek yok ama yine de simulasyonda kalıyoruz
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private TextMeshProUGUI respawnAmountText;
    [SerializeField] private GameObject childObject;

    public override void FixedUpdateNetwork()
    {
        if (Utils.IsLocalPlayer(Object)) // local player harici gösterim için( oynayan kişi öldürdüğünde diğer clientin ekranında gözükecek.)
        {
            var timerIsRunning = playerController.RespawnTimer.IsRunning;
            childObject.SetActive(timerIsRunning); // timer çalışıcaksa bu aktif olucak bool değişken yukardaki

            if (timerIsRunning && playerController.RespawnTimer.RemainingTime(Runner).HasValue)
            {
                var time = playerController.RespawnTimer.RemainingTime(Runner).Value; // kalan süre

                var roundInt = Mathf.RoundToInt(time);

                respawnAmountText.text = roundInt.ToString();

            }

        }


    }
}
