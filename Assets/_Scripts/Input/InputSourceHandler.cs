using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;


public class InputSourceHandler : MonoBehaviour
{
    
    public event Action<bool> OnDeviceLost;
    public event Action<bool> OnDeviceRegained;
    public event Action<InputSource> OnDeviceChanged;

    public bool IsInitialized { get; private set; } = false;
    public bool GlobalInputDisabled => playerInput != null && !playerInput.enabled;
    public GameObject CachedLastSelectable => cachedLastSelectable;
    public InputSource CurrentInputSource => GetInputSource(currentControlScheme);

    private PlayerInput playerInput;

    private InputSystemUIInputModule inputModule;

    private GameObject cachedLastSelectable = null;

    private string currentControlScheme = string.Empty;

    private const string gamepadString = "Gamepad";
    private const string mouseKeyString = "Keyboard&Mouse";

    [SerializeField] private List<int> pairedDevices = new List<int>();

    public bool IsOnGamepad()
    {
        foreach (var device in playerInput.user.pairedDevices)
        {
            if (device is Gamepad)
                return true;
        }

        return false;
    }

    public bool HasPairedDevices()
    {
        return pairedDevices.Count > 0;
    }

    public bool GamepadLostWhileOnGamepad()
    {
        foreach (var device in playerInput.user.lostDevices)
        {
            if (device is Gamepad && pairedDevices.Contains(device.deviceId))
            {
                pairedDevices.Remove(device.deviceId);
                return true;
            }
            else
            {
                pairedDevices.Remove(device.deviceId);
                return false;
            }
        }

        return false;
    }

    private void UpdatePairedDevices(PlayerInput input)
    {
        foreach (var device in input.user.pairedDevices)
        {
            if (!pairedDevices.Contains(device.deviceId))
                pairedDevices.Add(device.deviceId);
        }

        pairedDevices.RemoveAll(id => !input.user.pairedDevices.Any(d => d.deviceId == id));
    }

    private void CurrentDeviceChangeHandling(PlayerInput input)
    {
        UpdatePairedDevices(input);
        if (playerInput != input)
            playerInput = input;

        cachedLastSelectable = EventSystem.current.currentSelectedGameObject;
        currentControlScheme = playerInput.currentControlScheme;
        OnDeviceChanged?.Invoke(CurrentInputSource);

        Debug.Log($"Current input: {CurrentInputSource}");
    }

    private void DeviceLostHandling(PlayerInput input)
    {
        OnDeviceLost?.Invoke(GamepadLostWhileOnGamepad());
    }

    private void DeviceRegainedHandling(PlayerInput input)
    {
        OnDeviceRegained?.Invoke(IsOnGamepad());
    }

    private InputSource GetInputSource(string controlScheme)
    {
        return controlScheme switch
        {
            "Gamepad" => InputSource.Gamepad,
            "Keyboard&Mouse" => InputSource.MouseKeyboard,
            "Touch" => InputSource.Touch,
            _ => InputSource.Undefined
        };
    }


    private void InitializeUIInput()
    {
        inputModule = EventSystem.current.currentInputModule as InputSystemUIInputModule;
        playerInput = GetComponent<PlayerInput>();
        playerInput.uiInputModule = inputModule;

        playerInput.onControlsChanged += CurrentDeviceChangeHandling;
        playerInput.onDeviceLost += DeviceLostHandling;
        playerInput.onDeviceRegained += DeviceRegainedHandling;

        UpdatePairedDevices(playerInput);

        currentControlScheme = playerInput.currentControlScheme;//Application.isConsolePlatform ? gamepadString : playerInput.currentControlScheme;

        if (Application.isConsolePlatform && playerInput.currentControlScheme != gamepadString)
        {
            playerInput.SwitchCurrentControlScheme(currentControlScheme);
        }

        IsInitialized = true;
    }

    private void Start()
    {
        //InitializeUIInput();
    }

    private void OnDestroy()
    {
        //if (playerInput != null)
        //{
        //    playerInput.onControlsChanged -= CurrentDeviceChangeHandling;
        //    playerInput.onDeviceLost -= DeviceLostHandling;
        //    playerInput.onDeviceRegained -= DeviceRegainedHandling;
        //}
    }
}

public enum InputSource
{
    MouseKeyboard,
    Gamepad,
    Touch,
    Undefined
}

