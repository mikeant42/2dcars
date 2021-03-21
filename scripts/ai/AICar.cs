using Godot;
using System;

// Steering behaviors based on http://www.red3d.com/cwr/steer/

// TODO: Stop when a) player is slowed/stopped and b) we are in a certain radius of the player

// TODO: Race behaviors 


public class AICar : Car
{
    private Navigation2D navigation;
    private PlayerCar player;
    private Line2D line;
    private Godot.Collections.Array otherCars;

    // how often we pathfind
    private Timer pathingTimer;

    
    private Vector2[] path;

    // look at current velocity and relate it to it's desired velocity
    float maxSpeed = 2500;
    float maxForce = 1700f;

    float radius;

    private float friction = -0.9f;
    private float drag = -0.000015f;

    private float speed = 0;
    private float minSpeed;

    private Node2D positions;
    private Position2D followPos;

    [Export]
    public bool pathFind = false;

    public float maxSteerAngle = Mathf.Deg2Rad(15);

    //private Vector2 predictLocation = Vector2

    
    [Export]
    public float stopDist = 1.5f;

    public override void _Ready()
    {
        base._Ready();


        navigation = (Navigation2D)GetNode("../../Navigation2D");
        player = (PlayerCar)GetNode("../../Car");
        line = (Line2D)GetNode("../../Line2D");

        pathingTimer = (Timer)GetNode("PathingRefresh");

        velocity = new Vector2(20,5);

        radius = 30f;

        RandomNumberGenerator rng = new RandomNumberGenerator();
        //maxSpeed = rng.RandfRange(14000, 17000);
        //maxForce = rng.RandfRange(900, 1000);

        minSpeed = maxSpeed * .8f;
        

        otherCars = GetParent().GetChildren();
        otherCars.Remove(this);

        // TEMPORARY FIX
        otherCars.Add(player);

        // select a random point to follow around the car
        rng.Randomize();
        positions = (Node2D)player.GetNode("FollowPositions");
        followPos = (Position2D)positions.GetChild(rng.RandiRange(0, positions.GetChildCount()-1));

        if (!pathFind)
            pathingTimer.Paused = true;

    }

    private Vector2 GetFrictionAndDrag(float delta)
    {
        var frictionForce = velocity * friction;
        if (velocity.Length() < 100)
        {
            frictionForce *= 3;
        }
        var dragForce = velocity * velocity.Length() * drag;
        
        return  dragForce + frictionForce;
    }



    // use physics process

    private void _ChaseBehavior(float delta)
    {
        acceleration = Vector2.Zero;
        float steering = GetSeekSteeringAngle();

        //float sep = GetSeparate();
        //steering += sep;

        acceleration += GetFrictionAndDrag(delta);
        acceleration += Transform.x * maxSpeed;

        acceleration += GetSeparate();
        //acceleration += steering;

        //acceleration = acceleration.LinearInterpolate(acceleration.Length() * GetHeading(delta), 0.1f);
        velocity = _CalcSteering(delta, steering);
        velocity += acceleration * delta;
        velocity = velocity.Clamped(maxSpeed);

        velocity = MoveAndSlide(velocity);

        
        //MoveAndSlide(GetSeparateVelocity());

        //Rotation = player.velocity.Angle(); // buggy when the player backs up
    }

    private Vector2 leftperp(Vector2 v)
    {
        return new Vector2(v.y, -v.x);
    }


