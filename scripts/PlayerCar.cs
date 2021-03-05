using Godot;
using System;

public class PlayerCar : Car
{
	public override void _Input()
	{
		int turn = 0;
		if (Input.IsActionPressed("steer_right")) turn +=1;
		if (Input.IsActionPressed("steer_left")) turn -= 1;
		
		steerAngle = turn * Mathf.Deg2Rad(steeringAngle);
		//GD.Print(Mathf.Rad2Deg(steerAngle));
		//velocity = Vector2.Zero;

		if (Input.IsActionPressed("accelerate"))
		{
			acceleration = Transform.x * enginePower;

		}
		
		if (Input.IsActionPressed("brake"))
		{
			acceleration = Transform.x * braking;
		}
	}

    public override void _Process(float delta)
    {
        base._Process(delta);
    }
}
