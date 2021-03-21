using Godot;
using System;


public class AIClone : Sprite
{
    [Export]
    public AICar realCar;

    public override void _Ready()
    {
        SetAsToplevel(true);
    }

    public override void _Process(float delta)
    {
        GlobalPosition = realCar.GlobalPosition;

        Rotation = realCar.Rotation + Mathf.Deg2Rad(90);
    }


}
