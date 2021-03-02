using Godot;
using System;


public class AIClone : Node2D
{
    [Export]
    public NodePath playerPath;

    private AICar player;
    public override void _Ready()
    {
        player = (AICar)GetNode(playerPath);
    }

    public override void _Process(float delta)
    {
        Position = player.Position;

        Rotation = player.Rotation + Mathf.Deg2Rad(90);
    }


}