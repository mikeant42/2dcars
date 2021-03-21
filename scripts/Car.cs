using Godot;
using System;

public class Car : KinematicBody2D
{
    public Vector2 velocity = Vector2.Zero;
    public Vector2 acceleration = Vector2.Zero;
    public float spriteAngle = -(float)Math.PI / 2.0f; // -90deg
    public float driftConst = 0.65f;


    public bool isDrifting()
    {
        var vB = worldToBody(velocity.Normalized(), Rotation);
        GD.Print(vB.x);
        if (Math.Abs(vB.x) < driftConst)
        {
            return true;
        }

        return false;
    }

    private Vector2 worldToBody(Vector2 v, float angle)
	{
		Vector2 vn = new Vector2(v.x * Mathf.Cos(angle) + v.y * Mathf.Sin(angle),
			v.x * Mathf.Sin(angle) - v.y * Mathf.Cos(angle));
		
		return vn;
	}

}
