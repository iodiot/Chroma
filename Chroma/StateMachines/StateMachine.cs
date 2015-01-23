using System;
using System.Collections.Generic;

namespace Chroma.StateMachines
{

  public class StateMachine<TState, TEvent> 
    where TState : struct
    where TEvent : struct
  {

    public class StateConfig {
      public readonly TState state;
      public readonly StateMachine<TState, TEvent> owner;
      public readonly Dictionary<TEvent, TState> transitions;
      public bool autoTransition = false;
      public TState autoTransitionTarget = default(TState);
      public int autoDelay = -1;

      public StateConfig(StateMachine<TState, TEvent> machine, TState state)
      {
        owner = machine;
        this.state = state;
        transitions = new Dictionary<TEvent, TState>();
      }

      // Configuration ------------------------------------------------
      public StateConfig On(TEvent eventType, TState state) {
        transitions.Add(eventType, state);
        return this;
      }

      public StateConfig AutoTransitionTo(TState state)
      {
        autoTransition = true;
        autoTransitionTarget = state;
        return this;
      }

      public StateConfig IsInitial()
      {
        owner.SetInitialState(state);
        return this;
      }

      public StateConfig After(int delay)
      {
        autoDelay = delay;
        return this;
      }

    }

    //=================================================================

    private int ticksInState = 0;

    private readonly Dictionary<TState, StateConfig> states;
    private TState initialState;
    public TState currentState { get; private set; }
    private StateConfig currentStateConfig = null;

    // Whether the current state was entered during this tick
    public bool justEnteredState {
      get { 
        return (ticksInState == 1); 
      }
    }

    public StateMachine()
    {
      states = new Dictionary<TState, StateConfig>();

    }

    // Configuration --------------------------------------------------

    public StateConfig State(TState state) {

      // First declared state is initial by default
      if (states.Count == 0)
      {
        initialState = state;
      }

      StateConfig config;
      if (states.ContainsKey(state))
      {
        config = states[state];
      }
      else
      {
        config = new StateConfig(this, state);
        states.Add(state, config);
      }

      return config;
    }

    public void SetInitialState(TState state)
    {
      initialState = state;
    }

    // Operation -------------------------------------------------------

    public void Start() 
    {
      EnterState(initialState);
    }

    public bool EnterState(TState state)
    {
      if (!states.ContainsKey(state)) {
        return false;
      }
        
      currentState = state;
      currentStateConfig = states[state];
      ticksInState = 0;
      return true;
    }

    public bool Trigger(TEvent eventType)
    {
      if (currentStateConfig == null)
        return false;
      if (!currentStateConfig.transitions.ContainsKey(eventType))
        return false;

      EnterState(currentStateConfig.transitions[eventType]);
      return true;
    }

    public void Update(int ticks) {
      ticksInState++;

      if (currentStateConfig == null)
        return;

      // Handle autotransition
      if (currentStateConfig.autoTransition && currentStateConfig.autoDelay > 0 &&
          ticksInState >= currentStateConfig.autoDelay)
      {
        EnterState(currentStateConfig.autoTransitionTarget);
      }

      // Handle animations
    }

  }
}

