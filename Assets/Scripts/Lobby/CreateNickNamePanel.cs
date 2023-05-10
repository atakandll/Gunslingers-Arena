using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class CreateNickNamePanel : LobbyPanelBase
{
    [Header("CreateNicknamePanel Vars")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button createNicknameButton;
    private const int MAX_CHAR_FOR_NICKNAME = 2; // Sabit sayı belirledik.

    public override void InitPanel(LobbyUIManager lobbyUIManager)
    {
        base.InitPanel(lobbyUIManager);

        createNicknameButton.interactable = false; // başta false yaptık buttonu default olarak.
        createNicknameButton.onClick.AddListener(OnClickCreateNickname); // inspector yerine burdan yapmak istedim.
        inputField.onValueChanged.AddListener(OnInputValueChanged);

    }

    private void OnInputValueChanged(string arg0)
    {
        createNicknameButton.interactable = arg0.Length >= MAX_CHAR_FOR_NICKNAME;
    }

    private void OnClickCreateNickname() // eğer 2 harf veya dah fazla olmazsa button unable oluyor
    {
        var nickName = inputField.text;

        if (nickName.Length >= MAX_CHAR_FOR_NICKNAME)
        {
            base.ClosePanel(); // create panel kısmını kapattık 2. kısım açılsın diye.
            lobbyUIManager.ShowPanel(LobbyPanelType.MiddleSectionPanel); // middle section panelinin gözükeceği yer.

        }
    }
}
