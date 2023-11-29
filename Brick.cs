using Godot;
using System.Collections.Generic;

public partial class Brick : StaticBody2D
{
    AnimatedSprite2D sprite;
    AnimationPlayer animation;
    List<Node> characters = new List<Node>();
    string state = "default";
    public override void _Ready()
    {
        animation = GetNode<AnimationPlayer>("AnimationPlayer");
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        sprite.Animation = "default";
        sprite.Play();
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
    public void Hit()
    {
        switch (state)
        {
            case "default":
                state = "hit";
                animation.Play("hit_impulse");
                var particles = GetParent().GetNode<GpuParticles2D>("GPUParticles2D");
                particles.Emitting = true;
                sprite.QueueFree();
                var DeathTimer = new Timer();
                AddChild(DeathTimer);
                DeathTimer.WaitTime = 0.10f;
                DeathTimer.OneShot = true;
                DeathTimer.Start();
                DeathTimer.Timeout += () => QueueFree();
                break;
        }
        
    }
}
