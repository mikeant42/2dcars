using Godot;
using System;


// singleton class to manage level effects
public class EffectsManager : Node
{
    public Viewport waterVP; // shaders that want the reflection copy from this viewport
    public Node Ais;
    

    public override void _Ready()
    {
        waterVP = (Viewport)GetNode("/root/Level/WaterViewport");
        Ais = (Node)GetNode("/root/Level/AI");

        waterVP.Size = OS.WindowSize;


        // add copies to reflection viewport
        foreach (AICar car in Ais.GetChildren())
        {
            AIClone clone = new AIClone();
            Sprite p = (Sprite)car.GetNode("Frame");
            clone.Texture = p.Texture;
            clone.realCar = car;
            clone.FlipH = true;
            clone.Scale = new Vector2(1, 1.1f);
            waterVP.AddChild(clone);
        }

        GetTree().Root.Connect("size_changed", this, "ViewportSizeChanged");

    }

    public void ViewportSizeChanged()
    {
        waterVP.Size = OS.WindowSize;
    }
}
