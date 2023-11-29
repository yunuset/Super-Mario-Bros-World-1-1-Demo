using Godot;
using System;
using System.Threading.Tasks;
using System.Linq;

public partial class Turtle : CharacterBody2D
{
    int speed = 50;
    int direction = -1;
    AnimatedSprite2D sprite;
    CharacterBody2D player;
    Timer DeathTimer;
    int gravity = 1200;
    [Export]
    public string state = "fly";
    public Turtle()
    {
        DeathTimer = new Timer();
        AddChild(DeathTimer);
        DeathTimer.WaitTime = 2;
        DeathTimer.OneShot = true;
    }
    public override void _Ready()
    {
        player = GetParent().GetParent().GetNode<Mario>("Mario");
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        sprite.Animation = state;
        switch (state)
        {
            case "fly":
                gravity = 0;
                break;
        }
        sprite.Play();
    }
    public void Hit()
    {
        //CallDeferred("set_collision_layer_value", 3, false);
        //CallDeferred("set_physics_process", false);
        switch(state)
        {
            case "fly":
                gravity = 1200;
                sprite.Animation = "walk";
                state = "walk";
                break;
            case "walk":
                speed = 0;
                sprite.Animation = "knock_down";
                sprite.Position = new Vector2(sprite.Position.X, sprite.Position.Y+2);
                state = "knock_down";
                break;
            case "knock_down":
                sprite.Animation = "die";
                state = "die";
                QueueFree();
                break;
            case "moving_fast":
                speed = 0;
                state = "knock_down";
                break;
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        MoveAndSlide();
        Vector2 velocity = Velocity;
        sprite.FlipH = (velocity.X>0);
        if (!IsOnFloor())
        {
            velocity.Y += gravity*(float)delta;
        }
        int collisionCount = GetSlideCollisionCount();
        PhysicsBody2D[] collidedBodies = new PhysicsBody2D[collisionCount];
        for (int index = 0; index < collisionCount; index++)
        {
            KinematicCollision2D collision = GetSlideCollision(index);
            var normal = collision.GetNormal();
            var collider = (PhysicsBody2D)collision.GetCollider();
            if (collidedBodies.Contains(collider) || (collider.IsInGroup("ground") && normal == Vector2.Up))
            {
                continue;
            }
            collidedBodies[index] = collider;
            if (normal.X != 0 || normal == Vector2.Up)
            {
                switch (state)
                {
                    case "fly": case "walk":
                        if (collider.IsInGroup("player"))
                        {
                            collider.Call("Hit");
                        }
                        if (normal.X != 0) direction = (int)normal.X;
                        break;
                    case "knock_down":
                        if (normal.X != 0) direction = (int)normal.X;
                        speed = 200;
                        state = "moving_fast";
                        break;
                    case "moving_fast":
                        collider.Call("Hit");
                        if (!collider.IsInGroup("player") && !collider.IsInGroup("enemy"))
                        {
                            if (normal.X != 0) direction = (int)normal.X;
                        }
                        break;
                }
            }
        }
        velocity.X = speed*direction;
        Velocity = velocity;
    }
}
