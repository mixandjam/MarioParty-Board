using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;

public class StarSpace : SpaceEvent
{
    private CameraHandler cameraHandler;
    private TurnUI turnUI;
    private PlayableDirector starTimelineDirector;
    private SplineKnotAnimate currentSplineKnotAnimator;


    [SerializeField] private int starCost = 20;
    [SerializeField] private Transform starTransform;

    private void Start()
    {
        cameraHandler = FindAnyObjectByType<CameraHandler>();
        turnUI = FindAnyObjectByType<TurnUI>();
        starTimelineDirector = FindAnyObjectByType<PlayableDirector>();

        turnUI.StarButton.onClick.AddListener(OnStarBuyClick);
        turnUI.CancelStarButton.onClick.AddListener(OnStarCancel);

    }

    private void OnStarCancel()
    {
        currentSplineKnotAnimator.Paused = false;
        FocusOnStar(false);
        turnUI.FadeRollText(false);

    }

    private void OnStarBuyClick()
    {
        //Get Current Player, this would need to be adjusted with multiplayer
        PlayerStats playerStats = FindAnyObjectByType<PlayerStats>();
        if (playerStats.Coins < starCost)
            return;


        StartCoroutine(StarSequence());

        IEnumerator StarSequence()
        {
            playerStats.AddCoins(-starCost);
            playerStats.UpdateStats();

            cameraHandler.ZoomCamera(false);
            turnUI.ShowStarPurchaseUI(false);
            starTransform.DOScale(0, .1f);
            starTimelineDirector.Play();
            yield return new WaitUntil(() => starTimelineDirector.state == PlayState.Paused);

            playerStats.AddStars(1);
            playerStats.UpdateStats();

            FocusOnStar(false);
            starTransform.DOScale(1, .5f).SetEase(Ease.OutBack);
            currentSplineKnotAnimator.Paused = false;
            turnUI.FadeRollText(false);
        }
    }

    public override void StartEvent(SplineKnotAnimate animator)
    {
        base.StartEvent(animator);
        currentSplineKnotAnimator = animator;
        FocusOnStar(true);
        turnUI.FadeRollText(true);
    }

    public void FocusOnStar(bool focus)
    {
        FindAnyObjectByType<CameraHandler>().ZoomCamera(focus);
        turnUI.ShowStarPurchaseUI(focus);
        if (focus)
            currentSplineKnotAnimator.transform.GetChild(0).DOLookAt(starTransform.position, .5f, AxisConstraint.Y);
        else
            currentSplineKnotAnimator.transform.GetChild(0).DOLocalRotate(Vector3.zero, .3f);
    }
}
