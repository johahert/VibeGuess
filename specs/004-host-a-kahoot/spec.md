# Feature Specification: Kahoot-Style Hosted Music Quiz Sessions

**Feature Branch**: `004-host-a-kahoot`  
**Created**: 2025-09-28  
**Status**: Draft  
**Input**: User description: "i want to add a feature for this app to host a music quiz for other players. This should work similar to a game of kahoot, where only the host is required to be logged in, in order to start the session/room with a selected quiz. The host does not participate in the game but controls the flow/state of the session like changing to the next question"

## Execution Flow (main)
```
1. Parse request for a host-led multiplayer quiz experience
	→ Capture key roles: authenticated host vs. unauthenticated players
2. Extract functional pillars
	→ Session creation, player joining, host-controlled pacing, scoring/leaderboards
3. Flag underspecified behaviours
	→ Mark join limits, scoring logic, reconnection rules with [NEEDS CLARIFICATION]
4. Outline host and player journeys end-to-end
	→ Ensure lobby, active play, and wrap-up states are represented
5. Derive acceptance scenarios covering happy path and control edge cases
6. Define testable functional requirements mapped to each journey step
7. Identify primary entities involved in orchestrating live play
8. Review scope for business clarity before handing off for planning
```

---

## ⚡ Quick Guidelines
- ✅ Center the experience on the host’s ability to run a smooth live quiz session
- ✅ Emphasise how players discover, join, and experience each stage of the game
- ❌ Avoid prescribing implementation details such as networking protocols or UI frameworks
- 🎯 Keep language outcome-focused so stakeholders can sign off without technical translation

### Section Requirements
- **Mandatory sections**: Completed below for scenarios, requirements, entities, and review
- **Optional sections**: Omitted where not relevant to hosted quiz experience
- When a section doesn’t apply, it has been removed rather than left blank

---

## User Scenarios & Testing *(mandatory)*

### Primary User Story
As an authenticated quiz creator, I want to host a live music quiz session that other players can join with a shared quiz code, so that I can orchestrate gameplay and track scoring without personally competing.

### Acceptance Scenarios
1. **Given** a host is logged in and selects an existing quiz, **When** they launch a live session, **Then** the system must generate a join code and present a lobby view showing connected players and host controls.
2. **Given** players have joined a live session lobby using the code, **When** the host starts the game and advances through questions, **Then** each player must receive the current question in sync and have their answers recorded for scoring and the host must see aggregate progress.

### Edge Cases
- What happens when the host disconnects or navigates away during an active session? The game should have a grace period of a paused quiz for 30 sec, after that room is terminated
- How does the system handle players who join after the host has already started the first question? Whatever is simplest to implement and handle
- What happens if two players attempt to join with the same display name? Auto suffix names

## Requirements *(mandatory)*

### Functional Requirements
- **FR-001**: System MUST require the host to authenticate before creating or managing a live quiz session.
- **FR-002**: System MUST allow the host to select an existing quiz and launch a live session that produces a shareable join code or link for players.
- **FR-003**: System MUST let players join an active lobby without logging in by submitting the join code and a display name.
- **FR-004**: System MUST provide the host with controls to start the session, advance to the next question, reveal answers, and end the game.
- **FR-005**: System MUST display each active question to all joined players once the host advances, capturing their responses within the allotted timeframe. The respone length should be adjustable by the host. Scoring should have a high baseline score and a little extra difference for timing
- **FR-006**: System MUST calculate individual player scores based on answer correctness (and timing, if applicable) and keep a running leaderboard visible to the host. Baseline score for a correct answer is 100 and 1 point for every second left when answering 
- **FR-007**: System MUST allow the host to remove or mute disruptive players from the session. Removed player should not be able to get back in (blacklisted for the session). The host should be able to remove people from the blacklist
- **FR-008**: System MUST present a final summary screen to the host and players when the session ends, highlighting top performers and offering an option to restart or close the room.

### Key Entities *(include if feature involves data)*
- **Live Quiz Session**: Represents a real-time instance of a quiz being hosted; tracks associated quiz, join code, current state (lobby/in-progress/completed), and timestamps.
- **Participant**: Represents a player in a session; stores display name, join status, response history, and cumulative score.
- **Host Control State**: Captures the host’s configured settings for the session (question pacing, removed players, visibility of leaderboard) and the sequence of state transitions made during gameplay.

---

## Review & Acceptance Checklist
*GATE: Automated checks run during main() execution*

### Content Quality
- [x] No implementation details (languages, frameworks, APIs)
- [x] Focused on user value and business needs
- [x] Written for non-technical stakeholders
- [x] All mandatory sections completed

### Requirement Completeness
- [ ] No [NEEDS CLARIFICATION] markers remain
- [x] Requirements are testable and unambiguous where fully specified
- [ ] Success criteria are measurable (pending clarification of scoring metrics and timing)
- [x] Scope is clearly bounded to hosted live sessions
- [ ] Dependencies and assumptions identified (pending answers to clarification items)

---

## Execution Status
*Updated by main() during processing*

- [x] User description parsed
- [x] Key concepts extracted
- [x] Ambiguities marked
- [x] User scenarios defined
- [x] Requirements generated
- [x] Entities identified
- [ ] Review checklist passed (clarifications outstanding)

---
