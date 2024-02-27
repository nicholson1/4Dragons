﻿using System.Linq;
using UnityEngine;
//using Newtonsoft.Json;

namespace Map
{
    public class MapManager : MonoBehaviour
    {
        public MapConfig config;
        public MapView view;

        public Map CurrentMap { get; private set; }

        public static MapManager _instance;
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

        private void Start()
        {
            // if (PlayerPrefs.HasKey("Map"))
            // {
            //     var mapJson = PlayerPrefs.GetString("Map");
            //     // map = JsonConvert.DeserializeObject<Map>(mapJson);
            //     // using this instead of .Contains()
            //     if (map.path.Any(p => p.Equals(map.GetBossNode().point)))
            //     {
            //         // payer has already reached the boss, generate a new map
            //         GenerateNewMap();
            //     }
            //     else
            //     {
            //         CurrentMap = map;
            //         // player has not reached the boss yet, load the current map
            //         view.ShowMap(map);
            //     }
            // }
            // else
            // {
            //     GenerateNewMap();
            // }
        }

        public void GenerateNewMap()
        {
            var map = MapGenerator.GetMap(config);
            CurrentMap = map;
            //Debug.Log(map.ToJson());
            view.ShowMap(map);
        }

        public void SaveMap()
        {
            return;
            // if (CurrentMap == null) return;
            //
            // var json = JsonConvert.SerializeObject(CurrentMap, Formatting.Indented,
            //     new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
            // PlayerPrefs.SetString("Map", json);
            // PlayerPrefs.Save();
        }

        private void OnApplicationQuit()
        {
            SaveMap();
        }
    }
}