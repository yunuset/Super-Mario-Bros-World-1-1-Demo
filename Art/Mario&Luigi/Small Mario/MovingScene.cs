using Godot;
using System;

public partial class MovingScene : Node2D
{
    
    Mario player;
    Node character;
    public float SceneVelocity;
    public bool inUnderground = false;
    public override void _Ready()
    {
        player = GetParent().GetNode<Mario>("Mario");
    }
    public void OnPipeEntered(Node body)
    {
        if (body.IsInGroup("player"))
        {
            character = body;
        }
    }
    public void OnPipe2Entered(Node body)
    {
        player.SetPhysicsProcess(false);
        inUnderground = false;
        Position = new Vector2(-2575, 0);
        player.Position = new Vector2(50, 175);
        player.SetPhysicsProcess(true);
    }
    public void OnPipeExited(Node body)
    {
        if (character==body)
        {
            character = null;
        }
    }
    public override void _Process(double delta)
    {
        Vector2 position = Position;
        if (character != null && Input.IsActionPressed("down"))
        {
            player.SetPhysicsProcess(false);
            character = null;
            inUnderground = true;
            position = new Vector2(-768, -240);
            player.Position = new Vector2(38, 30);
            player.SetPhysicsProcess(true);
        }
        else if (player.Position.X >= 100 && player.Velocity.X > 0 && !inUnderground)
        {
            position.X -= player.Velocity.X * (float)delta;
        }
        Position = position;
    }
    public void OnDeathBoxBodyEntered(Node body)
    {
        if (body.IsInGroup("player"))
        {
            player.Hit();
        }
    }
}
