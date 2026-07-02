using Fusion;
using TMPro;
using UnityEngine;

public class ReadyButtonManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private ReadyButtonBehaviour _readyButton;
    [SerializeField] private TextMeshProUGUI _playerReadyText;

    [Networked, OnChangedRender(nameof(UpdateReadyPlayersText))]
    private int ReadyPlayers { get; set; } = 0;

    [Networked, OnChangedRender(nameof(UpdateReadyPlayersText))]
    private int ActivePlayers { get; set; } = 0;

    private void OnEnable()
    {
        _readyButton.ReadyButtonClicked += OnReadyButtonClicked;
    }

    private void OnDisable()
    {
        _readyButton.ReadyButtonClicked -= OnReadyButtonClicked;
    }

    public void PlayerJoined(PlayerRef player)
    {
        ActivePlayers++;
    }

    public void PlayerLeft(PlayerRef player)
    {
        ActivePlayers--;
    }

    private void UpdateReadyPlayersText()
    {
        _playerReadyText.text = $"{ReadyPlayers}/{ActivePlayers}";
    }

    private void OnReadyButtonClicked(bool isReady)
    {
        ReadyPlayers += isReady ? 1 : -1;
    }
}
