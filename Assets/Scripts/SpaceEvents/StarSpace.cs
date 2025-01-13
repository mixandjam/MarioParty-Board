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
    }

    private void OnStarBuyClick()
    {

        StartCoroutine(StarSequence());

        IEnumerator StarSequence()
        {
            cameraHandler.ZoomCamera(false);
            turnUI.ShowStarPurchaseUI(false);
            starTimelineDirector.Play();
            yield return new WaitUntil(() => starTimelineDirector.state == PlayState.Paused);
            FocusOnStar(false);
            currentSplineKnotAnimator.Paused = false;
        }
    }

    public override void StartEvent(SplineKnotAnimate animator)
    {
        base.StartEvent(animator);
        currentSplineKnotAnimator = animator;
        FocusOnStar(true);
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
