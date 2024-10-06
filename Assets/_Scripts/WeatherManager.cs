using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager _instance;
    public int weatherLevel = 0;

    [SerializeField] private WindDirector _windDirector;
    
    //particle rate for each element at each level
    //wind speed at each level
    [SerializeField] private GameObject[] WeatherEffects;
    [SerializeField] private VisualEffect[] natureEffects;
    [SerializeField] private VisualEffect[] frostEffects;
    [SerializeField] private VisualEffect[] fireEffects;
    [SerializeField] private VisualEffect[] shadowEffects;
    [SerializeField] private VisualEffect[] bloodEffects;
    [SerializeField] private ParticleSystem bloodRain;

    [SerializeField] private Light primaryLight;
    [SerializeField] private Light secondaryLight;
    [SerializeField] private Color[] PrimaryLights;
    [SerializeField] private Color[] SecondaryLights;

    [SerializeField] private Material standardWater;
    [SerializeField] private Material shadowWater;
    [SerializeField] private Material bloodWater;
    [SerializeField] private MeshRenderer waterObj;

    private int[][] natureEffectsParticleRates = new int[][]
    {
        new int[] { 1, 2, 10}, // new trial start
        new int[] { 2, 2, 15},
        new int[] { 2, 4, 20},
        new int[] { 3, 4, 25},
        new int[] { 4, 8, 30},
        new int[] { 5, 16, 35},
        new int[] { 10, 32, 40},
        new int[] { 20, 60, 45},
        new int[] { 40, 70, 50},
        new int[] { 60, 100, 55}, // boss fight
    };
    private int[] natureEffectsWind = new[]
    {
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10
    };
    private int[][] frostEffectsParticleRates = new int[][]
    {
        new int[] { 1, 2, 10, 5}, // new trial start
        new int[] { 2, 2, 15, 15},
        new int[] { 2, 4, 20, 25},
        new int[] { 3, 4, 25, 35},
        new int[] { 4, 8, 30, 55},
        new int[] { 5, 16, 35, 75},
        new int[] { 10, 32, 40, 105},
        new int[] { 20, 60, 45, 135},
        new int[] { 40, 70, 50, 160},
        new int[] { 60, 100, 55, 200}, // boss fight
    };

    private int[] frostEffectsWind = new[]
    {
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10
    };
    private int[][] fireEffectsParticleRates = new int[][]
    {
        new int[] { 5, 5, 1, 1}, // new trial start
        new int[] { 10, 10, 1, 1},
        new int[] { 20, 20, 1, 1},
        new int[] { 40, 40, 1, 1},
        new int[] { 80, 80, 2, 2},
        new int[] { 100, 100, 2, 2},
        new int[] { 110, 110, 4, 4},
        new int[] { 130, 130, 7, 7},
        new int[] { 150, 150, 10, 10},
        new int[] { 200, 200, 50, 50}, // boss fight
    };

    private int[] fireEffectsWind = new[]
    {
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10
    };
    // shadow a little different
    private int shadowWind = 2;
    // make winddirector point down
    
    private int[][] shadowEffectsParticleRates = new int[][]
    {
        new int[] { 5, 1, 1}, // new trial start
        new int[] { 10, 1, 1},
        new int[] { 20, 1, 1},
        new int[] { 40, 1, 1},
        new int[] { 80, 2, 2},
        new int[] { 120, 2, 2},
        new int[] { 140, 3, 3},
        new int[] { 160, 5, 5},
        new int[] { 180, 10, 10},
        new int[] { 200, 50, 50}, // boss fight
    };


    //blood is a little differnt
    // make winddirector point up
    private int bloodWind = 5;
    
    private int[][] bloodEffectsParticleRates = new int[][]
    {
        new int[] { 2, 5, 1, 1}, // new trial start
        new int[] { 5, 10, 1, 1},
        new int[] { 10, 20, 1, 1},
        new int[] { 20, 40, 2, 2},
        new int[] { 30, 80, 2, 2},
        new int[] { 40, 100, 3, 3},
        new int[] { 65, 110, 5, 5},
        new int[] { 70, 130, 7, 7},
        new int[] { 80, 150, 10, 10},
        new int[] { 100, 200, 50, 50}, // boss fight
    };
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public void UpdateWeather(int level = 1, SpellSchool spellClass = SpellSchool.Blood)
    {
        weatherLevel = level;
        Debug.Log("Weather Level:"+level);
        //Debug.Log(level);
        //turn to make the wind director look at the player
        // activate the correct weather object
        // adjust the weather values
        // adjust the wind values

        primaryLight.color = PrimaryLights[(int)spellClass];
        secondaryLight.color = SecondaryLights[(int)spellClass];

        switch (spellClass)
        {
            case SpellSchool.Nature:
                for (int i = 0; i < natureEffects.Length; i++)
                {
                    if(i ==0)
                        natureEffects[i].SetFloat(Shader.PropertyToID("ParticlesRate"), natureEffectsParticleRates[level][i]);
                    else
                        natureEffects[i].SetFloat(Shader.PropertyToID("Particle Rate"), natureEffectsParticleRates[level][i]);
                }
                _windDirector.force = natureEffectsWind[level];
                waterObj.material = standardWater;
                _windDirector.transform.localRotation = Quaternion.Euler(0, 0, 180);


                break;
            case SpellSchool.Ice:
                for (int i = 0; i < frostEffects.Length; i++)
                {
                    if(i ==0)
                        frostEffects[i].SetFloat(Shader.PropertyToID("ParticlesRate"), frostEffectsParticleRates[level][i]);
                    else
                        frostEffects[i].SetFloat(Shader.PropertyToID("Particle Rate"), frostEffectsParticleRates[level][i]);
                }
                _windDirector.force = frostEffectsWind[level];
                waterObj.material = standardWater;
                _windDirector.transform.localRotation = Quaternion.Euler(0, 0, 180);


                break;
            case SpellSchool.Fire:
                for (int i = 0; i < fireEffects.Length; i++)
                {
                    fireEffects[i].SetFloat(Shader.PropertyToID("Particle Rate"), fireEffectsParticleRates[level][i]);
                }
                _windDirector.force = fireEffectsWind[level];
                waterObj.material = standardWater;
                _windDirector.transform.localRotation = Quaternion.Euler(0, 0, 180);



                break;
            case SpellSchool.Shadow:
                for (int i = 0; i < shadowEffects.Length; i++)
                {
                    shadowEffects[i].SetFloat(Shader.PropertyToID("Particle Rate"), shadowEffectsParticleRates[level][i]);
                }
                _windDirector.force = shadowWind;
                waterObj.material = shadowWater;
                _windDirector.transform.localRotation = Quaternion.Euler(0, 0, -90);



                break;
            case SpellSchool.Blood:
                bloodRain.gameObject.SetActive(true);
                var emission = bloodRain.emission;
                emission.rateOverTime =(float)bloodEffectsParticleRates[level][0];
                for (int i = 1; i < bloodEffects.Length; i++)
                {
                    bloodEffects[i].SetFloat(Shader.PropertyToID("Particle Rate"), bloodEffectsParticleRates[level][i]);
                }
                _windDirector.force = bloodWind;
                waterObj.material = bloodWater;
                _windDirector.transform.localRotation = Quaternion.Euler(0, 0, 90);


                break;
            default:
                Debug.LogWarning("INCORRECT SPELL SCHOOL FOR DRAGON");
                break;
        }
        
        //_windDirector.transform.LookAt(lookDirectionForWind);

        for (int i = 0; i < WeatherEffects.Length; i++)
        {
            if ((int)spellClass == i)
            {
                WeatherEffects[i].gameObject.SetActive(true);
            }
            else
            {
                WeatherEffects[i].gameObject.SetActive(false);
            }
        }
        _windDirector.AddVisualEffects();

    }
}
