using Godot;
class MathY
{
    public float QuadraticBezier(float from, float intermediate, float to, float t)
    {
        float q0 = Mathf.Lerp(from, intermediate, t);
        float q1 = Mathf.Lerp(intermediate, to, t);
        float r = Mathf.Lerp(q0, q1, t);
        return r;
    }
}
