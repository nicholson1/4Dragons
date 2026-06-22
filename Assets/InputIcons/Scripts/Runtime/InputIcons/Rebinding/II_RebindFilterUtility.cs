using UnityEngine.InputSystem;

namespace InputIcons
{
    /// <summary>
    /// Provides reusable filter helpers for InputAction rebinding operations.
    /// </summary>
    public static class II_RebindFilterUtility
    {
        /// <summary>
        /// Applies control exclusions for rebind operations based on binding type.
        /// - Excludes stick axes for composites.
        /// - Restricts full stick bindings to only other stick-type inputs.
        /// </summary>
        public static void ApplyGamepadBindingFilters(InputActionRebindingExtensions.RebindingOperation rebindOperation, InputBinding binding)
        {
            if (rebindOperation == null || string.IsNullOrEmpty(binding.path))
                return;

            // 🚫 Prevent stick axes for composite bindings
            if (binding.isPartOfComposite)
            {
                rebindOperation
                    .WithControlsExcluding("<Gamepad>/leftStick")
                    .WithControlsExcluding("<Gamepad>/leftStick/x")
                    .WithControlsExcluding("<Gamepad>/leftStick/y")
                    .WithControlsExcluding("<Gamepad>/rightStick")
                    .WithControlsExcluding("<Gamepad>/rightStick/x")
                    .WithControlsExcluding("<Gamepad>/rightStick/y");
                return;
            }

            // 🎯 Handle full-stick bindings (not stick clicks)
            bool isStickBinding = binding.path.Contains("leftStick") || binding.path.Contains("rightStick");
            bool isStickPress = binding.path.EndsWith("Press"); // e.g., leftStickPress or rightStickPress

            if (isStickBinding && !isStickPress)
            {
                rebindOperation
                    // Block buttons, triggers, D-Pad, and stick clicks
                    .WithControlsExcluding("<Gamepad>/dpad")
                    .WithControlsExcluding("<Gamepad>/dpad/*")
                    .WithControlsExcluding("<Gamepad>/button*")
                    .WithControlsExcluding("<Gamepad>/leftTrigger")
                    .WithControlsExcluding("<Gamepad>/rightTrigger")
                    .WithControlsExcluding("<Gamepad>/leftShoulder")
                    .WithControlsExcluding("<Gamepad>/rightShoulder")
                    .WithControlsExcluding("<Gamepad>/leftStickPress")
                    .WithControlsExcluding("<Gamepad>/rightStickPress");
            }
        }
    }
}
