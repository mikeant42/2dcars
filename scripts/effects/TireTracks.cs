using Godot;
using System;

public class TireTracks : Particles2D
{
    private Car car;
    [Export]
    public float speedWhenGenTracks = 1000;

    public override void _Ready()
    {
        car = (Car)GetParent();
        Emitting = false;
    }

    public override void _Process(float delta)
    {
		//if (car.velocity.Length() >= speedWhenGenTracks && Math.Abs(wDrResStr) < 0.1f)
        if (car.velocity.Length() >= speedWhenGenTracks && car.isDrifting())
		{
			Emitting = true;
		}
		else
		{
			Emitting = false;
		}
    }
}
