namespace Chroma.States
{
  public abstract class State
  {
    protected readonly Core core;

    public State(Core core)
    {
      this.core = core;
    }

    public virtual void Load()
    {
    }

    public virtual void Unload()
    {
    }

    public virtual void Enter()
    {
    }

    public virtual void Leave()
    {
    }

    public virtual void Update(int ticks)
    {
    }

    public virtual void Draw()
    {
    }
  }
}
