
using DG.Tweening;
using UnityEngine;
namespace PubScale.SdkOne.NativeAds.Hightower
{
    /// <summary>
    /// Follows the target with given offset
    /// </summary>
    public class FollowerAdHandler : MonoBehaviour
    {
        [SerializeField] private NativeAdHolder nativeAdHolder;
        [SerializeField] private Transform targetTransform;
        [SerializeField] private float smoothTime = 0.3f;
        [SerializeField] private Vector3 offset = Vector3.zero;

        private Vector3 velocity = Vector3.zero;
        private bool canFollow = false;
        private bool adLoaded;
        private bool gotFollowRequest;
        private bool Once = false;

        private void Awake()
        {
            nativeAdHolder.Event_AdLoaded += NativeAdHolder_Event_AdLoaded; //Subscribe to native ad loaded event
            nativeAdHolder.Event_AdFailed += NativeAdHolder_Event_AdFailed; //Subscribe to native ad failed event
            GameManager.ShowAdNow += GameManager_ShowAdNow;
            GameManager.HideAdNow += GameManager_HideAdNow;
        }

        private void GameManager_HideAdNow()
        {
            canFollow = false;
            gotFollowRequest = false;
            Vector3 offset = new Vector3(targetTransform.position.x + 10, targetTransform.position.y - 10, targetTransform.position.z);
            transform.DOMove(offset, 2).From(transform.position).OnComplete(() => {
                canFollow = false;
            });
        }

        private void GameManager_ShowAdNow(Transform obj)
        {
            targetTransform = obj;
            gotFollowRequest = true;
#if UNITY_EDITOR
            FollowPlayer();
#endif
            if (!adLoaded)
                return;
            FollowPlayer();
        }
       void FollowPlayer()
        {
            if (Once)
                return;
            Once = true;
            Vector3 offset = new Vector3(targetTransform.position.x - 10, targetTransform.position.y + 10, targetTransform.position.z);
            transform.DOMove(targetTransform.position + this.offset, 2).From(offset).SetEase(Ease.Linear).OnComplete(()=> {
                canFollow = true;
            });
        }
        private void OnDestroy()
        {
            GameManager.ShowAdNow -= GameManager_ShowAdNow;
            nativeAdHolder.Event_AdFailed -= NativeAdHolder_Event_AdFailed;  //Unsubscribe to native ad loaded event
            nativeAdHolder.Event_AdLoaded -= NativeAdHolder_Event_AdLoaded;  //Unsubscribe to native ad failed event
            GameManager.HideAdNow -= GameManager_HideAdNow;
        }

        private void NativeAdHolder_Event_AdFailed(object arg1, GoogleMobileAds.Api.AdFailedToLoadEventArgs arg2)
        {
            if (gotFollowRequest)
                canFollow = false;
        }
        private void NativeAdHolder_Event_AdLoaded(object arg1, GoogleMobileAds.Api.NativeAdEventArgs arg2)
        {
            adLoaded = true;
            if (gotFollowRequest)
                canFollow = true;
        }
        private void LateUpdate()
        {
            if (!canFollow)
                return;
            if (targetTransform != null)
            {
                Vector3 targetPosition = targetTransform.position + offset;
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime); //Follows the target with given offset
            }
        }
    }
}