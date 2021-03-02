using Godot;
using System;

public class PlayerClone : Node2D
{
    [Export]
    public NodePath playerPath;

    private Car player;
    public override void _Ready()
    {
        player = (Car)GetNode(playerPath);
    }

    public override void _Process(float delta)
    {
        Position = player.Position;
        Rotation = player.Rotation + Mathf.Deg2Rad(90);
    }


}
