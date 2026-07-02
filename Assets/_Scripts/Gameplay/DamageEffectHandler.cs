using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DamageEffectHandler : MonoBehaviour
{
    private Animator _effectAnimator;

    private void Awake()
    {
        _effectAnimator = GetComponent<Animator>();
    }

    public void RunEffect(Vector3 hitPoint)
    {
        transform.position = hitPoint;
        _effectAnimator.SetTrigger("Fire");
    }
}
