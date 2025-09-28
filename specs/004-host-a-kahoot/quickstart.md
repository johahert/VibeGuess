# Quickstart: Hosting a Live Music Quiz Session

1. **Prepare the environment**
   - Ensure the API is running locally with SignalR hubs enabled: `dotnet run --project src/VibeGuess.Api/VibeGuess.Api.csproj`
   - Start Redis locally (or configure connection string) to support the SignalR backplane.

2. **Create a hosted session (host only)**
   - Send `POST /api/hosted-sessions` with the quiz ID the host wants to use.
   - Response returns `sessionId`, `joinCode`, and the SignalR hub URL/token for the host client.

3. **Join as players**
   - Each player opens the web client join screen, enters the `joinCode`, and establishes a SignalR connection using the player token from `POST /api/hosted-sessions/{sessionId}/join`.
   - Confirm that duplicate names receive suffixes (e.g., “Taylor (2)”).

4. **Run the game flow**
   - Host uses hub methods `StartSession`, `AdvanceQuestion`, and `RevealAnswer` to control pacing.
   - Players answer via `SubmitAnswer` hub method before the per-question deadline elapses.
   - Observe leaderboard updates pushed to host and players after each question.

5. **Disconnect tests**
   - Temporarily drop the host connection and verify the session pauses, displaying the countdown.
   - Reconnect within 30 seconds to resume; repeat with a longer disconnect to confirm automatic termination.

6. **Session wrap-up**
   - Host calls `EndSession` (or finishes last question) to transition to the summary view.
   - Confirm final standings, blacklist state, and analytics persisted via `GET /api/hosted-sessions/{sessionId}/summary`.
