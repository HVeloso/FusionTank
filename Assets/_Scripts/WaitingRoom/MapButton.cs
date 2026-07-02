using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MapButton : MonoBehaviour
{
    //[SerializeField] private SceneAsset _targetScene;
    [SerializeField] private Button _mapButton;
    [SerializeField] private GameObject _selectedSprite;
    [SerializeField] private TextMeshProUGUI _numberOfVotes;

    public string SceneName => "";// _targetScene.name;
    public Transform ButtonTransform => transform;
    
    public event Action<MapButton> MapButtonClicked;

    private void Awake()
    {
        _mapButton.onClick.AddListener(OnButtonClicked);
    }

    private void OnDisable()
    {
        _mapButton.onClick.RemoveListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        MapButtonClicked?.Invoke(this);
    }

    public void SetSelected(bool state)
    {
        _selectedSprite.SetActive(state);
    }

    public void SetNumberOfVotes(int numberOfVotes)
    {
        _numberOfVotes.text = $"x{numberOfVotes}";
    }
}
