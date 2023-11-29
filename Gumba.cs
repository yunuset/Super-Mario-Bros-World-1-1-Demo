using Godot;
using System;
using System.Threading.Tasks;
using System.Linq;

public partial class Gumba : CharacterBody2D
{
    int speed = 50;
    int direction = -1;
    AnimatedSprite2D sprite;
    CharacterBody2D player;
    int gravity = 1200;
    bool dead = false;
    public override void _Ready()
    {
        player = GetParent().GetParent().GetNode<Mario>("Mario");
        sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        sprite.Animation = "walk";
        sprite.Play();
    }
    public void Hit()
    {
        sprite.Position = new Vector2(sprite.Position.X, sprite.Position.Y+2);
        Timer DeathTime = new Timer();
        AddChild(DeathTime);
        DeathTime.WaitTime = 2;
        DeathTime.OneShot = true;
        CallDeferred("set_collision_layer_value", 3, false);
        CallDeferred("set_physics_process", false);
        DeathTime.Start();
        speed = 0;
        sprite.Animation = "die";
        DeathTime.Timeout += () => QueueFree();
    }
    public override void _PhysicsProcess(double delta)
    {
        MoveAndSlide();
        Vector2 velocity = Velocity;
        velocity.X = speed*direction;
        {
            velocity.Y += gravity*(float)delta;
        }
        int collisionCount = GetSlideCollisionCount();
        PhysicsBody2D[] collidedBodies = new PhysicsBody2D[collisionCount];
        for (int index = 0; index < collisionCount; index++)
        {
            KinematicCollision2D collision = GetSlideCollision(index);
            var collider = (PhysicsBody2D)collision.GetCollider();
            if (collidedBodies.Contains(collider))
            {
                continue;
            }
            collidedBodies[index] = collider;
            var normal = collision.GetNormal();
            if (normal.X != 0 || normal == Vector2.Up)
            {
                if (collider.IsInGroup("player"))
                {
                    player.Call("Hit");
                }
                if (normal.X != 0) direction = (int)normal.X;
            }
        
        }
        Velocity = velocity;
    }
}
