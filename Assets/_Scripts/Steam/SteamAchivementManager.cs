// SteamAchievementManager.cs
// Requires Steamworks.NET. Add scripting define symbol: STEAMWORKS
// Drop this on a bootstrap GameObject. It persists across scenes.

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

public class SteamAchievementManager : MonoBehaviour
{
    public static SteamAchievementManager Instance { get; private set; }

    [Header("Config")]
    [Tooltip("List your achievement API names here for quick debug and full reset.")]
    [SerializeField] private List<string> knownAchievementIds = new List<string>();

    [Tooltip("Show a simple in-game debug UI for testing. Toggle at runtime with ToggleDebugUI().")]
    [SerializeField] private bool showDebugUI = false;

    [Tooltip("If true, Unlock calls will be ignored. Useful for playtesting without polluting stats.")]
    [SerializeField] private bool testMode = false;

    [Tooltip("Optional key to toggle the debug UI at runtime. Set to None to disable.")]
    [SerializeField] private KeyCode debugToggleKey = KeyCode.BackQuote;

    [Tooltip("How often to attempt replaying the offline queue, in seconds.")]
    [SerializeField] private float replayIntervalSeconds = 1.5f;

    public bool IsInitialized { get; private set; }

    // Static facade
    public static void Unlock(string achievementId)  => Instance?.UnlockAchievement(achievementId);
    public static void Clear(string achievementId)   => Instance?.ClearAchievement(achievementId);
    public static bool IsUnlocked(string achievementId) => Instance != null && Instance.GetAchievementState(achievementId, out var ok) && ok;
    public static void AddStat(string statName, int amount) => Instance?.AddStatInt(statName, amount);
    public static void SetStat(string statName, int value)  => Instance?.SetStatInt(statName, value);
    public static void ResetAll(bool includeStats)          => Instance?.ResetAllAchievements(includeStats);
    public static void Store()                              => Instance?.StoreStats();
    public void ToggleDebugUI() => showDebugUI = !showDebugUI;

    // Caches and timers
    private readonly HashSet<string> unlockedCache = new HashSet<string>(StringComparer.Ordinal);
    private float nextReplayTime;

    // Offline queue persistence
    [Serializable]
    private enum ActionType { Unlock, Clear, AddStatInt, SetStatInt }

    [Serializable]
    private class OfflineAction
    {
        public ActionType type;
        public string id;        // achievement id or unused
        public string statName;  // for stats
        public int value;        // amount or target
    }

    [Serializable]
    private class OfflineActionList { public List<OfflineAction> items = new List<OfflineAction>(); }

    private readonly Queue<OfflineAction> pending = new Queue<OfflineAction>();
    private string queuePath;

#if !DISABLESTEAMWORKS
    private Callback<UserStatsReceived_t> cbUserStatsReceived;
    private Callback<UserStatsStored_t> cbUserStatsStored;
    private Callback<UserAchievementStored_t> cbUserAchievementStored;
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        queuePath = Path.Combine(Application.persistentDataPath, "steam_ach_queue.json");
        LoadQueueFromDisk();
    }

    private void Start()
    {
        TryInitialize();
    }

    private void Update()
    {
        if (debugToggleKey != KeyCode.None && Input.GetKeyDown(debugToggleKey))
            ToggleDebugUI();

        // Attempt to replay queued actions at an interval once initialized
        if (IsInitialized && Time.unscaledTime >= nextReplayTime)
        {
            TryReplayQueue();
            nextReplayTime = Time.unscaledTime + replayIntervalSeconds;
        }
    }

    private void OnApplicationQuit()
    {
        SaveQueueToDisk();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        SaveQueueToDisk();
    }

