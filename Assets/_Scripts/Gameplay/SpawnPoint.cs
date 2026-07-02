using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SpawnPoint : MonoBehaviour
{
    public bool IsFree { get; private set; } = true;
    public Vector3 SpawnPosition => transform.position;

    private void OnTriggerStay(Collider other)
    {
        IsFree = false;
    }

    private void OnTriggerExit(Collider other)
    {
        IsFree = true;
    }
}
