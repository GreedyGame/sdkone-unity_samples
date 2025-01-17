
using UnityEngine;
using TMPro;
using PubScale.OfferWall;
using System;
using PubScale.SdkOne.NativeAds.Hightower;

public class HighTowerOfferwall : MonoBehaviour
{


    [SerializeField] private string appKey = "3f2a3eda-b6ab-4790-a85e-e51a14717fc3";
    [SerializeField] private PopUp popUp;
    [SerializeField] private bool autoInit = true;
    [SerializeField] private GameObject loadingUI;
    [SerializeField] private GameObject offerUI;
    [SerializeField] private GameObject failedUI;
    [SerializeField] private int rewardAmount = 0;
    [SerializeField] private TextMeshProUGUI rewardAmountText;
    [SerializeField] private Sprite OfferWallBackgroundImage;
    [SerializeField] private bool isFullScreen = false;
    [SerializeField] private bool devModeEnabled = false;
    private string uniqueID = "";

    private void Awake()
    {
        if (autoInit)
            Init();
        rewardAmount = PlayerPrefs.GetInt(PrefsHelper.Key_offWallRewardAmount, 0);
        rewardAmountText.text = rewardAmount.ToString();
    }
    void UpdateRewardAmount(int amount)
    {
        rewardAmount = rewardAmount + amount;
        rewardAmountText.text = rewardAmount.ToString();
        PlayerPrefs.SetInt(PrefsHelper.Key_offWallRewardAmount, rewardAmount);
    }
    public void Init()
    {
        string resourceFileName = "";
        if (OfferWallBackgroundImage != null)
        {
            resourceFileName = OfferWallBackgroundImage.name;
        }
#if !UNITY_ANDROID || UNITY_EDITOR
        return;
#endif
#pragma warning disable CS0162 // Unreachable code detected
        loadingUI.SetActive(true);
#pragma warning restore CS0162 // Unreachable code detected
        failedUI.SetActive(false);
        offerUI.SetActive(false);
        Debug.Log("Offer Wall Init");
        uniqueID = SystemInfo.deviceUniqueIdentifier;
        Debug.Log("saved user Id is " + PlayerPrefs.GetString("userId", ""));
        if (string.IsNullOrEmpty(uniqueID))
            uniqueID = PlayerPrefs.GetString("userId", "");
        if (string.IsNullOrEmpty(uniqueID))
        {
            Debug.Log("Offer Wall User ID is null");
            return;
        }
        Debug.Log("Offer Wall Unique ID: " + uniqueID); ;
        OfferWallManager.OnOfferClaimed += OfferWallManager_OnOfferClaimed;
        OfferWallManager.OnInitialize += OfferWallManager_OnInitialize;
        OfferWallManager.OnInitializeFailed += OfferWallManager_OnInitializeFailed;
        OfferWallManager.OnOfferWallClosed += OfferWallManager_OnOfferWallClosed;

        OfferWallManager.InitializeOfferWall(appKey, uniqueID, resourceFileName, isFullScreen, devModeEnabled);
    }

    bool offerwallClosedEvent = false;
    private void OfferWallManager_OnOfferWallClosed()
    {
        Debug.LogWarning("OfferwallManager - ON OFFERWALL CLOSED");
        offerwallClosedEvent = true;
    }

    void Update()
    {
        if (offerwallClosedEvent)
        {
            offerwallClosedEvent = false;
            GameManager.instance.OnOfferWallClosed();
        }

    }

    private void OfferWallManager_OnInitializeFailed(string obj)
    {
        loadingUI.SetActive(false);
        failedUI.SetActive(true);
        offerUI.SetActive(false);
    }

    private void OfferWallManager_OnInitialize()
    {
        loadingUI.SetActive(false);
        failedUI.SetActive(false);
        offerUI.SetActive(true);
    }

    private void OfferWallManager_OnOfferClaimed(float earnedAmount)
    {
        Debug.Log("Offer Wall Claimed " + (int)earnedAmount);
        UpdateRewardAmount((int)earnedAmount);
        popUp.ShowCompleteState(earnedAmount.ToString());
    }

    private void OnDestroy()
    {
        OfferWallManager.OnOfferClaimed -= OfferWallManager_OnOfferClaimed;
        OfferWallManager.OnInitialize -= OfferWallManager_OnInitialize;
        OfferWallManager.OnInitializeFailed -= OfferWallManager_OnInitializeFailed;
    }

}
