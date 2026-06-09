using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputIcons
{
    /// <summary>
    /// Handles all binding-related logic and resolution for Input Icons.
    /// Responsible for finding, analyzing, and categorizing input bindings.
    /// </summary>
    public class InputIconsBindingResolver
    {
        #region Binding Index Resolution

        /// <summary>
        /// Returns the binding index of an action of the active device.
        /// </summary>
        /// <param name="action">The input action to search</param>
        /// <param name="bindingType">Binding type of the action. Can be anything if the action is not a part of a composite</param>
        /// <param name="controlScheme">Control scheme to filter by</param>
        /// <param name="bindingNumberInList">Which binding to return if multiple exist (0-based)</param>
        /// <returns>Index of the binding, or -1 if not found</returns>
        public int GetIndexOfBindingType(InputAction action, InputIconsUtility.BindingType bindingType, string controlScheme, int bindingNumberInList = 0)
        {
            if (action == null) return -1;

            int c = 0;
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (BindingGroupContainsControlScheme(action.bindings[i].groups, controlScheme))
                {
                    bool isComposite = ActionIsComposite(action, controlScheme);
                    if (!isComposite)
                    {
                        // Is it the actual binding we are looking for? In case we have several fitting bindings
                        // (e.g. Attack with space and lmb, but we need the lmb not space)
                        if (bindingNumberInList != 0)
                        {
                            if (bindingNumberInList == c)
                                return i;
                            else
                                c++;
                        }
                        else
                            return i;
                    }
                    else if (action.bindings[i].name.ToUpper() == bindingType.ToString().ToUpper())
                    {
                        if (bindingNumberInList == c)
                            return i;
                        else
                            c++;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the binding index of an action of the active device with composite type filtering.
        /// </summary>
        /// <param name="action">The input action to search</param>
        /// <param name="compositeType">Whether to look for composite or non-composite bindings</param>
        /// <param name="bindingType">Binding type of the action</param>
        /// <param name="controlScheme">Control scheme to filter by</param>
        /// <param name="bindingNumberInList">Which binding to return if multiple exist (0-based)</param>
        /// <returns>Index of the binding, or -1 if not found</returns>
        public int GetIndexOfBindingType(InputAction action, InputIconsUtility.CompositeType compositeType, InputIconsUtility.BindingType bindingType, string controlScheme, int bindingNumberInList = 0)
        {
            if (action == null) return -1;

            int c = 0;
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (BindingGroupContainsControlScheme(action.bindings[i].groups, controlScheme))
                {
                    bool isComposite = BindingIsComposite(action.bindings[i], controlScheme);
                    //Debug.Log("is composite: " + isComposite);
                    //Debug.Log(action.name+" - "+compositeType.ToString()+" - "+bindingType.ToString()+ " - "+controlScheme+" - "+bindingNumberInList);
                    if (!isComposite && compositeType == InputIconsUtility.CompositeType.NonComposite)
                    {
                        // Is it the actual binding we are looking for? In case we have several fitting bindings
                        // (e.g. Attack with space and lmb, but we need the lmb not space)
                        if (bindingNumberInList != 0)
                        {
                            if (bindingNumberInList == c)
                                return i;
                            else
                                c++;
                        }
                        else
                            return i;
                    }
                    else if (action.bindings[i].name.ToUpper() == bindingType.ToString().ToUpper() && compositeType == InputIconsUtility.CompositeType.Composite)
                    {
                        if (bindingNumberInList == c)
                            return i;
                        else
                            c++;
                    }
                }
            }
            InputIconsLogger.LogWarning("Binding not found, returning -1 as index");
            return -1;
        }

        /// <summary>
        /// Returns all binding indexes of an action of the active device.
        /// </summary>
        /// <param name="action">The input action to search</param>
        /// <param name="bindingType">Binding type of the action</param>
        /// <param name="controlScheme">Control scheme to filter by</param>
        /// <returns>List of binding indexes</returns>
        public List<int> GetIndexesOfBindingType(InputAction action, InputIconsUtility.BindingType bindingType, string controlScheme)
        {
            List<int> outputList = new List<int>();

            if (action == null) return outputList;

            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (BindingGroupContainsControlScheme(action.bindings[i].groups, controlScheme))
                {
                    if (!BindingIsComposite(action.bindings[i], controlScheme))
                    {
                        outputList.Add(i);
                    }
                    else if (action.bindings[i].name.ToUpper() == bindingType.ToString().ToUpper())
                    {
                        outputList.Add(i);
                    }
                }
            }
            return outputList;
        }

        /// <summary>
        /// Returns all binding indexes of an action of the active device with composite type filtering.
        /// </summary>
        /// <param name="action">The input action to search</param>
        /// <param name="compositeType">Whether to look for composite or non-composite bindings</param>
        /// <param name="bindingType">Binding type of the action</param>
        /// <param name="controlScheme">Control scheme to filter by</param>
        /// <returns>List of binding indexes</returns>
        public List<int> GetIndexesOfBindingTypeDeviceSpecific(InputAction action, InputIconsUtility.CompositeType compositeType, InputIconsUtility.BindingType bindingType, string controlScheme)
        {
            List<int> outputList = new List<int>();

            if (action == null) return outputList;

            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (BindingGroupContainsControlScheme(action.bindings[i].groups, controlScheme))
                {
                    if (!BindingIsComposite(action.bindings[i], controlScheme) && compositeType == InputIconsUtility.CompositeType.NonComposite)
                    {
                        outputList.Add(i);
                    }
                    else if (action.bindings[i].name.ToUpper() == bindingType.ToString().ToUpper() && compositeType == InputIconsUtility.CompositeType.Composite)
                    {
                        outputList.Add(i);
                    }
                }
            }
            return outputList;
        }

        #endregion

        #region Binding Group Analysis

        /// <summary>
        /// Groups binding indexes by their logical groupings (composites vs individual bindings).
        /// </summary>
        /// <param name="action">The input action to analyze</param>
        /// <param name="controlScheme">Control scheme to filter by</param>
        /// <returns>List of lists, where each inner list contains binding indexes that belong together</returns>
        public List<List<int>> GetGroupIndexesOfActionDeviceSpecific(InputAction action, string controlScheme)
        {
            List<List<int>> outputList = new List<List<int>>();
            List<int> currentList = new List<int>();

            if (action == null)
                return outputList;

            bool lastWasPartOfComposite = false;

            for (int i = 0; i < action.bindings.Count; i++)
            {
                InputBinding binding = action.bindings[i];
                bool isPartOfComposite = binding.isPartOfComposite;
                bool isComposite = binding.isComposite;

                if (isComposite)
                {
                    if (currentList.Count > 0)
                        outputList.Add(new List<int>(currentList));

                    currentList.Clear();
                    continue;
                }

                // Check if the binding belongs to the specified control scheme
                if (BindingGroupContainsControlScheme(binding.groups, controlScheme))
                {
                    // This is a one button binding
                    if (!isComposite && !isPartOfComposite)
                    {
                        if (currentList.Count > 0)
                            outputList.Add(new List<int>(currentList));

                        currentList.Clear();
                        currentList.Add(i);
                        outputList.Add(new List<int>(currentList));
                        currentList.Clear();

                        lastWasPartOfComposite = false;
                    }
                    // Is part of a composite
                    else if (isPartOfComposite)
                    {
                        if (!lastWasPartOfComposite)
                        {
                            if (currentList.Count > 0)
                                outputList.Add(new List<int>(currentList));

                            currentList.Clear();
                        }

                        currentList.Add(i);
                        lastWasPartOfComposite = true;
                    }
                    else if (isComposite)
                    {
                        lastWasPartOfComposite = false;
                    }
                }
            }

            if (currentList.Count > 0)
                outputList.Add(currentList);

            return outputList;
        }

        /// <summary>
        /// Groups binding indexes by their logical groupings with composite type filtering.
        /// </summary>
        /// <param name="action">The input action to analyze</param>
        /// <param name="compositeType">Whether to include composite or non-composite bindings</param>
        /// <param name="controlScheme">Control scheme to filter by</param>
        /// <returns>List of lists, where each inner list contains binding indexes that belong together</returns>
        public List<List<int>> GetGroupIndexesOfActionDeviceSpecific(InputAction action, InputIconsUtility.CompositeType compositeType, string controlScheme)
        {
            List<List<int>> outputList = new List<List<int>>();
            List<int> currentList = new List<int>();

            if (action == null)
                return outputList;

            bool lastWasPartOfComposite = false;

            for (int i = 0; i < action.bindings.Count; i++)
            {
                InputBinding binding = action.bindings[i];
                bool isPartOfComposite = binding.isPartOfComposite;
                bool isComposite = binding.isComposite;

                if (isComposite)
                {
                    if (currentList.Count > 0)
                        outputList.Add(new List<int>(currentList));

                    currentList.Clear();
                    continue;
                }

                // Check if the binding belongs to the specified control scheme
                if (BindingGroupContainsControlScheme(binding.groups, controlScheme))
                {
                    // If the binding is non-composite and compositeType is NonComposite, or if the binding is part of a composite and compositeType is Composite
                    if ((!isComposite && !isPartOfComposite && compositeType == InputIconsUtility.CompositeType.NonComposite) ||
                        (isPartOfComposite && compositeType == InputIconsUtility.CompositeType.Composite))
                    {
                        if (compositeType == InputIconsUtility.CompositeType.NonComposite)
                        {
                            currentList.Clear();
                            currentList.Add(i);
                            outputList.Add(new List<int>(currentList));
                            currentList.Clear();
                        }

                        if (compositeType == InputIconsUtility.CompositeType.Composite)
                        {
                            currentList.Add(i);
                            lastWasPartOfComposite = true;
                        }
                    }
                    // If the binding is composite and compositeType is NonComposite, or if the binding is not part of a composite and compositeType is Composite
                    else
                    {
                        if (lastWasPartOfComposite)
                        {
                            if (currentList.Count > 0)
                                outputList.Add(new List<int>(currentList));

                            currentList.Clear();
                        }

                        lastWasPartOfComposite = false;
                    }
                }
            }

            if (currentList.Count > 0)
                outputList.Add(currentList);

            return outputList;
        }

        #endregion

        #region Binding Validation and Analysis

        /// <summary>
        /// Determines if two bindings belong to different binding groups.
        /// </summary>
        /// <param name="action">The input action containing the bindings</param>
        /// <param name="binding1">First binding to compare</param>
        /// <param name="binding2">Second binding to compare</param>
        /// <param name="controlSchemeName">Control scheme to check against</param>
        /// <returns>True if the bindings are in different groups</returns>
        public bool IsDifferentBindingGroup(InputAction action, InputBinding binding1, InputBinding binding2, string controlSchemeName)
        {
            if (action == null) return false;
            if (binding1 == null) return false;
            if (binding2 == null) return false;

            // If either one is not part of a composite, return true
            if (!binding1.isPartOfComposite || !binding2.isPartOfComposite)
                return true;

            bool bindingOneFound = false;
            InputBinding bindingOne = new InputBinding();
            InputBinding bindingTwo = new InputBinding();

            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (action.bindings[i] == binding1)
                {
                    if (!bindingOneFound)
                    {
                        bindingOne = action.bindings[i];
                        bindingOneFound = true;
                    }
                    else
                        bindingTwo = action.bindings[i];
                }

                if (action.bindings[i] == binding2)
                {
                    if (!bindingOneFound)
                    {
                        bindingOne = action.bindings[i];
                        bindingOneFound = true;
                    }
                    else
                        bindingTwo = action.bindings[i];
                }
            }

            bool bindingOnePassed = false;
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (action.bindings[i] == bindingOne)
                {
                    bindingOnePassed = true;
                    continue;
                }

                if (bindingOnePassed)
                {
                    if (action.bindings[i] == binding2)
                    {
                        if (bindingOne.isPartOfComposite && bindingTwo.isPartOfComposite)
                            return false;
                        else
                            return true;
                    }

                    if (action.bindings[i] != bindingTwo && BindingGroupContainsControlScheme(action.bindings[i].groups, controlSchemeName))
                    {
                        if (action.bindings[i].isComposite || !action.bindings[i].isPartOfComposite)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a binding group contains a specific control scheme.
        /// Comparing like this takes more effort, but improves usability by also accepting small typos in form of lower/uppercase errors.
        /// Also splits group into an array. A string is more difficult to check as "Gamepad" gets detected in "Gamepad 2" as well for example.
        /// </summary>
        /// <param name="group">The binding group string (semicolon-separated)</param>
        /// <param name="controlScheme">The control scheme to look for</param>
        /// <returns>True if the group contains the control scheme</returns>
        public bool BindingGroupContainsControlScheme(string group, string controlScheme)
        {
            List<string> groupArray = group.Split(';').ToList();

            foreach (string item in groupArray)
            {
                if (item.Trim().ToUpper() == controlScheme.Trim().ToUpper())
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines if a binding is part of a composite for the current device.
        /// </summary>
        /// <param name="binding">The binding to check</param>
        /// <returns>True if the binding is part of a composite</returns>
        public bool BindingIsComposite(InputBinding binding)
        {
            if (binding == null) return false;

            string deviceString = GetActiveDeviceString();
            return BindingIsComposite(binding, deviceString);
        }

        /// <summary>
        /// Determines if a binding is part of a composite for a specific control scheme.
        /// </summary>
        /// <param name="binding">The binding to check</param>
        /// <param name="controlSchemeName">The control scheme to check against</param>
        /// <returns>True if the binding is part of a composite</returns>
        public bool BindingIsComposite(InputBinding binding, string controlSchemeName)
        {
            if (binding == null) return false;

            if (BindingGroupContainsControlScheme(binding.groups, controlSchemeName))
            {
                if (binding.isPartOfComposite)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if an action has composite bindings for the current device.
        /// </summary>
        /// <param name="action">The action to check</param>
        /// <returns>True if the action has composite bindings</returns>
        public bool ActionIsComposite(InputAction action)
        {
            if (action == null) return false;

            string deviceString = GetActiveDeviceString();
            return ActionIsComposite(action, deviceString);
        }

        /// <summary>
        /// Determines if an action has composite bindings for a specific control scheme.
        /// </summary>
        /// <param name="action">The action to check</param>
        /// <param name="controlSchemeName">The control scheme to check against</param>
        /// <returns>True if the action has composite bindings</returns>
        public bool ActionIsComposite(InputAction action, string controlSchemeName)
        {
            if (action == null) return false;

            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (BindingGroupContainsControlScheme(action.bindings[i].groups, controlSchemeName))
                {
                    if (action.bindings[i].isPartOfComposite)
                        return true;
                }
            }

            return false;
        }

        #endregion

        #region Composite Type Detection

        /// <summary>
        /// Gets all available composite types for an action and control scheme.
        /// </summary>
        /// <param name="inputAction">The input action to analyze</param>
        /// <param name="controlSchemeName">The control scheme to check</param>
        /// <returns>Set of available composite types</returns>
        public HashSet<InputIconsUtility.CompositeType> GetAvailableCompositeTypesOfAction(InputAction inputAction, string controlSchemeName)
        {
            HashSet<InputIconsUtility.CompositeType> outputList = new HashSet<InputIconsUtility.CompositeType>();

            foreach (InputBinding binding in inputAction.bindings)
            {
                if (BindingGroupContainsControlScheme(binding.groups, controlSchemeName))
                {
                    if (binding.isPartOfComposite)
                        outputList.Add(InputIconsUtility.CompositeType.Composite);

                    if (!binding.isComposite && !binding.isPartOfComposite)
                        outputList.Add(InputIconsUtility.CompositeType.NonComposite);
                }
            }

            return outputList;
        }

        #endregion

        #region Binding Counting

        /// <summary>
        /// Gets the number of distinct binding groups for an action and control scheme.
        /// </summary>
        /// <param name="action">The input action to count</param>
        /// <param name="controlSchemeName">The control scheme to filter by</param>
        /// <returns>Number of distinct binding groups</returns>
        public int GetNumberOfBindingsAllMatchingBindingsDeviceSpecific(InputAction action, string controlSchemeName)
        {
            if (action == null) return 0;

            List<int> bindingIDs = new List<int>();
            InputBinding lastBinding = new InputBinding();
            int numberOfMatches = 1;
            bool needsDelimiter = false;

            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (bindingIDs.Count > 0)
                {
                    if (IsDifferentBindingGroup(action, lastBinding, action.bindings[i], controlSchemeName))
                    {
                        needsDelimiter = true;
                    }
                }

                if (BindingGroupContainsControlScheme(action.bindings[i].groups, controlSchemeName))
                {
                    if (needsDelimiter)
                    {
                        bindingIDs.Add(-1);
                        needsDelimiter = false;
                        numberOfMatches++;
                    }
                    bindingIDs.Add(i);
                }

                lastBinding = action.bindings[i];
            }

            return numberOfMatches;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets the name of the currently used device.
        /// </summary>
        /// <returns>Control scheme name for the current device</returns>
        private string GetActiveDeviceString()
        {
            InputDevice device = InputIconsManagerSO.GetCurrentInputDevice();
            string deviceString;
            if (device is Gamepad)
            {
                deviceString = InputIconsManagerSO.GetGamepadControlSchemeName(0);
            }
            else
            {
                deviceString = InputIconsManagerSO.GetKeyboardControlSchemeName(0);
            }

            return deviceString;
        }

        /// <summary>
        /// Logs binding information for debugging purposes.
        /// </summary>
        /// <param name="binding">The binding to log</param>
        public void LogBindingInfo(InputBinding binding)
        {
            Debug.Log("binding: " + binding.name + " ___ composite: " + binding.isComposite + " ___ partOfComposite:" + binding.isPartOfComposite);
        }

        #endregion
    }
}