#if !DISABLESTEAMWORKS
    private void TryInitialize()
    {
        try
        {
            cbUserStatsReceived     = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
            cbUserStatsStored       = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
            cbUserAchievementStored = Callback<UserAchievementStored_t>.Create(OnUserAchievementStored);

            bool requested = SteamUserStats.RequestCurrentStats();
            Debug.Log($"[SteamAchievementManager] Requested current stats: {requested}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SteamAchievementManager] Steam not ready. Exception: {e.Message}");
        }
    }

    private void OnUserStatsReceived(UserStatsReceived_t p)
    {
        try
        {
            if ((CGameID)p.m_nGameID == new CGameID(SteamUtils.GetAppID()) && p.m_eResult == EResult.k_EResultOK)
            {
                IsInitialized = true;
                BuildUnlockedCache();
                Debug.Log("[SteamAchievementManager] Stats received. Ready.");
                // Kick an immediate replay try
                TryReplayQueue();
            }
            else
            {
                Debug.LogWarning($"[SteamAchievementManager] Stats received with result {p.m_eResult} for game {p.m_nGameID}.");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SteamAchievementManager] OnUserStatsReceived exception: {e.Message}");
        }
    }

    private void OnUserStatsStored(UserStatsStored_t p)
    {
        Debug.Log($"[SteamAchievementManager] Stats stored. Result: {p.m_eResult}");
    }

    private void OnUserAchievementStored(UserAchievementStored_t p)
    {
        Debug.Log($"[SteamAchievementManager] Achievement stored: {p.m_rgchAchievementName}. CurProgress: {p.m_nCurProgress}/{p.m_nMaxProgress}");
    }

    private void BuildUnlockedCache()
    {
        unlockedCache.Clear();
        foreach (var id in knownAchievementIds)
        {
            if (GetAchievementState(id, out var achieved) && achieved)
                unlockedCache.Add(id);
        }
    }
#else
    private void TryInitialize()
    {
        Debug.Log("[SteamAchievementManager] STEAMWORKS not defined. Running in simulate mode. Queue will persist but never flush.");
        IsInitialized = false;
    }
#endif

    // Public API
    public void UnlockAchievement(string achievementId)
    {
        if (string.IsNullOrWhiteSpace(achievementId)) return;
        if (testMode)
        {
            Debug.Log($"[SteamAchievementManager] Test mode. Skip unlock: {achievementId}");
            return;
        }

#if !DISABLESTEAMWORKS
        if (IsInitialized)
        {
            if (unlockedCache.Contains(achievementId))
            {
                Debug.Log($"[SteamAchievementManager] Already unlocked: {achievementId}");
                return;
            }
            if (InternalUnlock(achievementId))
            {
                unlockedCache.Add(achievementId);
                return;
            }
            // If a live attempt fails, fall through to queue
        }
#endif
        EnqueueAndPersist(new OfflineAction { type = ActionType.Unlock, id = achievementId });
    }

    public void ClearAchievement(string achievementId)
    {
        if (string.IsNullOrWhiteSpace(achievementId)) return;

#if !DISABLESTEAMWORKS
        if (IsInitialized)
        {
            if (InternalClear(achievementId))
            {
                unlockedCache.Remove(achievementId);
                return;
            }
        }
#endif
        EnqueueAndPersist(new OfflineAction { type = ActionType.Clear, id = achievementId });
    }

    public bool GetAchievementState(string achievementId, out bool achieved)
    {
        achieved = false;
#if !DISABLESTEAMWORKS
        if (string.IsNullOrWhiteSpace(achievementId)) return false;
        bool ok = SteamUserStats.GetAchievement(achievementId, out achieved);
        if (!ok)
            Debug.LogWarning($"[SteamAchievementManager] GetAchievement failed for {achievementId}");
        return ok;
#else
        return false;
#endif
    }

    public void ResetAllAchievements(bool includeStats)
    {
#if !DISABLESTEAMWORKS
        bool ok = SteamUserStats.ResetAllStats(includeStats);
        bool stored = SteamUserStats.StoreStats();
        unlockedCache.Clear();
        Debug.Log($"[SteamAchievementManager] ResetAllAchievements(includeStats: {includeStats}). Reset ok: {ok}. Store ok: {stored}");
#else
        Debug.Log("[SteamAchievementManager] STEAMWORKS not defined. Simulate ResetAllAchievements.");
#endif
    }

    public void AddStatInt(string statName, int amount)
    {
        if (string.IsNullOrWhiteSpace(statName)) return;

#if !DISABLESTEAMWORKS
        if (IsInitialized)
        {
            if (InternalAddStatInt(statName, amount))
                return;
        }
#endif
        EnqueueAndPersist(new OfflineAction { type = ActionType.AddStatInt, statName = statName, value = amount });
    }

    public void SetStatInt(string statName, int value)
    {
        if (string.IsNullOrWhiteSpace(statName)) return;

#if !DISABLESTEAMWORKS
        if (IsInitialized)
        {
            if (InternalSetStatInt(statName, value))
                return;
        }
#endif
        EnqueueAndPersist(new OfflineAction { type = ActionType.SetStatInt, statName = statName, value = value });
    }

    public void StoreStats()
    {
#if !DISABLESTEAMWORKS
        if (!IsInitialized) return;
        SteamUserStats.StoreStats();
#endif
    }

    // Internal Steam calls return success so caller knows to queue or not
