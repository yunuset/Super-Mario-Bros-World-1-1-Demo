using Godot;
using System;

public partial class Coin : Area2D
{
    public override void _Ready()
    {
        var sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        sprite.Play();
    }
    public void OnBodyEntered(Node body)
    {
        if (body.IsInGroup("player"))
        {
            QueueFree();
        }
    }
}
