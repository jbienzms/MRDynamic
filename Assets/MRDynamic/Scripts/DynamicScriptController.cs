using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DynamicScriptController : MonoBehaviour
{
    #region Member Variables
    private Script dynamicScript;   // The dynamically loaded script
    private object updateFunc;      // The dynamically loaded update function
    #endregion // Member Variables

    #region Unity Inspector Fields
    [Tooltip("The name of the file to load from storage.")]
    public string ScriptFileName = "DynamicScript";

    [Tooltip("The name of the script function to call for every update.")]
    public string UpdateFunction = "updateFunction";

    [Tooltip("The game object that will be updated dynamically.")]
    public GameObject DynamicObject;
    #endregion // Unity Inspector Fields



    #region Script Callable Methods

    /// In this example we have two types of C# methods that can be called from Lua:
    ///
    /// 1. "Pure" - methods which only use Lua native types
    /// 2. "Unity" - methods which use Unity types but require some conversion
    ///
    /// Any custom conversion is handled in the file LuaCustomConverters.cs

    #region Pure Script Methods
    /// <summary>
    /// Gets the X rotation of the dynamic object in Euler angles.
    /// </summary>
    /// <returns>
    /// A <see cref="float"/> containing the X Euler angle.
    /// </returns>
    private float GetRotationX()
    {
        return GetRotationUnity().x;
    }

    /// <summary>
    /// Gets the Y rotation of the dynamic object in Euler angles.
    /// </summary>
    /// <returns>
    /// A <see cref="float"/> containing the Y Euler angle.
    /// </returns>
    private float GetRotationY()
    {
        return GetRotationUnity().y;
    }

    /// <summary>
    /// Gets the Z rotation of the dynamic object in Euler angles.
    /// </summary>
    /// <returns>
    /// A <see cref="float"/> containing the Z Euler angle.
    /// </returns>
    private float GetRotationZ()
    {
        return GetRotationUnity().z;
    }

    /// <summary>
    /// Sets the rotation of the dynamic object to the specified Euler angles.
    /// </summary>
    /// <param name="x">
    /// The X rotation.
    /// </param>
    /// <param name="y">
    /// The Y rotation.
    /// </param>
    /// <param name="z">
    /// The Z rotation.
    /// </param>
    private void SetRotation(float x, float y, float z)
    {
        SetRotationUnity(new Vector3(x, y, z));
    }
    #endregion // Pure Script Methods

    #region Unity Script Methods
    /// <summary>
    /// Gets the rotation of the dynamic object in Euler angles.
    /// </summary>
    /// <returns>
    /// A <see cref="Vector3"/> containing the Euler angles.
    /// </returns>
    private Vector3 GetRotationUnity()
    {
        if (DynamicObject == null)
        {
            Debug.LogError($"{nameof(GetRotationUnity)} called but no dynamic object to update.");
            return Vector3.zero;
        }
        return DynamicObject.transform.rotation.eulerAngles;
    }

    /// <summary>
    /// Sets the rotation of the dynamic object to the specified Euler angles.
    /// </summary>
    /// <param name="rotation">
    /// The Euler rotation.
    /// </param>
    private void SetRotationUnity(Vector3 rotation)
    {
        if (DynamicObject == null)
        {
            Debug.LogError($"{nameof(SetRotationUnity)} called but no dynamic object to update.");
            return;
        }

        DynamicObject.transform.rotation = Quaternion.Euler(rotation);
    }
    #endregion // Unity Script Methods
    #endregion // Script Callable Methods

    #region Internal Methods
    /// <summary>
    /// Initializes the scripting engine, sets up converters, etc.
    /// </summary>
    /// <returns>
    /// A <see cref="Task"/> that represents the operation.
    /// </returns>
    /// <remarks>
    /// This method is asynchronous because initialization includes loading any available
    /// dynamic script. <seealso cref="LoadScriptAsync(Script)"/>.
    /// </remarks>
    private async Task InitializeScriptingAsync()
    {
        // Setup global type converters
        LuaCustomConverters.RegisterAll();

        // Create the dynamic script object
        Script script = new Script();

        // Register "Pure" functions as callable from Lua script
        script.Globals[nameof(GetRotationX)] = (Func<float>)GetRotationX;
        script.Globals[nameof(GetRotationY)] = (Func<float>)GetRotationY;
        script.Globals[nameof(GetRotationZ)] = (Func<float>)GetRotationZ;
        script.Globals[nameof(SetRotation)] = (Action<float, float, float>)SetRotation;

        // Register "Unity" functions as callable from Lua script
        script.Globals[nameof(GetRotationUnity)] = (Func<Vector3>)GetRotationUnity;
        script.Globals[nameof(SetRotationUnity)] = (Action<Vector3>)SetRotationUnity;

        // Load the script
        await LoadScriptAsync(script);

        // Store the dynamically loaded script in the class-level variable
        dynamicScript = script;
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    /// <summary>
    /// Loads a dynamic script (from disk, network, etc).
    /// </summary>
    /// <param name="script">
    /// The dynamic <see cref="Script"/> object where the script will be loaded.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> that represents the operation.
    /// </returns>
    /// <remarks>
    /// This method is asynchronous because UWP file and network access APIs are asynchronous.
    /// However, currently the sample just loads the script from a string.
    /// </remarks>
    protected virtual async Task LoadScriptAsync(Script script)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        // Define script in a string for now
        // TODO: Load from a file later

        /*
        // This version uses the "Pure" methods defined above
        string updateCode = @"
        function updateFunction (delta)
        	-- Get current rotation
            y = GetRotationY();

            -- Rotate on Y axis by delta time
            y = y + (delta * 100);

            -- If rotated past 359 degrees, loop back
            if (y >= 360) then
                y = y - 360;
            end

            -- Set rotation back
            SetRotation(0, y, 0);
        end";
        */

        // This version uses the "Unity" methods defined above
        string updateCode = @"
        function updateFunction (delta)
        	-- Get current rotation
            rot = GetRotationUnity();

            -- Calculate angle from Y axis and delta time
            angle = rot.y + (delta * 100);

            -- If rotated past 359 degrees, loop back
            if (angle >= 360) then
                angle = angle - 360;
            end

            -- Rotate X and Y
            rot.x = angle;
            rot.y = angle;

            -- Set rotation back
            SetRotationUnity(rot);
        end";

        // Load script string into script
        script.DoString(updateCode);

        // Attempt to get a reference to the update function by name (specified in the inspector)
        updateFunc = script.Globals[UpdateFunction];
    }
    #endregion // Internal Methods

    #region Unity Overrides
    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    protected virtual async void Start()
    {
        try
        {
            // Initialize the scripting engine
            await InitializeScriptingAsync();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Could not initialize scripting engine. {ex.Message}");
        }
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    protected virtual void Update()
    {
        // If a dynamic update function has been loaded
        if (updateFunc != null)
        {
            // Call it and pass in delta time
            dynamicScript.Call(updateFunc, Time.deltaTime);
        }
    }
    #endregion // Unity Overrides
}
