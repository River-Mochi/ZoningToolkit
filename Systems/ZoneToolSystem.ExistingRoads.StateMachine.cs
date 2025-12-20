// Systems/ZoneToolSystem.ExistingRoads.StateMachine.cs
// Simple click/drag state machine for ZoneToolSystemExistingRoads.

namespace ZoningToolkit.Systems
{
    using System.Collections.Generic;
    using Game.Input;
    using Game.Tools;
    using Unity.Jobs;

    internal delegate JobHandle StateCallback(
        ZoneToolSystemExistingRoadsState previousState,
        ZoneToolSystemExistingRoadsState nextState);

    internal enum ZoneToolSystemExistingRoadsState
    {
        Default,
        Selecting,
        Selected
    }

    internal sealed class ZoneToolSystemExistingRoadsStateMachine
    {
        private ZoneToolSystemExistingRoadsState m_CurrentState;
        private readonly Dictionary<(ZoneToolSystemExistingRoadsState previous, ZoneToolSystemExistingRoadsState next), StateCallback> m_Transitions;

        internal ZoneToolSystemExistingRoadsStateMachine(
            Dictionary<(ZoneToolSystemExistingRoadsState previous, ZoneToolSystemExistingRoadsState next), StateCallback> transitions)
        {
            m_CurrentState = ZoneToolSystemExistingRoadsState.Default;
            m_Transitions = transitions;
        }

        internal ApplyMode Transition(IProxyAction applyAction)
        {
            ZoneToolSystemExistingRoadsState previousState = m_CurrentState;

            switch (m_CurrentState)
            {
                case ZoneToolSystemExistingRoadsState.Default:
                    if (applyAction.WasPressedThisFrame() && applyAction.WasReleasedThisFrame())
                    {
                        // Single quick click.
                        m_CurrentState = ZoneToolSystemExistingRoadsState.Selected;
                        TryRunCallback(previousState, m_CurrentState);
                        return ApplyMode.Apply;
                    }
                    else if (applyAction.WasPressedThisFrame() && !applyAction.WasReleasedThisFrame())
                    {
                        // Press start (drag select).
                        m_CurrentState = ZoneToolSystemExistingRoadsState.Selecting;
                        TryRunCallback(previousState, m_CurrentState);
                        return ApplyMode.None;
                    }
                    else if (applyAction.IsPressed())
                    {
                        // Holding mouse while dragging.
                        m_CurrentState = ZoneToolSystemExistingRoadsState.Selecting;
                        TryRunCallback(previousState, m_CurrentState);
                        return ApplyMode.None;
                    }
                    else if (applyAction.WasReleasedThisFrame())
                    {
                        // Mouse released -> apply.
                        m_CurrentState = ZoneToolSystemExistingRoadsState.Selected;
                        TryRunCallback(previousState, m_CurrentState);
                        return ApplyMode.Apply;
                    }

                    break;

                case ZoneToolSystemExistingRoadsState.Selecting:
                    if (applyAction.IsPressed())
                    {
                        // Continue drag select.
                        m_CurrentState = ZoneToolSystemExistingRoadsState.Selecting;
                        TryRunCallback(previousState, m_CurrentState);
                        return ApplyMode.None;
                    }
                    else if (!applyAction.IsPressed() && applyAction.WasReleasedThisFrame())
                    {
                        // Drag finished; selection is done, but zoning will be applied by Selected state.
                        m_CurrentState = ZoneToolSystemExistingRoadsState.Selected;
                        TryRunCallback(previousState, m_CurrentState);
                        return ApplyMode.None;
                    }

                    break;

                case ZoneToolSystemExistingRoadsState.Selected:
                    // After applying, go back to idle.
                    m_CurrentState = ZoneToolSystemExistingRoadsState.Default;
                    TryRunCallback(previousState, m_CurrentState);
                    return ApplyMode.Apply;
            }

            return ApplyMode.None;
        }

        internal void Reset()
        {
            m_CurrentState = ZoneToolSystemExistingRoadsState.Default;
        }

        private void TryRunCallback(ZoneToolSystemExistingRoadsState previous, ZoneToolSystemExistingRoadsState next)
        {
            if (m_Transitions.TryGetValue((previous, next), out StateCallback callback))
            {
                callback(previous, next);
            }
        }
    }
}