    // takes a turn angle and calculates current velocity, rotation
    private Vector2 _CalcSteering(float delta, float steering)
    {

        int wheelBase = 70; // distance from front to rear wheel
        float steeringAngle = 15; // amount that the front wheel turns (deg)

        float maxSpeedReverse = 800;

        float slipSpeed = 600; // speed where traction is reduced
        float tractionFast = 0.001f;
        float tractionSlow = 0.9f;

        float superSlipSpeed = 1500;
        float tractionSuperFast = 0.025f;
        Vector2 final = Vector2.Zero;

        //GD.Print(steering);

        ///////////////////////////////////////

        var rearWheel = Position - Transform.x * wheelBase / 2;
        var frontWheel = Position + Transform.x * wheelBase / 2;
        
        rearWheel += velocity * delta;


        frontWheel += velocity.Rotated(steering * maxSteerAngle)* delta;
        
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

        //acceleration += newHeading * maxSpeed;
        
        Rotation = newHeading.Angle();

        return final;
    }
    
    
    public override void _Process(float delta)
    {
        _ChaseBehavior(delta);
    }

    // add a flee velocity from close vehicles 
    // multiplier weighs the output
    private Vector2 GetSeparate(float mult = 1)
    {
        float desiredSeparation = radius * 12;
        Vector2 sum = Vector2.Zero;
        int count = 0;
        foreach (KinematicBody2D car in otherCars)
        {
            float d = car.GlobalPosition.DistanceTo(GlobalPosition);

            if (d > 0 && d < desiredSeparation)
            {
                Vector2 diff = GlobalPosition - car.GlobalPosition;
                diff = diff.Normalized();
                diff /= d;
                sum += diff;
                count++;
            }
        }

        if (count > 0)
        {
            sum /= count; // divide by the number of velocities to get the average separate velocity

            sum = sum.Normalized();
            
            sum *= maxSpeed * mult; // desired velocity is average scaled to maxspeed
            //GD.Print(sum);
        }
        
        return sum;
        //return DesiredToTurn(sum);
    }


    private float GetSeekSteeringAngle(float mult=1)
    {
        float d = GlobalPosition.DistanceTo(followPos.GlobalPosition);
        Vector2 desiredVelocity = followPos.GlobalPosition - GlobalPosition;

        desiredVelocity = desiredVelocity.Normalized() * maxSpeed; // set magnitude        

        // reynold's steering formula
        Vector2 steering = desiredVelocity - velocity;
        steering.Clamped(maxForce);


        return DesiredToTurn(desiredVelocity) * mult;
    }


    // converts a desired velocity into the turn angle for the car [-1, 1]
    private float DesiredToTurn(Vector2 desired)
    {
        var spaceBtwen = Mathf.Clamp(desired.Normalized().Dot(velocity.Normalized()), -1, 1); // space between the angle of our desired and current velocities
        // ty stackoverflow!
        var fullSteers = Mathf.Acos(spaceBtwen) / maxSteerAngle;
        var steer = leftperp(desired.Normalized()).Dot(velocity.Normalized()) * fullSteers;
        steer = Mathf.Clamp(steer, -1, 1);

        return steer;
    }

    private Vector2 GetPathFollowVector(Vector2[] currentPath, float mult=1)
    {
        //path follow
        if (path.Length > 1)
        {
            // var start = path[1];
            // var end = path[path.Length - 1];
            // var normal = GetNormalPoint(predictLocation, end, start);

            // Vector2 dir = end - start;
            // dir = dir.Normalized();
            // dir *= 10; // speed
            // Vector2 target = normal + dir;

            var dist = currentPath[1] - GlobalPosition; // needs to have a max speed
            var direction = dist.Normalized();
            
            return direction * maxSpeed * mult;
        }
        return Vector2.Zero;
    }

    public void _on_PathingRefresh_timeout()
    {
        path = navigation.GetSimplePath(GlobalPosition, followPos.GlobalPosition, true);
        
        line.Points = path;
    }

    private float Map(float value, 
                    float istart, 
                    float istop, 
                    float ostart, 
                    float ostop)
                    {
                        return ostart + (ostop - ostart) * ((value - istart) / (istop - istart));
                    }


    
    private Vector2 GetNormalPoint(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ap = p - a;
        Vector2 ab = b - a;
        ab = ab.Normalized();
        ab *= ap.Dot(ab);
        Vector2 normalPoint = a + ab;
        
        return normalPoint;
    }


}
