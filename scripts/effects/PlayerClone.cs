using Godot;
using System;

public class PlayerClone : Sprite
{
    [Export]
    public NodePath playerPath;

    private PlayerCar player;
    public override void _Ready()
    {
        player = (PlayerCar)GetNode(playerPath);
    }

    public override void _Process(float delta)
    {
        Position = player.Position;
        Rotation = player.Rotation + Mathf.Deg2Rad(90);
    }


}
