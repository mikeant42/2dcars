using Godot;
using System;

// Steering behaviors based on http://www.red3d.com/cwr/steer/

// TODO: Stop when a) player is slowed/stopped and b) we are in a certain radius of the player

// TODO: Race behaviors (just path following??)


public class AICar : KinematicBody2D
{
    private Navigation2D navigation;
    private Car player;
    private Line2D line;
    private Godot.Collections.Array otherCars;

    // how often we pathfind
    private Timer pathingTimer;
    

    private Vector2[] path;

    // look at current velocity and relate it to it's desired velocity
    float maxSpeed = 350;
    float maxForce = 120f;
    Vector2 velocity = Vector2.Zero;

    float radius;

    private Vector2 acceleration = Vector2.Zero;
    private float friction = -2.2f;
    private float drag = -0.0015f;

    private float speed = 0;
    private float minSpeed;

    private Node2D positions;
    private Position2D followPos;

    [Export]
    public bool pathFind = false;

    //private Vector2 predictLocation = Vector2

    
    [Export]
    public float stopDist = 1.5f;

    public override void _Ready()
    {
        base._Ready();

        navigation = (Navigation2D)GetNode("../../Navigation2D");
        player = (Car)GetNode("../../Car");
        line = (Line2D)GetNode("../../Line2D");

        pathingTimer = (Timer)GetNode("PathingRefresh");

        velocity = new Vector2(20,5);

        radius = 30f;

        RandomNumberGenerator rng = new RandomNumberGenerator();
        maxSpeed = rng.RandfRange(14000, 17000);
        maxForce = rng.RandfRange(900, 1000);

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

    private Vector2 GetHeading(float delta)
    {
        int wheelBase = 70; // distance from front to rear wheel
        float steeringAngle = 15; // amount that the front wheel turns (deg)'

        var rearWheel = Position - Transform.x * wheelBase / 2;
        var frontWheel = Position + Transform.x * wheelBase / 2;
        
        rearWheel += velocity * delta;
        frontWheel += velocity.Rotated(velocity.Angle()) * delta;
        
        var newHeading = (frontWheel - rearWheel).Normalized();

        return newHeading;
    }



    // use physics process
    
    
    public override void _Process(float delta)
    {
        acceleration = Vector2.Zero;
        Vector2 steering = GetSeekVector();

        acceleration += GetFrictionAndDrag(delta);

        acceleration += GetSeparateVelocity();
        acceleration += steering;

        //acceleration = acceleration.LinearInterpolate(acceleration.Length() * GetHeading(delta), 0.1f);

        velocity += acceleration * delta;
        velocity = velocity.Clamped(maxSpeed);

        velocity = MoveAndSlide(velocity);

        
        //MoveAndSlide(GetSeparateVelocity());

        Rotation = player.velocity.Angle(); // buggy when the player backs up
    }

    // add a flee velocity from close vehicles and seek the player
    // multiplier weighs the velocity more heavily
    // boid-esque behavior
    private Vector2 GetSeparateVelocity(float mult = 1)
    {
        float desiredSeparation = radius * 12; // TODO FIX THIS (r)
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
    }


    private Vector2 GetSeekVector(float mult=1)
    {
        float d = GlobalPosition.DistanceTo(followPos.GlobalPosition);
        Vector2 desired = followPos.GlobalPosition - GlobalPosition;

        desired = desired.Normalized() * maxSpeed; // set magnitude
        //desired = desired.LinearInterpolate(desired.Length() * GetHeading(delta), 0.000001f);
        

        // reynold's steering formula
        Vector2 steering = desired - velocity;
        steering.Clamped(maxForce);

        return steering * mult;
    }

    private Vector2 GetPathFollowVector(float mult=1)
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

            var dist = path[1] - GlobalPosition; // needs to have a max speed
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
