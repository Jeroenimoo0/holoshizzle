using UnityEngine;
using HoloToolkit.Unity.SpatialMapping;

namespace HoloToolkitExtensions.Animation
{
    public class KeepInViewController : MonoBehaviour
    {

        public float MaxDistance = 2f;

        public float MoveTime = 0.8f;

        private bool _isActive = false;

        private SpatialMappingManager _mappingManager;

        private SpatialMappingManager MappingManager
        {
            get
            {
                if (_mappingManager == null)
                {
                    _mappingManager = SpatialMappingManager.Instance;
                }
                return _mappingManager;
            }
        }

        public void SetActive(bool active)
        {
            _isActive = active;
        }

        // Use this for initialization
        void Start()
        {
            //Upon Instantiation, set the visibility to true
            SetActive(true);
        }

        // Update is called once per frame
        void Update()
        {
            if (!_isActive || _isBusy)
            {
                return;
            }

            _isBusy = true;
            LeanTween.moveLocal(transform.gameObject,
                    GetPostionInLookingDirection(), MoveTime).
                setEase(LeanTweenType.easeInOutSine).
                setOnComplete(MovingDone);
        }

        private void MovingDone()
        {
            _isBusy = false;
        }

        private bool _isBusy;

        private Vector3 GetPostionInLookingDirection()
        {

            RaycastHit hitInfo;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward,
                out hitInfo, MaxDistance, MappingManager.LayerMask))
            {
                return hitInfo.point;
            }

            return CalculatePositionDeadAhead(MaxDistance);
        }

        private Vector3 CalculatePositionDeadAhead(float distance)
        {
            return Camera.main.transform.position + (Camera.main.transform.forward * distance);
        }
    }
}