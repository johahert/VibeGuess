# NextQuestion Method Usage Examples

## Overview

The `NextQuestion` method now uses strongly-typed `QuestionData` instead of a generic `object`, providing type safety, validation, and better development experience.

## Method Signature

```csharp
public async Task<object> NextQuestion(string sessionId, int questionIndex, QuestionData questionData)
```

## JavaScript/TypeScript Usage

### 1. Basic Multiple Choice Question

```javascript
const questionData = {
  questionId: "q1",
  questionText: "Which artist sang 'Bohemian Rhapsody'?",
  options: ["Queen", "The Beatles", "Led Zeppelin", "Pink Floyd"],
  correctAnswer: "Queen",
  type: 0, // MultipleChoice
  timeLimit: 30,
  points: 100,
  metadata: {
    difficulty: 3,
    category: "Classic Rock",
    explanation: "Bohemian Rhapsody was written by Freddie Mercury and performed by Queen in 1975."
  }
};

const result = await connection.invoke('NextQuestion', sessionId, 1, questionData);
console.log(result); 
// { success: true, questionId: "q1", timeLimit: 30, startTime: "2025-10-02T14:30:00.000Z" }
```

### 2. True/False Question

```javascript
const questionData = {
  questionId: "q2",
  questionText: "The Beatles were formed in Liverpool.",
  options: ["True", "False"],
  correctAnswer: "True",
  type: 1, // TrueFalse
  timeLimit: 15,
  points: 50,
  metadata: {
    difficulty: 1,
    category: "Music History",
    explanation: "The Beatles were indeed formed in Liverpool in 1960."
  }
};

const result = await connection.invoke('NextQuestion', sessionId, 2, questionData);
```

### 3. Text Input Question

```javascript
const questionData = {
  questionId: "q3",
  questionText: "Name any song by Elvis Presley.",
  options: [
    "Heartbreak Hotel", "Hound Dog", "Love Me Tender", "Jailhouse Rock",
    "Can't Help Falling in Love", "Suspicious Minds", "Blue Suede Shoes"
  ],
  correctAnswer: "Heartbreak Hotel", // Any of the options would be correct
  type: 2, // Text
  timeLimit: 45,
  points: 150,
  metadata: {
    difficulty: 2,
    category: "Rock and Roll",
    explanation: "Elvis Presley, known as the King of Rock and Roll, had many hit songs."
  }
};

const result = await connection.invoke('NextQuestion', sessionId, 3, questionData);
```

## C# Usage (Host Application)

```csharp
// Create question data
var questionData = new QuestionData
{
    QuestionId = "q1",
    QuestionText = "Which instrument is Jimi Hendrix famous for playing?",
    Options = new List<string> { "Guitar", "Piano", "Drums", "Bass" },
    CorrectAnswer = "Guitar",
    Type = QuestionType.MultipleChoice,
    TimeLimit = 25,
    Points = 120,
    Metadata = new QuestionMetadata
    {
        Difficulty = 2,
        Category = "Rock Guitar",
        Tags = new List<string> { "guitar", "rock", "legend" },
        Explanation = "Jimi Hendrix revolutionized electric guitar playing and is considered one of the greatest guitarists of all time.",
        Source = "Rock History Database"
    }
};

// Validate before sending
if (!questionData.IsValid())
{
    Console.WriteLine("Question validation failed!");
    return;
}

// Send to SignalR hub
var result = await hubConnection.InvokeAsync<object>("NextQuestion", sessionId, questionIndex, questionData);
```

## Response Format

### Success Response
```json
{
  "success": true,
  "questionId": "q1",
  "timeLimit": 30,
  "startTime": "2025-10-02T14:30:00.000Z"
}
```

### Error Responses
```json
// Authorization error
{
  "success": false,
  "error": "Unauthorized or session not found"
}

// Validation error
{
  "success": false,
  "error": "Invalid question data: correct answer must be one of the provided options"
}

// Missing question data
{
  "success": false,
  "error": "Question data is required"
}
```

## Events Broadcast

### To Participants: `NewQuestion`
```json
{
  "sessionId": "session-123",
  "questionIndex": 1,
  "question": {
    "questionId": "q1",
    "questionText": "Which artist sang 'Bohemian Rhapsody'?",
    "options": ["Queen", "The Beatles", "Led Zeppelin", "Pink Floyd"],
    "type": 0,
    "timeLimit": 30,
    "metadata": {
      "difficulty": 3,
      "category": "Classic Rock"
    }
    // Note: correctAnswer is NOT included for participants
  },
  "timeLimit": 30,
  "startTime": "2025-10-02T14:30:00.000Z"
}
```

### To Host: `QuestionStarted`
```json
{
  "sessionId": "session-123",
  "questionIndex": 1,
  "question": {
    "questionId": "q1",
    "questionText": "Which artist sang 'Bohemian Rhapsody'?",
    "options": ["Queen", "The Beatles", "Led Zeppelin", "Pink Floyd"],
    "correctAnswer": "Queen", // Host gets the correct answer
    "type": 0,
    "timeLimit": 30,
    "metadata": {
      "difficulty": 3,
      "category": "Classic Rock",
      "explanation": "Bohemian Rhapsody was written by Freddie Mercury..."
    }
  },
  "timeLimit": 30,
  "startTime": "2025-10-02T14:30:00.000Z",
  "participantCount": 15
}
```

## Answer Submission (Enhanced)

When participants submit answers using `SubmitAnswer`, they now get more detailed responses:

```json
{
  "success": true,
  "isCorrect": true,
  "selectedAnswer": "Queen",
  "correctAnswer": null, // Only shown if answer was incorrect
  "score": 135,
  "baseScore": 100,
  "timeBonus": 35,
  "responseTime": "00:05.23",
  "totalScore": 450,
  "explanation": "Bohemian Rhapsody was written by Freddie Mercury and performed by Queen in 1975."
}
```

## Question Types

| Type | Value | Description | Options Format |
|------|-------|-------------|----------------|
| MultipleChoice | 0 | Standard multiple choice | `["A", "B", "C", "D"]` |
| TrueFalse | 1 | True/False question | `["True", "False"]` |
| Text | 2 | Text input (flexible matching) | `["possible", "answers", "list"]` |

## Validation Rules

1. **Question Text**: 5-500 characters
2. **Options**: 2-6 options required
3. **Correct Answer**: Must exactly match one of the options (case-insensitive)
4. **Time Limit**: 5-300 seconds
5. **Points**: 1-1000 points
6. **Question ID**: Required, unique identifier

## Best Practices

1. **Always validate** question data before sending
2. **Use descriptive question IDs** for tracking
3. **Provide explanations** in metadata for educational value
4. **Set appropriate time limits** based on question complexity
5. **Use consistent categories** for better organization
6. **Test with different question types** during development

## Migration from Old API

### Before (Unsafe)
```javascript
// Old way - no type safety, no validation
const questionData = {
  text: "Some question",
  answers: ["A", "B", "C"],
  correct: 0 // Index-based, error-prone
};

await connection.invoke('NextQuestion', sessionId, index, questionData);
```

### After (Type-Safe)
```javascript
// New way - strongly typed, validated
const questionData = {
  questionId: "q1",
  questionText: "Some question",
  options: ["A", "B", "C"],
  correctAnswer: "A", // Value-based, clear
  type: 0,
  timeLimit: 30,
  points: 100
};

await connection.invoke('NextQuestion', sessionId, index, questionData);
```

This new implementation provides much better developer experience, type safety, and validation! 🎉