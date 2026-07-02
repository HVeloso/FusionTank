using Fusion;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class TankControl : NetworkBehaviour, IDamageable
{
    [Header("Components")]
    [SerializeField] private Transform _cameraHolder;
    [SerializeField] private GameObject _individualUI;
    [SerializeField] private TextMeshPro _reloadingText;
    [Space]
    [SerializeField] private Animator _fireAnimator;
    [SerializeField] private DamageEffectHandler _hitEffect;
    private SpawnManager _spawnManager;

    [Header("General Parameters")]
    [SerializeField] private Transform _cameraPivot;
    [SerializeField, Min(0)] private float _cameraRotationSpeed = 45f;
    private float _cameraYaw = 0f;
    private bool _isCameraRotationUnlocked = false;
    [SerializeField, Min(1)] private int _tankHealth = 3;
    private int _currentHealth;

    [Header("Tank Body Parameters")]
    [SerializeField] private Transform _tankTransform;
    [SerializeField, Min(0)] private float _speed = 5f;
    [SerializeField, Min(0)] private float _tankRotationSpeed = 30f;

    [Header("Turret Parameters")]
    [SerializeField] private GameObject _muzzleCamera;
    [SerializeField] private Transform _turretTransform;
    [SerializeField, Min(0)] private float _turretRotationSpeed = 30f;
    [SerializeField] private Transform _turretMuzzle;
    [SerializeField, Min(0)] private float _fireCooldown = 1.4f;

    // Network Parameters
    [Networked, OnChangedRender(nameof(TurretRotate))]
    private float TurretYaw { get; set; }

    [Networked, OnChangedRender(nameof(BodyRotate))]
    private float BodyYaw { get; set; }

    [Networked, OnChangedRender(nameof(UpdateRigidBodyLinearVelocityOnline))]
    private Vector3 rigidBodyLinearVelocity { get; set; }

    // Internal Parameters
    private float _fireTimer = 0f;
    private Vector3 _movementInput = Vector3.zero;
    private float _turretRotationDirection = 0f;
    private bool _fireRequested = false;

    // Components
    private Rigidbody _tankRb;
    private InputSystem_Actions _inputAction;

    private void Awake()
    {
        _tankRb = GetComponent<Rigidbody>();

        _inputAction = new();
        _inputAction.Enable();

        _currentHealth = _tankHealth;
    }

    public void RegisterInputs()
    {
        _inputAction.Tank.Move.performed += Move;
        _inputAction.Tank.Aim.performed += Aim;
        _inputAction.Tank.Fire.performed += Fire;
        _inputAction.Tank.CameraMove.started += CameraMoveStarted;
        _inputAction.Tank.CameraMove.canceled += CameraMoveCanceled;
    }

    private void OnDisable()
    {
        _inputAction.Tank.Move.performed -= Move;
        _inputAction.Tank.Aim.performed -= Aim;
        _inputAction.Tank.Fire.performed -= Fire;
        _inputAction.Tank.Fire.performed -= Fire;
        _inputAction.Tank.CameraMove.performed -= CameraMoveStarted;
        _inputAction.Tank.CameraMove.performed -= CameraMoveCanceled;
    }

    private void Update()
    {
        TankMovement();
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    public override void FixedUpdateNetwork()
    {
        CountdownTimer();

        PerformFire();
        BodyRotate();
        TurretRotate();

        //rigidBodyLinearVelocity = _tankRb.linearVelocity;
    }

    private void Move(InputAction.CallbackContext context)
    {
        _movementInput = context.ReadValue<Vector2>();

        if (_movementInput.x != 0f)
            _movementInput.x = Mathf.Sign(_movementInput.x);

        if (_movementInput.y != 0f)
            _movementInput.y = Mathf.Sign(_movementInput.y);
    }

    private void Aim(InputAction.CallbackContext context)
    {
        _turretRotationDirection = context.ReadValue<Vector2>().x;

        if (_turretRotationDirection != 0f)
            _turretRotationDirection = Mathf.Sign(_turretRotationDirection);
    }

    private void Fire(InputAction.CallbackContext context)
    {
        if (!context.ReadValueAsButton() || _fireTimer > 0f) return;
        _fireRequested = true;
    }

    private void CameraMoveStarted(InputAction.CallbackContext context)
    {
        _isCameraRotationUnlocked = true;
    }

    private void CameraMoveCanceled(InputAction.CallbackContext context)
    {
        _isCameraRotationUnlocked = false;
    }

    private void PerformFire()
    {
        if (!_fireRequested) return;

        _fireTimer = _fireCooldown;
        Rpc_FireAnimation();

        List<RaycastHit> hits = Physics.RaycastAll(_turretMuzzle.position, _turretMuzzle.forward).ToList();
        hits.Sort((x, y) => Vector3.Distance(x.point, _turretMuzzle.position).CompareTo(Vector3.Distance(y.point, _turretMuzzle.position)));

        for (int i = hits.Count - 1; i >= 0; i--)
        {
            if (hits[i].collider.gameObject == gameObject || hits[i].collider.TryGetComponent(out SpawnPoint _))
            {
                hits.RemoveAt(i);
                continue;
            }
        }

        if (hits.Count > 0)
        {
            IDamageable damageable = hits[0].collider.GetComponentInParent<IDamageable>();
            damageable?.TakeDamage(hits[0].point);
        }

        _fireRequested = false;
    }

    private void CameraRotation()
    {
        if (!_isCameraRotationUnlocked) return;

        _cameraYaw += _turretRotationDirection * _cameraRotationSpeed * Time.deltaTime;
        _cameraPivot.localRotation = Quaternion.Euler(0f, _cameraYaw, 0f);
    }

    private void TankMovement()
    {
        if (HasStateAuthority)
            _tankRb.linearVelocity = _speed * _movementInput.y * _tankTransform.forward;
    }

    private void UpdateRigidBodyLinearVelocityOnline()
    {
        _tankRb.linearVelocity = rigidBodyLinearVelocity;
    }

    private void BodyRotate()
    {
        if (HasStateAuthority)
        {
            BodyYaw += _movementInput.x * _tankRotationSpeed * Runner.DeltaTime;
        }

        _tankTransform.localRotation = Quaternion.Euler(0f, BodyYaw, 0f);
    }

    private void TurretRotate()
    {
        if (_isCameraRotationUnlocked) return;

        if (HasStateAuthority)
        {
            TurretYaw += _turretRotationDirection * _turretRotationSpeed * Runner.DeltaTime;
        }

        _turretTransform.localRotation = Quaternion.Euler(0f, TurretYaw, 0f);
    }

    public void SetSpawnManager(SpawnManager spawnManager)
    {
        _spawnManager = spawnManager;
    }

    public void TakeDamage(Vector3 hitPoint)
    {
        Rpc_DealDamage(hitPoint);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void Rpc_DealDamage(Vector3 hitPoint)
    {
        _currentHealth--;

        if (_currentHealth > 0)
        {
            Rpc_HitAnimation(hitPoint);
        }
        else
        {
            transform.position = _spawnManager.GetSpawnPosition();
            _currentHealth = _tankHealth;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_FireAnimation()
    {
        _fireAnimator.SetTrigger("Fire");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void Rpc_HitAnimation(Vector3 hitPoint)
    {
        _hitEffect.RunEffect(hitPoint);
    }

    private void CountdownTimer()
    {
        _reloadingText.gameObject.SetActive(_fireTimer > 0f);
        _reloadingText.text = $"Reloading.. {_fireTimer:0.0}s";

        if (_fireTimer > 0f)
            _fireTimer -= Runner.DeltaTime;
    }

    public override void Spawned()
    {
        _individualUI.SetActive(HasStateAuthority);
        _muzzleCamera.SetActive(HasStateAuthority);

        if (!HasStateAuthority) return;

        Transform camera = Camera.main.transform;

        camera.parent = _cameraHolder;
        camera.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }
}
