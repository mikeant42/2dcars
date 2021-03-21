using Godot;
using System;

public class PlayerCar : Car
{

	public Sprite frontR, frontL, backR, backL;

    public int wheelBase = 70; // distance from front to rear wheel
    public float steeringAngle = 15; // amount that the front wheel turns (deg)
    
    public float steerAngle;

    public float friction = -0.9f;  //-0.9f;
    [Export]
    public bool doesDrag = true;

    [Export]
    public bool doesDrift = true;

    public float drag = -0.000015f;    //-0.0015f;

    [Export]
    public float enginePower = 3000;


    public float braking = -650;
    public float maxSpeedReverse = 800;

    public float slipSpeed = 600; // speed where traction is reduced
    public float tractionFast = 0.01f;
    public float tractionSlow = 0.7f;

    public float superSlipSpeed = 1500;
    public float tractionSuperFast = 0.01f;


    // drift
    public float wheelDriftResistance = 5; // direction to velocity
    public float minWDrResStr = 0.05f;
    public float rotationSpeed = 0.8f;
    
    public Vector2 driftFactor = Vector2.Zero;

    public override void _Ready()
    {
        frontR = (Sprite)GetNode("Wheels/FrontR");
        frontL = (Sprite)GetNode("Wheels/FrontL");
        backR = (Sprite)GetNode("Wheels/BackR");
        backL = (Sprite)GetNode("Wheels/BackL");

    }

    

	public void _Input()
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

	    public Vector2 _CalcSteering(float delta)
    {
        /* alternative steering / wheelbase from engineeringdotnet.blogspot.com
        B = (b-r) cos(steerAngle)
        C = r(2b-r)
        b is the size of the baseline
        r = carSpeed * dt is the distance the rear wheel moves this frame
        */ 
        Vector2 final = Vector2.Zero;

        var rearWheel = Position - Transform.x * wheelBase / 2;
        var frontWheel = Position + Transform.x * wheelBase / 2;
        
        rearWheel += velocity * delta;
        frontWheel += velocity.Rotated(steerAngle) * delta;
        
        var newHeading = (frontWheel - rearWheel).Normalized();

        var traction = tractionSlow;

        if (velocity.Length() > superSlipSpeed)
            traction = tractionSuperFast;
        else if (velocity.Length() > slipSpeed)
            traction = tractionFast;

        // Reverse heading based on the forward or reverse acceleration
        var d = newHeading.Dot(velocity.Normalized());
        if (d > 0)
            final = velocity.LinearInterpolate(newHeading * velocity.Length(), traction);
        if (d < 0)
            final = -newHeading * Math.Min(velocity.Length(), maxSpeedReverse);
        
        Rotation = newHeading.Angle();

        return final;
    }

    public void _ApplyFrictionAndDrag()
    {
        // Set minimum speed
        if (velocity.Length() < 1)
        {
            velocity = Vector2.Zero;
        }

        var frictionForce = velocity * friction;
        var dragForce = velocity * velocity.Length() * drag;
        if (velocity.Length() < 100)
        {
            frictionForce *= 3;
        }
        
        if (!doesDrag) dragForce = Vector2.Zero;
        acceleration += dragForce + frictionForce;
    }

    public void _ApplyDrift()
    {
        // drift
        var angle = velocity.Angle() - (Rotation - spriteAngle);
        float wDrResStr = (float)Math.Sin(angle);
        if (isDrifting())
        {
            velocity += new Vector2(wDrResStr*wheelDriftResistance, 0).Rotated(Rotation);
        }
        else
        {
            velocity *= Math.Abs(Mathf.Cos(angle));
        }

        //velocity *= driftFactor;
        
    }

    public override void _Process(float delta)// Use _Physics_Process for fixed 60hz timestep
    {
        acceleration = Vector2.Zero;
        _Input();
        _ApplyFrictionAndDrag();
        if (doesDrift) _ApplyDrift();
        velocity = _CalcSteering(delta);
        _RotateWheels();

        velocity += acceleration * delta;
        
        velocity = MoveAndSlide(velocity);

    }




    private void _RotateWheels()
    {
        frontR.Rotation = Mathf.LerpAngle(frontR.Rotation, steerAngle, 0.1f);
        frontL.Rotation = Mathf.LerpAngle(frontL.Rotation, steerAngle, 0.1f);

        backR.Rotation = Mathf.LerpAngle(backR.Rotation, steerAngle, 0.01f);
        backL.Rotation = Mathf.LerpAngle(backL.Rotation, steerAngle, 0.01f);
    }



}
