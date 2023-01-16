using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundObserver : MonoBehaviour, Observer
{
    [SerializeField] private AudioClip PickupSound;
    [SerializeField] private AudioClip FireSound;
    [SerializeField] private AudioClip FreezeSound;
    [SerializeField] private AudioClip SpeedSound;
    [SerializeField] private AudioClip DoublePointsSound;

    [SerializeField] private AudioClip enemeyHit;
    [SerializeField] private AudioClip FreezeEnemy;
    [SerializeField] private AudioClip burnEnemy;

    [SerializeField] private AudioClip jump;
    
    
     [SerializeField]private AudioSource soundEffects;

            
    void Awake()
    {
        //soundEffects = GetComponent<AudioSource>();
        foreach (PickupObserved subject in FindObjectsOfType<PickupObserved>())
        {
            //Debug.Log(subject.name);

            subject.AddObserver(this);
        }
        foreach (ButtonBeingObserved subject in FindObjectsOfType<ButtonBeingObserved>())
        {
            subject.AddObserver(this);

        }
        //playJump();

    }

    void Start()
    {
        CharacterMovement.playerJumpEvent += playJump;
        Enemy.enemyFireDeath += playEnemyDeathFire;
        Enemy.hitPlayer += playPlayerHit;
        Enemy.enemyFreezeDeath += playEnemyDeath;
    }

    private void OnDestroy()
    {
        CharacterMovement.playerJumpEvent -= playJump;
        Enemy.enemyFireDeath -= playEnemyDeathFire;
        Enemy.hitPlayer -= playPlayerHit;
        Enemy.enemyFreezeDeath -= playEnemyDeath;
    }



    public void OnNotify(Collectible obj, NotificationType notificationType)
    {
        soundEffects.PlayOneShot(PickupSound, .5f);
    }

    public void OnNotifyUI(int i, NotificationType notificationType)
    {
        // not used
    }

    public void OnNotifyButtonClick(NotificationType notificationType)
    {
        switch (notificationType)
        {
            case NotificationType.FirePickup:
                soundEffects.PlayOneShot(FireSound, .25f);

                break;
            case NotificationType.FreezePickup :
                soundEffects.PlayOneShot(FreezeSound, .5f);

                break;
            case NotificationType.SpeedPickup:
                soundEffects.PlayOneShot(SpeedSound, .75f);

                break;
            case NotificationType.DoublePointsPickup:
                soundEffects.PlayOneShot(DoublePointsSound, .75f);

                break;
        }
    }

    public void playJump()
    {
        //Debug.Log("jump");
        soundEffects.PlayOneShot(jump, .50f);
    }
    
    public void playPlayerHit()
    {
        soundEffects.PlayOneShot(enemeyHit, 1f);
    }
    public void playEnemyDeath()
    {
        soundEffects.PlayOneShot(FreezeEnemy, 1f);
    }
    public void playEnemyDeathFire()
    {
        soundEffects.PlayOneShot(burnEnemy, 1f);
    }
}

