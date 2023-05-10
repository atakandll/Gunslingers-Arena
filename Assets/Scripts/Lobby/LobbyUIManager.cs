using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyUIManager : MonoBehaviour // enable and disable current requested panel.
{
    [SerializeField] private LoadingCanvasController loadingCanvasControllerPrefab;
    [SerializeField] private LobbyPanelBase[] lobbyPanels; // lobbypanelbase classını getirdik.


    void Start()
    {
        foreach (var lobby in lobbyPanels)
        {
            lobby.InitPanel(this);

        }

        Instantiate(loadingCanvasControllerPrefab);
    }

    public void ShowPanel(LobbyPanelBase.LobbyPanelType type)
    {
        foreach (var lobby in lobbyPanels)
        {
            if (lobby.PanelType == type)
            {
                // show new panel
                lobby.ShowPanel();

                break;
            }

        }
    }
}
