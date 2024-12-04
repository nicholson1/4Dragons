using System;
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
    
    [SerializeField] private AudioClip Ambiance1;
    [SerializeField] private AudioClip VictoryMusic;

    
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

    private void Start()
    {
        PlayAmbiance1();
    }

    public void PlayMenuMusic()
    {
        SoundManager.Instance.PlayMusic(MenuMusic, MusicChannel.MenuMusic,3);
    }
    public void PlayAdventureMusic()
    {
        SoundManager.Instance.PlayMusic(AdventureMusic,  MusicChannel.AdventureMusic,3);
    }
    public void PlayBattleMusic()
    {
        SoundManager.Instance.PlayMusic(StandardBattleMusic, MusicChannel.StandardBattleMusic, 3);
    }
    public void PlayEliteMusic()
    {
        SoundManager.Instance.PlayMusic(EliteBattleMusic,  MusicChannel.EliteBattleMusic, 3);
    }
    public void PlayDragonMusic()
    {
        SoundManager.Instance.PlayMusic(DragonBattleMusic,  MusicChannel.DragonBattleMusic, 3);
    }
    public void PlayShopMusic()
    {
        SoundManager.Instance.PlayMusic(ShopMusic,  MusicChannel.ShopMusic, 3);
    }
    public void PlayDeathMusic()
    {
        SoundManager.Instance.PlayMusic(DeathMusic,  MusicChannel.DeathMusic, 3);
    }
    
    public void PlayAmbiance1()
    {
        SoundManager.Instance.PlayAmbience(Ambiance1,  true, 3);
    }
    public void PlayVictoryMusic()
    {
        SoundManager.Instance.PlayMusic(VictoryMusic,  MusicChannel.VictoryMusic, 2);
    }
    
    
    
}
