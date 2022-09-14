using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
    public int score = 0;
    public int lives = 5;
    public int highScore;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI[] highScoreText;

    public TextMeshProUGUI LivesText;


    public GameObject LoseScreen;
    public GameObject MainMenu;
    public GameObject GameScreen;



    public bool doublePoints = false;
    
    
    private void Start()
    {
        lives = 5;
        score = 0;
        highScore = PlayerPrefs.GetInt("highScore");
        foreach (var t in highScoreText)
        {
            t.text = "HighScore: " + highScore;
        }

        Time.timeScale = 0;

    }

    public void GainPoints()
    {
        if (doublePoints)
        {
            score += 2;
        }
        else
        {
            score += 1;
        }
    }

    private void Update()
    {
        scoreText.text = "Score: " + score;
        LivesText.text = "Lives: " + lives;

        if (score > highScore)
        {
            highScore = score;
            foreach (var t in highScoreText)
            {
                t.text = "HighScore: " + highScore;
            }
            //highScoreText.text = "HighScore: " + highScore;
            PlayerPrefs.SetInt("highScore", highScore);

        }

        if (lives <= 0)
        {
            LoseScreen.SetActive(true);
            Time.timeScale = 0;
        }

    }

    public void ResetLevel()
    {
        Time.timeScale = 1;

        SceneManager.LoadScene(0);
    }
    public void PlayLevel()
    {
        Time.timeScale = 1;

        MainMenu.SetActive(false);
        GameScreen.SetActive(true);
    }
    
    
}
