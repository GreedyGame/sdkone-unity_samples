using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using PubScale.SdkOne.NativeAds;
using System;
using PubScale.Common;
public class LandscapeAdAnimationVariant : MonoBehaviour
{
    public Transform pilot;
    public List<Sprite> images;  // Array to hold your big images
    // public Transform[] slides;
    public Transform AdIcon = null;
    public Transform AdDetails = null;
    public Transform buttonCta = null;
    public TextMeshProUGUI ctaText = null;
    string ctaString = "";

    private DynamicADFormatHandler dynamicADFormatHandler = null;
    string targetCtaString = "";
    public Image imageDisplay;
    public Image imageIn;    // Reference to the Image component for the incoming image
    public Image imageOut; 
    private float moveDistance = 800;
    private float slideDuration = 2;
    public float yOffset;
    NativeAdHolder nativeAdHolder;

    private void Awake() 
    {
        dynamicADFormatHandler = GetComponent<DynamicADFormatHandler>();
        targetCtaString = dynamicADFormatHandler?.adCallToActionTxt.text;

        
    }

    private void OnEnable() 
    {
        InitState();
        StartCoroutine(Start());
    }
    IEnumerator Start()
    {
        GetBigImageSprites();
        yield return new WaitForSeconds(.1f);
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(AdIcon.DOScale(1.5f, .2f));
        mySequence.Append(AdIcon.DOPunchScale(Vector3.one * .1f,.5f,0,0));
        mySequence.Insert(1,AdIcon.DOScale(1, .3f));
        mySequence.Insert(1,AdIcon.DOLocalMove(new Vector2(-270,80),.3f));
        mySequence.Append(AdDetails.DOScale(1,0.3f));
        mySequence.Append(AdDetails.DOPunchScale(Vector3.one * .05f,.5f,0,0));
        mySequence.Append(buttonCta.DOScale(1,0.3f));
        mySequence.Append(DOTween.To(()=> ctaString, x=>ctaString = x,targetCtaString, .5f).SetEase(Ease.InSine)).OnUpdate(()=>
        {
            ctaText.text = ctaString;
        });
        mySequence.InsertCallback(3,MyCallback);
    }

    private IEnumerator MyNewMethod()
    {
        int currentIndex = 0;
        int count = 0;
        yield return new WaitForSeconds(slideDuration); 
        pilot.DOLocalMoveX(-moveDistance,slideDuration);    
            // Initialize the first image position to the left of the screen

        imageIn.rectTransform.anchoredPosition = new Vector2(moveDistance, yOffset);
        imageIn.sprite = images[currentIndex];
        imageIn.rectTransform.DOAnchorPosX(0f, slideDuration);
        yield return new WaitForSeconds(slideDuration + 1);
        pilot.SetLocalPositionAndRotation(new Vector2(moveDistance, yOffset),pilot.rotation);
        while (count < images.Count - 1)
        {
            // Set the outgoing image to the current image
            imageOut.sprite = images[currentIndex];
            
            // Move out to the left
            imageOut.rectTransform.anchoredPosition = new Vector2(0, yOffset);
            imageOut.rectTransform.DOAnchorPosX(-moveDistance, slideDuration);

            // Prepare the next index
            int nextIndex = (currentIndex + 1) % images.Count;

            // Move in from the right
            imageIn.sprite = images[nextIndex];
            imageIn.rectTransform.anchoredPosition = new Vector2(moveDistance, yOffset);
            imageIn.rectTransform.DOAnchorPosX(0f, slideDuration);

            yield return new WaitForSeconds(slideDuration);

            currentIndex = nextIndex;
            count++;
        }
        // pilot.DOLocalMoveX(0,1,true);  
        yield return new WaitForSeconds(slideDuration);
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(imageIn.rectTransform.DOAnchorPosX(-moveDistance, slideDuration));
        mySequence.Insert(0,pilot.DOLocalMoveX(0,slideDuration));
        mySequence.InsertCallback(3,MyCallback);
    }

    private void MyCallback()
    {
        StartCoroutine(MyNewMethod());
    }

    private void InitState()
    {
        pilot.SetLocalPositionAndRotation(new Vector2(0, yOffset),pilot.rotation);
        AdIcon.localScale = Vector3.zero;
        AdIcon.SetLocalPositionAndRotation(new Vector2(0, yOffset),pilot.rotation);
        buttonCta.localScale = Vector3.zero;
        AdDetails.localScale = Vector3.zero;
        imageIn.rectTransform.anchoredPosition = new Vector2(moveDistance, yOffset);
        imageOut.rectTransform.anchoredPosition = new Vector2(moveDistance, yOffset);
        ctaText.text = "";
        ctaString = "";
    }
    private List<Texture2D> bigImageTextures = new();
    private void GetBigImageSprites()
    {
        bigImageTextures = GetComponentInParent<NativeAdHolder>().adDisplay.BigMediaImages;
        Debug.Log("Number of big image textures : " + bigImageTextures.Count);
        // if(bigImageTextures == null || bigImageTextures.Count <= 0) return;

        images.Clear();
        foreach (Texture2D texture in bigImageTextures)
        {
            ConvertTextureToSprite(texture, (sprite) =>
            {
                for (int i = 0; i < bigImageTextures.Count; i++)
                {
                    images.Add(sprite);
                    Debug.Log("image name : " + images[i].name);
                }
            });
        }

    }
    void ConvertTextureToSprite(Texture2D texture, Action<Sprite> OnSpriteConverted)
    {
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100, 0, meshType: SpriteMeshType.FullRect);
        OnSpriteConverted?.Invoke(sprite);
    }
}
