using System.Collections;
using System;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class NetworkHippoController : NetworkBehaviour
{
    private const string BALL_TAG = "Ball";

    [SerializeField] SpriteRenderer SpriteRenderer;

    [SerializeField] Sprite closeMouthSprite;
    [SerializeField] Sprite openMouthSprite;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip openMouthSFX;
    [SerializeField] AudioClip closeMouthSFX;
    [SerializeField] AudioClip munchSFX;

    [SerializeField] float animationDuration = 1f;
    [SerializeField] float lungeForwardDistance = 5f;

    private Coroutine openMouth_CRT;
    private Coroutine powerUp_CRT;

    private Tween lungeTween;
    private Tween lungeScaleTween;

    private bool mouthIsOpen;

    public event Action<int> OnBallCollected;
    public event Action<int> OnBallCountReset;

    private int ballsCollected;

    private Vector3 startPos;
    private Vector3 startScale;

    private void Start()
    {
        startPos = transform.localPosition;
        startScale = transform.localScale;

        if (NetworkGameManager.Instance)
        {
            NetworkGameManager.Instance.OnRoundEnded += ResetCollectedBallCount;
            NetworkGameManager.Instance.OnGameRestarted += ResetCollectedBallCount;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
            transform.tag = "Player";
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Space)) 
            StartBiteServerRPC();
    }

    [Rpc(SendTo.Server)]
    private void StartBiteServerRPC() 
    {
        TriggerBiteClientRPC();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerBiteClientRPC()
    {
        if (openMouth_CRT != null) 
        {
            lungeTween.Kill();
            lungeScaleTween.Kill();
            StopCoroutine(openMouth_CRT);
            openMouth_CRT = null;
        }
        openMouth_CRT = StartCoroutine(OpenMouth());
    }

    private IEnumerator OpenMouth() 
    {
        transform.localPosition = startPos;
        transform.localScale = startScale;
        mouthIsOpen = true;
        SpriteRenderer.sprite = openMouthSprite;
        lungeTween = transform.DOPunchPosition(transform.up * lungeForwardDistance, animationDuration, 1).SetEase(Ease.OutElastic);
        lungeScaleTween = transform.DOPunchScale(transform.localScale * 0.1f, animationDuration, 1);
        yield return new WaitForSeconds(animationDuration * 0.5f);
        CloseMouth();
        yield return new WaitForSeconds(animationDuration * 0.5f);
        openMouth_CRT = null;
    }

    private void CloseMouth()
    {
        mouthIsOpen = false;
        SpriteRenderer.sprite = closeMouthSprite;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer) return;

        if (mouthIsOpen) 
        {
            if (collision.transform.CompareTag(BALL_TAG))
            {
                TriggerMouthFlagShutRPC();

                NetworkObject ballNO = collision.gameObject.GetComponent<NetworkObject>();
                ballNO.Despawn();

                TriggerBallCollectedClientRPC();
                NetworkGameManager.Instance.PlayerCollectedBallServerRPC(OwnerClientId);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerMouthFlagShutRPC() 
    {
        mouthIsOpen = false;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerBallCollectedClientRPC()
    {
        ballsCollected++;
        OnBallCollected?.Invoke(ballsCollected);
        PlayMunchSFX();
    }

    private void ResetCollectedBallCount()
    {
        ballsCollected = 0;
        OnBallCountReset?.Invoke(ballsCollected);
    }


    private void PlayMunchSFX() 
    {
        audioSource.clip = munchSFX;
        audioSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
        audioSource.Play();
    }

    private void PlayMouthSFX() 
    {
        audioSource.clip = mouthIsOpen ? openMouthSFX : closeMouthSFX;
        audioSource.Play();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerDoubleReachPowerupRPC()
    {
        powerUp_CRT = StartCoroutine(IncreaseLungeReach());
    }

    private IEnumerator IncreaseLungeReach(float multiplier = 2f) 
    {
        float startLungeReach = lungeForwardDistance;
        lungeForwardDistance *= multiplier;
        yield return new WaitForSeconds(8f);
        lungeForwardDistance = startLungeReach;
        powerUp_CRT = null;
    }

    [ContextMenu("Test double lunge")]
    private void LungeTest() 
    {
        powerUp_CRT = StartCoroutine(IncreaseLungeReach());
    }
}
