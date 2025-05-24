using DG.Tweening;
using System.Collections;
using UnityEngine;
using System;
using Unity.Netcode;

public class HippoController : NetworkBehaviour
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
    private Tween lungeTween;

    private bool mouthIsOpen;

    public event Action<int> OnBallCollected;

    private int ballsCollected;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.localPosition;
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
        {
            StartBiteServerRPC();
        }
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
            StopCoroutine(openMouth_CRT);
            openMouth_CRT = null;
        }
        openMouth_CRT = StartCoroutine(OpenMouth());
    }

    private IEnumerator OpenMouth() 
    {
        transform.localPosition = startPos;
        mouthIsOpen = true;
        SpriteRenderer.sprite = openMouthSprite;
        lungeTween = transform.DOPunchPosition(transform.up * lungeForwardDistance, animationDuration, 1); 
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
        if (mouthIsOpen) 
        {
            if (collision.transform.CompareTag(BALL_TAG))
            {
                if (!IsServer) return;
                NetworkObject ballNO = collision.gameObject.GetComponent<NetworkObject>();
                ballNO.Despawn();
                HungryHippoGameManager.Instance.PlayerCollectedBallRPC();
                TriggerBallCollectedClientRPC();
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerBallCollectedClientRPC()
    {
        ballsCollected++;
        OnBallCollected?.Invoke(ballsCollected);
        PlayMunchSFX();
        Debug.Log("Collecting Ball : " + ballsCollected.ToString());
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
}
