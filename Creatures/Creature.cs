using System.Windows.Media.Imaging;

namespace Desktop_Creatures.Creatures;

public abstract class Creature
{
    public double X { get; protected set; }
    public double Y { get; protected set; }
    public double SpeedX { get; protected set; }
    public BitmapImage CurrentFrame { get; protected set; } = null!;

    public abstract void Update();

    public virtual void DragTo(double x, double y)
    {
        X = x;
        Y = y;
    }

    public virtual void Release()
    {
    }
}