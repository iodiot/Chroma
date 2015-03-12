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

      public StateConfig ForcedOn(TEvent eventType) {
        owner.forcedTransitions.Add(eventType, state);
        return this;
      }
    }

    //=================================================================

    public int TicksInState { get; private set; }

    private readonly Dictionary<TState, StateConfig> states;
    private TState initialState;
    public TState currentState { get; private set; }
    private StateConfig currentStateConfig = null;

    protected readonly Dictionary<TEvent, TState> forcedTransitions;

    // Whether the current state was entered during this tick
    public bool justEnteredState {
      get { 
        return (TicksInState == 1); 
      }
    }

    public StateMachine()
    {
      states = new Dictionary<TState, StateConfig>();
      forcedTransitions = new Dictionary<TEvent, TState>();
      TicksInState = 0;
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
      TicksInState = 0;
      return true;
    }

    public bool IsIn(TState state)
    {
      return EqualityComparer<TState>.Default.Equals(state, currentState);
    }

    public bool Trigger(TEvent eventType)
    {
      if (currentStateConfig == null)
        return false;

      if (currentStateConfig.transitions.ContainsKey(eventType))
      {
        EnterState(currentStateConfig.transitions[eventType]);
        return true;
      }

      if (forcedTransitions.ContainsKey(eventType))
      {
        EnterState(forcedTransitions[eventType]);
        return true;
      }

      return false;
    }

    public void Update(int ticks) {
      TicksInState++;

      if (currentStateConfig == null)
        return;

      // Handle autotransition
      if (currentStateConfig.autoTransition && currentStateConfig.autoDelay > 0 &&
          TicksInState >= currentStateConfig.autoDelay)
      {
        EnterState(currentStateConfig.autoTransitionTarget);
      }

    }

  }
}

