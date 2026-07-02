using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class MapButtonManager : NetworkBehaviour
{
    [SerializeField] private Transform _buttonsHolder;

    private readonly Dictionary<string, MapButton> _mapButtons = new();
    [Networked, Capacity(5), OnChangedRender(nameof(UpdateUIVotes))]
    private NetworkDictionary<NetworkString<_32>, int> MapVotes => default;

    private MapButton _lastMapButton;

    private void Awake()
    {
        LoadMapDictionary();
    }

    private void OnEnable()
    {
        foreach (MapButton mapButton in _mapButtons.Values)
        {
            mapButton.MapButtonClicked += OnMapButtonClicked;
        }
    }

    private void OnDisable()
    {
        foreach (MapButton mapButton in _mapButtons.Values)
        {
            mapButton.MapButtonClicked += OnMapButtonClicked;
        }
    }

    private void LoadMapDictionary()
    {
        foreach (Transform child in _buttonsHolder)
        {
            if (!child.TryGetComponent(out MapButton mapButton)) return;

            _mapButtons.Add(mapButton.SceneName, mapButton);
        }
    }

    private void OnMapButtonClicked(MapButton mapButton)
    {
        if (_lastMapButton != mapButton)
        {
            if (_lastMapButton != null)
            {
                Rpc_RemoveVote(_lastMapButton.SceneName);
                _lastMapButton.SetSelected(false);
            }

            Rpc_AddVote(mapButton.SceneName);
            mapButton.SetSelected(true);
            _lastMapButton = mapButton;
        }
    }

    private void UpdateUIVotes()
    {
        foreach (KeyValuePair<NetworkString<_32>, int> vote in MapVotes)
        {
            string key = vote.Key.ToString();

            if (!_mapButtons.ContainsKey(key)) return;

            _mapButtons[key].SetNumberOfVotes(vote.Value);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void Rpc_AddVote(string sceneName)
    {
        int value = 1;

        if (MapVotes.ContainsKey(sceneName))
            value += MapVotes[sceneName];

        MapVotes.Set(sceneName, value);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void Rpc_RemoveVote(string sceneName)
    {
        if (MapVotes.ContainsKey(sceneName) && MapVotes[sceneName] > 0)
            MapVotes.Set(sceneName, MapVotes[sceneName] - 1);
    }
}