#if !DISABLESTEAMWORKS
    private bool InternalUnlock(string achievementId)
    {
        bool ok = SteamUserStats.SetAchievement(achievementId);
        if (!ok)
        {
            Debug.LogWarning($"[SteamAchievementManager] SetAchievement failed for {achievementId}");
            return false;
        }
        bool stored = SteamUserStats.StoreStats();
        if (!stored)
        {
            Debug.LogWarning($"[SteamAchievementManager] StoreStats failed after unlocking {achievementId}");
            return false;
        }
        Debug.Log($"[SteamAchievementManager] Unlocked: {achievementId}");
        return true;
    }

    private bool InternalClear(string achievementId)
    {
        bool ok = SteamUserStats.ClearAchievement(achievementId);
        bool stored = SteamUserStats.StoreStats();
        Debug.Log($"[SteamAchievementManager] Clear {achievementId}. Clear ok: {ok}. Store ok: {stored}");
        return ok && stored;
    }

    private bool InternalAddStatInt(string statName, int amount)
    {
        bool got = SteamUserStats.GetStat(statName, out int current);
        if (!got)
        {
            Debug.LogWarning($"[SteamAchievementManager] GetStat failed for {statName}");
            return false;
        }
        int next = current + amount;
        bool set = SteamUserStats.SetStat(statName, next);
        bool stored = SteamUserStats.StoreStats();
        Debug.Log($"[SteamAchievementManager] AddStat {statName}: {current} -> {next}. Set ok: {set}, Store ok: {stored}");
        return set && stored;
    }

    private bool InternalSetStatInt(string statName, int value)
    {
        bool set = SteamUserStats.SetStat(statName, value);
        bool stored = SteamUserStats.StoreStats();
        Debug.Log($"[SteamAchievementManager] SetStat {statName} = {value}. Set ok: {set}, Store ok: {stored}");
        return set && stored;
    }
