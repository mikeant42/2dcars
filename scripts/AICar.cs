using Godot;
using System;

public class AICar : Car
{
    private Navigation2D navigation;
    private Car player;
    private Line2D line;
    private RayCast2D frontCast;

    //private Vector2[] path;
    
    [Export]
    public float stopDist = 1.5f;

    public override void _Ready()
    {
        base._Ready();

        navigation = (Navigation2D)GetNode("../Navigation2D");
        player = (Car)GetNode("../Car");
        line = (Line2D)GetNode("../Line2D");
        frontCast = (RayCast2D)GetNode("FrontCast");
        
    }

    // control the direction of the car
    // need to control how often the pathfinding runs
    public override void _Input()
    {
        var path = navigation.GetSimplePath(GlobalPosition, player.GlobalPosition, true);
        line.Points = path;
        if (path.Length > 1)
        {
            int turn = 0;
            //for (int i = 0; i < path.Length; i++) {
                var distance = path[1] - GlobalPosition;
                //var direction = distance.Normalized();
                var dir = GlobalPosition.DirectionTo(path[1]);
                if (dir.Dot(GlobalPosition) < 0)
                    turn = 1;
                else
                    turn = -1;

                GD.Print(turn);
                
                steerAngle = turn * Mathf.Deg2Rad(steeringAngle);
                // the steerAngle should be -1 or +1 depending on whether we want to make a left/right turn * steering which is the strength of each turn
                //steerAngle = direction.Angle();
                //GD.Print(Mathf.Rad2Deg(direction.Angle()));
                
                //steerAngle = GlobalPosition.AngleTo(player.GlobalPosition) * Mathf.Deg2Rad(steeringAngle);
                if (path.Length > stopDist)
                    if (!frontCast.IsColliding())
                        acceleration = Transform.x * enginePower; // we still want to move straight
                    else
                        acceleration = Transform.x * braking;
            
            //}
                

        }
        
    }
}
