# Phase 0 Research: Kahoot-Style Hosted Music Quiz Sessions

## Real-time Delivery Stack
- **Decision**: Use ASP.NET Core SignalR hubs hosted within the existing VibeGuess API to coordinate host and player messaging.
- **Rationale**: SignalR is already specified by stakeholders, integrates natively with the .NET stack, and supports WebSockets with automatic fallbacks for real-time broadcasting.
- **Alternatives Considered**:
  - **gRPC streaming**: Higher setup overhead for browser clients and less alignment with stakeholder request.
  - **Polling via REST**: Simpler but fails the responsiveness needed for quiz pacing and leaderboards.

## Session Lifecycle & Host Presence
- **Decision**: Suspend the session when the host disconnects, begin a 30-second grace countdown, and automatically terminate the room if the host fails to reconnect before the timer elapses.
- **Rationale**: Matches the specification requirement, prevents stalled games, and gives hosts a brief recovery window.
- **Alternatives Considered**:
  - **Immediate termination**: Violates the 30-second pause expectation and punishes transient network blips.
  - **Infinite wait**: Risks orphaned sessions consuming resources indefinitely.

## Late Join Handling
- **Decision**: Allow players to join the lobby until the host starts question one; once gameplay begins, additional join attempts are rejected with guidance to wait for the next session.
- **Rationale**: Keeps implementation simple while preserving fairness for players who missed early questions.
- **Alternatives Considered**:
  - **Spectator mode**: Adds UI logic outside current scope.
  - **Allow full participation mid-game**: Complicates scoring and answer pacing.

## Display Name Collisions
- **Decision**: Auto-suffix duplicate names with incrementing numerals (e.g., "Alex", "Alex (2)") while persisting the original base name for analytics.
- **Rationale**: Delivers a frictionless join flow and mirrors expectations from similar quiz platforms.
- **Alternatives Considered**:
  - **Reject duplicates**: Forces users to try multiple names and slows lobby entry.
  - **Randomized aliases**: Reduces player personalization and complicates host recognition.

## Answer Timing & Scoring Formula
- **Decision**: Host chooses the per-question response window (default 30 seconds). Correct answers yield 100 base points plus 1 bonus point per unused second, capped at the configured limit.
- **Rationale**: Implements the spec’s baseline-plus-bonus model while keeping arithmetic transparent.
- **Alternatives Considered**:
  - **Linear percentage scoring**: Harder to explain during live play.
  - **Fixed bonus tiers**: Adds branching logic without strong user value.

## Moderation & Blacklist Behaviour
- **Decision**: Provide host controls to remove or mute participants, persist a per-session blacklist, and allow the host to reinstate players from that list.
- **Rationale**: Satisfies requirement for both blocking disruptive users and permitting manual review.
- **Alternatives Considered**:
  - **Permanent ban without reinstatement**: Conflicts with spec which demands re-entry control.
  - **Temporary mute only**: Fails to address removal use cases.

## Session Analytics Persistence
- **Decision**: Store post-session analytics summarizing participant count, average accuracy, and podium results in the existing database through a dedicated summary table.
- **Rationale**: Aligns with the requirement to persist minimal analytics and leverages current persistence stack.
- **Alternatives Considered**:
  - **In-memory only**: Loses data after session ends.
  - **Full event history storage**: Increases storage costs beyond the "minimal" mandate.

## Scaling & Backplane Strategy
- **Decision**: Configure SignalR with Redis backplane support to ensure message distribution across instances once the feature moves beyond single-node deployments.
- **Rationale**: Matches the constitution’s expectation for Redis usage and prepares for the 100 concurrent user goal.
- **Alternatives Considered**:
  - **In-memory hub state**: Works only on single server and prevents horizontal scaling.
  - **Azure SignalR Service**: Viable long-term option but adds cloud dependency beyond immediate scope.
