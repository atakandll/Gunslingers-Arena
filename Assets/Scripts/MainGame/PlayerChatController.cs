using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using System;
using UnityEngine.UI;

public class PlayerChatController : NetworkBehaviour
{
    public static bool IsTyping { get; private set; }
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Animator bubbleAnimator;
    [SerializeField] private TextMeshProUGUI bubbleText;

    public override void Spawned()
    {
        var isLocalPlayer = Object.InputAuthority == Runner.LocalPlayer; // local player mıyım?
        gameObject.SetActive(isLocalPlayer); // ozaman bubble paneli aç

        if (isLocalPlayer)
        {
            inputField.onSelect.AddListener(args => IsTyping = true); // we are typing
            inputField.onDeselect.AddListener(args => IsTyping = false);

            inputField.onSubmit.AddListener(onInputFieldSubmit);
        }
    }

    private void onInputFieldSubmit(string arg0)
    {
        if (!string.IsNullOrEmpty(arg0))
        {
            RpcSetBubbleSpeech(arg0);

        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RpcSetBubbleSpeech(NetworkString<_256> txt)
    {
        bubbleText.text = txt.Value;
        const string TRIGGER = "Open";
        bubbleAnimator.SetTrigger(TRIGGER);
    }

}
