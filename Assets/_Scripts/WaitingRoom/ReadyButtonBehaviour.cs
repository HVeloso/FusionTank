using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ReadyButtonBehaviour : MonoBehaviour
{
    [SerializeField] private Image _buttonImage;
    [SerializeField] private TextMeshProUGUI _buttonText;
    private Button _readyButton;
    public event Action<bool> ReadyButtonClicked;

    private bool _isReady = false;

    private void Awake()
    {
        _readyButton = GetComponent<Button>();
    }

    private void OnEnable()
    {
        _readyButton.onClick.AddListener(OnClicked);
    }

    private void OnDisable()
    {
        _readyButton.onClick.RemoveListener(OnClicked);
    }

    private void OnClicked()
    {
        _isReady = !_isReady;
        ReadyButtonClicked?.Invoke(_isReady);

        if (_isReady)
        {
            _buttonImage.color = Color.red;
            _buttonText.color = Color.black;
            return;
        }

        _buttonImage.color = Color.green;
        _buttonText.color = Color.white;
    }
}
