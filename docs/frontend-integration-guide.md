# VibeGuess Live Quiz - Quick Reference

## Frontend Integration Checklist

### 1. Host Flow Implementation

```javascript
// 1. Connect to SignalR hub
const connection = new HubConnectionBuilder()
  .withUrl('/hubs/hostedquiz')
  .build();

await connection.start();

// 2. Create session
const result = await connection.invoke('CreateSession', 'quiz-123', 'My Quiz');
const { sessionId, joinCode } = result;

// 3. Set up event listeners
connection.on('ParticipantJoined', (data) => {
  updateParticipantList(data.participant, data.participantCount);
});

connection.on('AnswerSubmitted', (data) => {
  markParticipantAnswered(data.participantId);
});

// 4. Start game when ready
await connection.invoke('StartGame', sessionId);

// 5. Send questions
const questionData = {
  questionId: "q1",
  questionText: "Which artist sang 'Bohemian Rhapsody'?",
  options: ["Queen", "The Beatles", "Led Zeppelin", "Pink Floyd"],
  correctAnswer: "Queen",
  type: "multiple-choice"
};

await connection.invoke('NextQuestion', sessionId, questionIndex, questionData);

// 6. End session
await connection.invoke('EndSession', sessionId);
```

### 2. Player Flow Implementation

```javascript
// 1. Connect to SignalR hub
const connection = new HubConnectionBuilder()
  .withUrl('/hubs/hostedquiz')
  .build();

await connection.start();

// 2. Join session with join code
const result = await connection.invoke('JoinSession', 'ABC123', 'PlayerName');
const { sessionId, participantId } = result;

// 3. Set up event listeners
connection.on('GameStarted', () => {
  showGameStartedMessage();
});

connection.on('NewQuestion', (data) => {
  displayQuestion(data.question, data.timeLimit);
  startTimer(data.timeLimit);
});

connection.on('LeaderboardUpdate', (data) => {
  updateLeaderboard(data.leaderboard);
});

connection.on('GameEnded', (data) => {
  showFinalResults(data.leaderboard);
});

// 4. Submit answers
await connection.invoke('SubmitAnswer', sessionId, participantId, selectedAnswer);

// 5. Leave session when done
await connection.invoke('LeaveSession', sessionId, participantId);
```

## Key UI Components Needed

### Host Dashboard
- [ ] Session creation form (quiz selection, title, time limits)
- [ ] Join code display (large, shareable)
- [ ] Participant list with real-time updates
- [ ] Game controls (Start, Next Question, End)
- [ ] Question display/management interface
- [ ] Real-time answer tracking per participant
- [ ] Final results and analytics view

### Player Interface
- [ ] Join session form (join code + display name)
- [ ] Waiting lobby (show other participants)
- [ ] Question display with multiple choice options
- [ ] Timer visualization
- [ ] Answer feedback (correct/incorrect, points earned)
- [ ] Real-time leaderboard
- [ ] Final results screen

## Essential State Management

### Host State
```typescript
interface HostState {
  sessionId: string | null;
  joinCode: string | null;
  sessionState: 'Lobby' | 'Active' | 'Paused' | 'Completed';
  participants: ParticipantSummary[];
  currentQuestionIndex: number;
  questions: QuestionData[];
  isConnected: boolean;
}
```

### Player State
```typescript
interface PlayerState {
  sessionId: string | null;
  participantId: string | null;
  displayName: string | null;
  sessionState: 'Lobby' | 'Active' | 'Paused' | 'Completed';
  currentQuestion: QuestionData | null;
  hasAnsweredCurrentQuestion: boolean;
  score: number;
  leaderboard: ParticipantSummary[];
  timeRemaining: number;
  isConnected: boolean;
}
```

## Error Handling Patterns

### Connection Management
```javascript
// Auto-reconnection logic
connection.onclose(async (error) => {
  console.log('Connection lost:', error);
  showConnectionLostMessage();
  
  // Attempt to reconnect
  try {
    await connection.start();
    showReconnectedMessage();
    // Rejoin session if participant
    if (participantId && sessionId) {
      // Note: You may need to track this state separately
      // as rejoining requires special handling
    }
  } catch (err) {
    showConnectionFailedMessage();
  }
});
```

### Method Call Error Handling
```javascript
async function safeInvoke(method, ...args) {
  try {
    const result = await connection.invoke(method, ...args);
    if (!result.success) {
      showUserError(result.error);
      return null;
    }
    return result;
  } catch (error) {
    showNetworkError('Connection error. Please try again.');
    return null;
  }
}
```

## Styling Considerations

### Real-time Updates
- Use smooth animations for leaderboard changes
- Show loading states during answer submissions
- Provide visual feedback for connection status
- Display timer with appropriate urgency indicators

### Mobile Responsiveness
- Large touch targets for answer options
- Clear, readable fonts for questions
- Responsive leaderboard layout
- Easy-to-share join codes

## Testing Scenarios

### Host Testing
1. Create session and verify join code generation
2. Test participant joining/leaving
3. Test question flow (next/previous)
4. Test removing participants
5. Test ending session and analytics

### Player Testing
1. Test joining with valid/invalid join codes
2. Test duplicate display names
3. Test answer submission timing
4. Test disconnection/reconnection during game
5. Test receiving all real-time updates

### Multi-participant Testing
1. Test with multiple browser tabs/devices
2. Test simultaneous answer submissions
3. Test leaderboard accuracy
4. Test performance with many participants (10+)

## Performance Optimization

### SignalR Connection
```javascript
// Optimize SignalR connection
const connection = new HubConnectionBuilder()
  .withUrl('/hubs/hostedquiz', {
    transport: HttpTransportType.WebSockets | HttpTransportType.ServerSentEvents
  })
  .configureLogging(LogLevel.Warning) // Reduce logging in production
  .build();
```

### State Updates
- Debounce rapid leaderboard updates
- Use React.memo or similar for expensive re-renders
- Implement virtual scrolling for large participant lists

## Common Pitfalls

1. **Not handling connection drops**: Always implement reconnection logic
2. **Race conditions**: Handle rapid answer submissions properly
3. **State synchronization**: Keep local state in sync with server events
4. **Timer synchronization**: Account for network latency in timers
5. **Memory leaks**: Clean up event listeners on component unmount

## Development Tools

### SignalR Testing
```javascript
// Console helpers for testing
window.testSignalR = {
  connection,
  createSession: (quizId, title) => connection.invoke('CreateSession', quizId, title),
  joinSession: (code, name) => connection.invoke('JoinSession', code, name),
  submitAnswer: (sessionId, participantId, answer) => 
    connection.invoke('SubmitAnswer', sessionId, participantId, answer)
};
```

### REST API Testing
```bash
# Test session creation
curl -X POST https://localhost:7009/api/hosted-sessions \
  -H "Content-Type: application/json" \
  -d '{"quizId":"test-quiz","title":"Test Session","questionTimeLimit":30}'

# Test session info
curl https://localhost:7009/api/hosted-sessions/ABC123
```

## Production Deployment Notes

1. **Redis Configuration**: Ensure Redis is properly configured with persistence
2. **CORS Settings**: Update allowed origins for production domains
3. **SSL/TLS**: Ensure WebSocket connections work with SSL
4. **Load Balancing**: Configure sticky sessions for SignalR if using multiple servers
5. **Monitoring**: Monitor Redis memory usage and connection counts

## Next Steps

1. Implement basic host and player components
2. Add SignalR connection management
3. Create question display and answer submission
4. Add real-time leaderboard
5. Implement error handling and reconnection
6. Add responsive design and animations
7. Test with multiple participants
8. Add analytics and session summary views