using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Different levels of vibration force
public enum VibrationForce
{
    Light,
    Hard
}
 
// Provides haptic feedback to the oculus controllers.
public class OculusHaptics : MonoBehaviour
{
    [SerializeField] OVRInput.Controller controllerMask;
    private OVRHapticsClip clipLight;
    private OVRHapticsClip clipHard;
 
    void Start()
    {
        InitializeOVRHaptics();
    }
 
    // Initializes the vibration clips that are played.
    private void InitializeOVRHaptics()
    {
        int count = 10;
        clipLight = new OVRHapticsClip(count);
        clipHard = new OVRHapticsClip(count);
        for (int i = 0; i < count; ++i)
        {
           clipLight.Samples[i] = i % 2 == 0 ? (byte)0 : (byte)45;
           clipHard.Samples[i] = i % 2 == 0 ? (byte)0 : (byte)180;
        }
        clipLight = new OVRHapticsClip(clipLight.Samples, clipLight.Samples.Length);
        clipHard = new OVRHapticsClip(clipHard.Samples, clipHard.Samples.Length);
    }
 
    // If enabled, the haptics are initialized.
    void OnEnable()
    {
        InitializeOVRHaptics();
    }
 
    // Play the vibration clip with the chosen force.
    public void Vibrate(VibrationForce vibrationForce)
    {
        OVRHaptics.OVRHapticsChannel channel = (controllerMask == OVRInput.Controller.LTouch) ? OVRHaptics.LeftChannel : OVRHaptics.RightChannel;
        channel.Preempt((vibrationForce == VibrationForce.Light) ? clipLight : clipHard);
    }
}
