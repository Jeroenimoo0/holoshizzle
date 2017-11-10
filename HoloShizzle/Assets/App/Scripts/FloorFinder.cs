using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using HoloToolkitExtensions.Messaging;
using UnityEngine;

public class FloorFinder : MonoBehaviour
{
    public float MaxDistance = 3.0f;

    public float MinHeight = 1.0f;

    private Vector3? _foundPosition = null;

    public GameObject LabelText;

    private float _delayMoment;

    void Start()
    {
        _delayMoment = Time.time + 2;
        Messenger.Instance.AddListener<PositionFoundMessage>(ProcessMessage);
#if !UNITY_EDITOR
        Reset();
#else
        LabelText.SetActive(false);
#endif
    }

    void Update()
    {
        if (_foundPosition == null && Time.time > _delayMoment)
        {
            _foundPosition = GetPostionInLookingDirection();
            if (_foundPosition != null)
            {
                if (GazeManager.Instance.Stabilizer.StablePosition.y - _foundPosition.Value.y > MinHeight)
                {
                    Messenger.Instance.Broadcast(new PositionFoundMessage(_foundPosition.Value));
                    PlayConfirmationSound();
                }
                else
                {
                    _foundPosition = null;
                }
            }
        }
    }

    public void Reset()
    {
        _delayMoment = Time.time + 2;
        _foundPosition = null;
        if(LabelText!= null) LabelText.SetActive(true);
    }

    private Vector3? GetPostionInLookingDirection()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(GazeManager.Instance.Stabilizer.StableRay,
            out hitInfo, MaxDistance, SpatialMappingManager.Instance.LayerMask))
        {
            return hitInfo.point;
        }

        return null;
    }

    private void ProcessMessage(PositionFoundMessage message)
    {
        if (message.Status == PositionFoundStatus.Rejected)
        {
            Reset();
        }
        else
        {
            LabelText.SetActive(false);
        }
    }

    private void PlayConfirmationSound()
    {
        Messenger.Instance.Broadcast(new ConfirmSoundMessage());
    }
}
