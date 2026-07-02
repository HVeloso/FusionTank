using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private List<SpawnPoint> _spawnPoints = new();

    public Vector3 GetSpawnPosition()
    {
        int index = Random.Range(0, _spawnPoints.Count);
        int numberOfTries = 0;

        Vector3 spawnPoint = new(0f, 0.5f, 0f);

        while (numberOfTries < _spawnPoints.Count)
        {
            if (_spawnPoints[index].IsFree)
            {
                spawnPoint = _spawnPoints[index].SpawnPosition;
                break;
            }
            
            numberOfTries++;
            index++;
            if (index >= _spawnPoints.Count) index = 0;
        }

        return spawnPoint;
    }
}
