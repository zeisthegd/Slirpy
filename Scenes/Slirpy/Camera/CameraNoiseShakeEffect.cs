using Godot;
using System;

public class CameraNoiseShakeEffect : Camera
{
    #region Constant value of the effect

    const float SPEED = 1.0F;
    const float DECAY_RATE = 1.5F;
    const float MAX_YAW = 0.05F;
    const float MAX_PITCH = 0.05F;
    const float MAX_ROLL = 0.05F;
    const float MAX_TRAUMA = 0.05F;

    #endregion

    #region Default Values

    Vector3 startRotation;
    float trauma = 0.0F;
    float time = 0.0F;
    OpenSimplexNoise noise = new OpenSimplexNoise();
    uint noiseSeed = GD.Randi();

    #endregion

    public override void _Ready()
    {
        InitOpenSimplexNoise();

        //This variable is reset if the camera's position is changed by other scripts
        //such as zooming in/out or focusing on a different position
        //This should not be done when the camera shake is happening
        startRotation = this.Rotation;      
    }
    
    private void InitOpenSimplexNoise()
    {
        noise.Seed = (int)noiseSeed;
        noise.Octaves = 1;
        noise.Period = 256.0F;
        noise.Persistence = 0.5F;
        noise.Lacunarity = 1.0F;
    }

    public override void _Process(float delta)
    {
        if(trauma > 0.0F)
        {
            DecayTrauma(delta);
            ApplyShake(delta);
        }
    }

    private void AddTrauma(float amount)
    {
        trauma = Mathf.Min(trauma + amount, MAX_TRAUMA);
    }

    private void DecayTrauma(float delta)
    {
        float change = DECAY_RATE * delta;
        trauma = Math.Max(trauma - change, 0.0F);
    }

    private void ApplyShake(float delta)
    {
        //Using a magic number here to get the pleasing effect at SPEED 1.0F
        time += delta * SPEED * 5000.0F;
        var shake = trauma * trauma;
        var yaw = MAX_YAW * shake * GetNoiseValue((int)noiseSeed, time);
        var pitch = MAX_PITCH * shake * GetNoiseValue((int)noiseSeed + 1, time);
        var roll = MAX_ROLL * shake * GetNoiseValue((int)noiseSeed + 2, time);

        Rotation = startRotation + new Vector3(pitch, yaw, roll);
    }

    private float GetNoiseValue(int seedValue, float t)
    {
        noise.Seed = seedValue;
        return noise.GetNoise1d(t);
    }

}