#endif

    // Offline queue helpers
    private void EnqueueAndPersist(OfflineAction action)
    {
        pending.Enqueue(action);
        SaveQueueToDisk();
        Debug.Log($"[SteamAchievementManager] Queued offline action: {action.type} {(action.id ?? action.statName)}");
    }

    private void TryReplayQueue()
    {
#if !DISABLESTEAMWORKS
        if (!IsInitialized) return;
        if (pending.Count == 0) return;

        bool anyFailedThisPass = false;
        int safety = 1024; // avoid infinite loops if things go weird
        int countThisPass = Mathf.Min(pending.Count, safety);

        var tempList = new List<OfflineAction>(countThisPass);
        while (countThisPass-- > 0 && pending.Count > 0)
            tempList.Add(pending.Dequeue());

        foreach (var a in tempList)
        {
            bool ok = false;
            switch (a.type)
            {
                case ActionType.Unlock:
                    if (unlockedCache.Contains(a.id)) ok = true;
                    else
                    {
                        ok = InternalUnlock(a.id);
                        if (ok) unlockedCache.Add(a.id);
                    }
                    break;

                case ActionType.Clear:
                    ok = InternalClear(a.id);
                    if (ok) unlockedCache.Remove(a.id);
                    break;

                case ActionType.AddStatInt:
                    ok = InternalAddStatInt(a.statName, a.value);
                    break;

                case ActionType.SetStatInt:
                    ok = InternalSetStatInt(a.statName, a.value);
                    break;
            }

            if (!ok)
            {
                // Requeue on failure and mark that we should pause until the next interval
                pending.Enqueue(a);
                anyFailedThisPass = true;
            }
        }

        if (anyFailedThisPass)
            Debug.Log("[SteamAchievementManager] Some offline actions could not be applied. Will retry later.");

        SaveQueueToDisk();
#endif
    }

    private void SaveQueueToDisk()
    {
        try
        {
            var wrapper = new OfflineActionList();
            wrapper.items.AddRange(pending);

            string json = JsonUtility.ToJson(wrapper, true);
            File.WriteAllText(queuePath, json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SteamAchievementManager] Failed to save queue. {e.Message}");
        }
    }

    private void LoadQueueFromDisk()
    {
        try
        {
            if (!File.Exists(queuePath)) return;
            string json = File.ReadAllText(queuePath);
            var wrapper = JsonUtility.FromJson<OfflineActionList>(json);
            if (wrapper?.items != null)
            {
                foreach (var a in wrapper.items)
                    pending.Enqueue(a);
            }
            Debug.Log($"[SteamAchievementManager] Loaded offline queue. Count {pending.Count}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SteamAchievementManager] Failed to load queue. {e.Message}");
        }
    }

    // Debug helpers
    [ContextMenu("Debug/Unlock First Known")]
    private void Debug_UnlockFirstKnown()
    {
        if (knownAchievementIds.Count > 0)
            UnlockAchievement(knownAchievementIds[0]);
        else
            Debug.LogWarning("[SteamAchievementManager] No known achievements listed.");
    }

    [ContextMenu("Debug/Clear First Known")]
    private void Debug_ClearFirstKnown()
    {
        if (knownAchievementIds.Count > 0)
            ClearAchievement(knownAchievementIds[0]);
        else
            Debug.LogWarning("[SteamAchievementManager] No known achievements listed.");
    }

    [ContextMenu("Debug/Reset All (keep stats=false)")]
    private void Debug_ResetAll_NoStats() => ResetAllAchievements(false);

    [ContextMenu("Debug/Reset All (keep stats=true)")]
    private void Debug_ResetAll_KeepStats() => ResetAllAchievements(true);

    private void OnGUI()
    {
        if (!showDebugUI) return;

        const int width = 380;
        GUILayout.BeginArea(new Rect(10, 10, width, Screen.height - 20), GUI.skin.window);
        GUILayout.Label("Steam Achievement Debug");
        GUILayout.Label(IsInitialized ? "Initialized: yes" : "Initialized: no");
        GUILayout.Label($"Test Mode: {(testMode ? "on" : "off")}");
        GUILayout.Label($"Queued Offline Actions: {pending.Count}");

        if (GUILayout.Button("Toggle Test Mode")) testMode = !testMode;

        GUILayout.Space(6);
        if (GUILayout.Button("Reset All (keep stats=false)")) ResetAllAchievements(false);
        if (GUILayout.Button("Reset All (keep stats=true)")) ResetAllAchievements(true);

        GUILayout.Space(6);
        GUILayout.Label("Known Achievements:");
        foreach (var id in knownAchievementIds)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(id, GUILayout.Width(220));
            if (GUILayout.Button("Unlock", GUILayout.Width(60))) UnlockAchievement(id);
            if (GUILayout.Button("Clear", GUILayout.Width(60)))   ClearAchievement(id);
            GUILayout.EndHorizontal();
        }

        GUILayout.EndArea();
    }
}
