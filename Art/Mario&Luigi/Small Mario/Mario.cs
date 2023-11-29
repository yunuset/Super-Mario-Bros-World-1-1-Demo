using Godot;
using System.Linq;

/*Vector2 motion = Velocity * (float)delta;
        KinematicCollision2D collision;
        int max_collisions = 4;
        int collision_count = 0;
        while ((motion.Length() > 0) && (collision_count < max_collisions))
        {
            collision = MoveAndCollide(motion);
            if (collision != null)
            {
                Velocity = Velocity.Bounce(collision.GetNormal());
                var collider = collision.GetCollider();
                motion = collision.GetRemainder();
                collision_count += 1;
            }
            else
            {
                break;
            }
        }
*/
public partial class Mario : CharacterBody2D
{
    public int Speed = 100;
    int fastSpeed = 200;

    float airSpeedMultiplier = 1;
    bool inUnderground;
    float Acceleration = 0.5f;
    bool jumpedWithSpeed = false;
    int jumpSpeed = 400;
    int bounceSpeed = 200;
    MovingScene world;
    bool bounced = false;
    int gravity = 1200;
    int deathGravity = 800;
    float deathDelay = 0;
    bool startDeathAnimation = false;
    private KinematicCollision2D[] GetAllCollisions()
    {
        int collisionCount = GetSlideCollisionCount();
        var collisionResult = shapeCast.CollisionResult;
        KinematicCollision2D[] collisions = new KinematicCollision2D[collisionCount+collisionResult.Count];
        for (int index = 0; index < collisionCount; index++)
        {
            collisions[index] = GetSlideCollision(index);
        }
        for (int index = 0; index < collisionResult.Count; index++)
        {
            collisions[collisionCount+index] = (KinematicCollision2D)collisionResult[index];
        }
        collisions = collisions.Distinct().ToArray();
        return collisions;
    }
    AnimatedSprite2D animatedSprite;
    Vector2 screenSize;
    ShapeCast2D shapeCast;
    bool dead = false;
    public override void _Ready()
    {
        animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        screenSize = GetViewportRect().Size;
        world = GetParent().GetNode<MovingScene>("World_1_1");
        shapeCast = GetNode<ShapeCast2D>("ShapeCast2D");
    }
    public void Hit()
    {
        dead = true;
    }
    public float QuadraticBezier(float from, float intermediate, float to, float t)
    {
        float q0 = Mathf.Lerp(from, intermediate, t);
        float q1 = Mathf.Lerp(intermediate, to, t);
        float r = Mathf.Lerp(q0, q1, t);
        return r;
    }
    public Vector2 Bounce(Vector2 velocity)
    {
        velocity.Y -= bounceSpeed;
        bounced = true;
        return velocity;
    }
    public override void _PhysicsProcess(double delta)
    {
        inUnderground = world.inUnderground;
        MoveAndSlide();
        Vector2 velocity = Velocity;
        Vector2 direction = Vector2.Zero;
        if (Input.IsActionPressed("move_right"))
        {
            direction.X += 1;
        }
        if (Input.IsActionPressed("move_left"))
        {
            direction.X -= 1;
        }
        if (Input.IsActionPressed("jump") && IsOnFloor())
        {
            velocity.Y -= jumpSpeed;
        }
        if (Input.IsActionJustReleased("jump") && !IsOnFloor())
        {
            velocity.Y = QuadraticBezier(velocity.Y, velocity.Y/2, 0, 0.5f);
        }
        if (direction.X == 0)
        {
            if (!bounced)
            {
                velocity.X = Mathf.Lerp(velocity.X, 0, 0.1f);
            }
            else
            {
                velocity.X = 0;
                bounced = false;
            }
        }
        else if (Input.IsActionPressed("move_faster") && IsOnFloor())
        {
            velocity.X = Mathf.Lerp(velocity.X, fastSpeed*direction.X, 0.025f);
            if (Input.IsActionPressed("jump"))
            {
                jumpedWithSpeed = true;
            }
        }
        else if (jumpedWithSpeed && !IsOnFloor())
        {
            velocity.X = Mathf.Lerp(velocity.X, fastSpeed*direction.X, 0.025f);
        }
        else
        {
            jumpedWithSpeed = false;
            if (Mathf.Abs(velocity.X - Speed*direction.X) > 2*Speed)
            {
                velocity.X = Mathf.Lerp(velocity.X, Speed*direction.X, 0.025f);
                animatedSprite.Animation = "change_direction";
            }
            else
            {
                velocity.X = Mathf.Lerp(velocity.X, Speed*direction.X, 0.1f);
            }
        }
        if (IsOnFloor())
        {
            if (Mathf.Abs(velocity.X) > 25)
            {
                if (Input.IsActionPressed("move_faster"))
                {
                    animatedSprite.Animation = "run";
                }
                else
                {
                    animatedSprite.Animation = "walk";
                }
                bool newDirection = (Velocity.X < 0);
                animatedSprite.FlipH = newDirection;
                if ((velocity.X<0) != (direction.X<0))
                {
                    animatedSprite.Animation = "change_direction";
                }
            }
            else
            {
                animatedSprite.Animation = "stand";
            }
        }
        else
        {
            velocity.X *= airSpeedMultiplier;
            velocity.Y += gravity * (float)delta;
            animatedSprite.Animation = "jump";
        }
        if (dead)
        {
            velocity.X = 0;
            GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true;
            animatedSprite.Animation = "die";
            deathDelay += (float)delta;
            if (deathDelay < 1)
            {
                velocity = Vector2.Zero;
            }
            else
            {
                if (!startDeathAnimation)
                {
                    velocity.Y -= jumpSpeed;
                    startDeathAnimation = true;
                }
                else
                {
                    velocity.Y += deathGravity * (float)delta;
                }
            }
        }
        animatedSprite.Play();
        if (!inUnderground)
        {
            Position = new Vector2(x: Mathf.Clamp(Position.X, 10, 100), y: Position.Y);
        }
        var collisions = GetAllCollisions();
        foreach (KinematicCollision2D collision in collisions)
        {
            var collider = (PhysicsBody2D)collision.GetCollider();
            var normal = collision.GetNormal();
            if (collider.IsInGroup("enemy"))
            {
                if (Vector2.Up == normal)
                {
                    collider.Call("Hit");
                    velocity = Bounce(velocity);
                }
            }
            else if (collider.IsInGroup("block"))
            {
                if (normal == Vector2.Down)
                {
                    
                    collider.Call("Hit");
                }
            }
        }
        velocity.Y = Mathf.Clamp(velocity.Y, -jumpSpeed, jumpSpeed);
        Velocity = velocity;
    }
}
