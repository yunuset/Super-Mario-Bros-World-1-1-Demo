using Godot;
using System;
using System.Collections.Generic;

public partial class CoinBlock : StaticBody2D
{
    AnimatedSprite2D sprite;
    List<Node> characters = new List<Node>();
    AnimationPlayer animation;
    string state = "default";
    Vector2 defaultPosition;
    public override void _Ready()
    {
        animation = GetNode<AnimationPlayer>("AnimationPlayer");
        defaultPosition = Position;
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        sprite.Animation = "default";
        sprite.Play();
    }
    public override void _Process(double delta)
    {
        if (characters.Count != 0 && state == "hit")
        {
            foreach (Node character in characters)
            {
                character.Call("Hit");
            }
        }
    }
    public void OnArea2DBodyEntered(Node body)
    {
        characters.Add(body);
    }
    public void OnArea2DBodyExited(Node body)
    {
        if (characters.Contains(body))
        {
            characters.Remove(body);
        }
    }
    public void Hit()
    {
        switch (state)
        {
            case "default":
                sprite.Animation = "hit";
                animation.Play("hit_impulse");
                state = "hit";
                var DeathTimer = new Timer();
                AddChild(DeathTimer);
                DeathTimer.WaitTime = 0.10f;
                DeathTimer.OneShot = true;
                DeathTimer.Start();
                DeathTimer.Timeout += () => state = "emptied";
                break;
        }
        
    }
}
