using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioClip MenuMusic;
    [SerializeField] private AudioClip AdventureMusic;
    [SerializeField] private AudioClip StandardBattleMusic;
    [SerializeField] private AudioClip EliteBattleMusic;
    [SerializeField] private AudioClip DragonBattleMusic;
    [SerializeField] private AudioClip DeathMusic;
    [SerializeField] private AudioClip ShopMusic;
    
    public static MusicManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMenuMusic()
    {
        SoundManager.Instance.PlayMusic(MenuMusic, 1);
    }
    public void PlayAdventureMusic()
    {
        SoundManager.Instance.PlayMusic(AdventureMusic, 1);
    }
    public void PlayBattleMusic()
    {
        SoundManager.Instance.PlayMusic(StandardBattleMusic, 1);
    }
    public void PlayEliteMusic()
    {
        SoundManager.Instance.PlayMusic(EliteBattleMusic, 1);
    }
    public void PlayDragonMusic()
    {
        SoundManager.Instance.PlayMusic(DragonBattleMusic, 1);
    }
    public void PlayShopMusic()
    {
        SoundManager.Instance.PlayMusic(ShopMusic, 1);
    }
    public void PlayDeathMusic()
    {
        SoundManager.Instance.PlayMusic(DeathMusic, 1);
    }
    
    
    
}
