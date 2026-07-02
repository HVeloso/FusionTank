using Fusion;
using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private SpawnManager _spawnManager;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private TextMeshProUGUI _pingText;

    public void PlayerJoined(PlayerRef player)
    {
        if (player != Runner.LocalPlayer) return;

        Vector3 spawnPosition = _spawnManager.GetSpawnPosition();

        NetworkObject _player = Runner.Spawn(
            _playerPrefab, spawnPosition, new Quaternion(0f, Random.Range(0f, 360f), 0f, 0f));

        TankControl tankControl = _player.GetComponent<TankControl>();
        tankControl.SetSpawnManager(_spawnManager);
        tankControl.RegisterInputs();

        StartCoroutine(UpdatePing());
    }

    private IEnumerator UpdatePing()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            _pingText.text = $"{Runner.GetPlayerRtt(Runner.LocalPlayer) * 1000f:000} ms";
        }
    }
}
