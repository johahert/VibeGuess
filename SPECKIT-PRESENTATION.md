# SpecKit: Spec-Driven Development

## 1. SpecKit
**SpecKit** keeps AI-assisted development anchored in specs instead of improvisation.

**What it gives us**
- Clarity before code: every feature starts with a plain-language spec.
- AI that stays on-script: assistants get structured context, so outputs are predictable.
- Living documentation: specs, plans, and tests evolve with the feature.
- Constitution enforcement: TDD, simplicity, and security checks happen automatically.

**VibeGuess outcomes so far**
- 3-day turnaround for Feature 003 (vs. 14-day estimate without SpecKit).
- 95%+ automated test coverage on the first implementation pass.
- Zero major refactors needed.

---

## 2. Core Building Blocks

| Piece | Purpose | Location |
| --- | --- | --- |
| Constitution | Project rules (TDD, simplicity, security) | `.specify/memory/constitution.md` |
| Templates | Consistent specs, plans, tasks, agent context | `.specify/templates/` |
| Scripts | Automate branches, checks, path lookups | `.specify/scripts/powershell/` |
| Feature folders | Single source of truth per feature | `specs/00N-feature-name/` |
| AI prompts | Translate commands into scripted workflows | `.github/prompts/*.prompt.md` |

- Specs stay functional (no implementation details).
- Plans describe the "how" and generate contracts/data models.
- Tasks enforce Red → Green → Refactor.
- Every feature branch mirrors its folder in `specs/`.

---

## 3. Workflow in Four Moves

```mermaidgraph LR
    A[💡 Idea] --> B[/specify]
    B --> C[📄 spec.md]
    C --> D[/plan]
    D --> E[📋 Phase 0 & 1 outputs]
    E --> F[/tasks]
    F --> G[✅ tasks.md]
    G --> H[🔨 Implementation]
    H --> I[🧪 Validation]
    I --> J[🎉 Done]
```
| Step | Command | Output | What we focus on |
| --- | --- | --- | --- |
| 1 | `/specify "Describe the feature"` | `spec.md`, feature branch | Business value, user scenarios, functional requirements |
| 2 | `/plan` | `plan.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md` | Architectural choices, integration points, accepted constraints |
| 3 | `/tasks` | `tasks.md` | Ordered checklist (tests first, implementation second) |
| 4 | `/implement` | Code + tests | Follow the list, keep docs updated, run the validation suite |

**Built-in guardrails**
- Constitution check blocks progress if rules are broken (like TDD/Simplicity).
- `/tasks` refuses to run unless Phase 1 artifacts exist.
- AI context files are refreshed so copilots stay aligned with the plan.

---

## 4. Example of Feature 

**Mission:** Ship a music quiz API with Spotify playback.

- **Spec phase:** Captured user stories, functional requirements, and entities—no tech stacks mentioned.
- **Plan phase:** Chose OAuth PKCE, GPT-4 JSON outputs, mapped the data model, drafted REST contracts (`POST /api/quiz/generate`, `GET /api/playback/devices`, etc.).
- **Tasks phase:** Produced 40 ordered tasks with parallel flags `[P]` and TDD ordering.
- **Implementation:** Followed the checklist, achieved 95%+ coverage, and passed the constitution check with no waivers.


---

## 5. Toolkit Essentials

### Core commands (run in chat/terminal)
```bash
/specify "Summarize the feature"      # Creates branch + spec scaffold
/plan                                   # Generates plan, research, contracts, data model
/tasks                                  # Produces ordered task list (tests before code)
```

### Supporting PowerShell scripts
```powershell
.specify/scripts/powershell/create-new-feature.ps1 -Json "Feature text"
.specify/scripts/powershell/check-task-prerequisites.ps1
.specify/scripts/powershell/update-agent-context.ps1 -AgentType copilot
```
Run `get-feature-paths.ps1` to print every relevant file path for the active feature—handy during reviews.

---

## 6. Impact for project

### Impact
- **Speed:** faster progress from idea to production-ready feature.
- **Quality:** 67 tests written before or during implementation; no surprise rework afterwards.
- **Clarity:** Feature folders double as onboarding guides for new teammates or AI assistants.

### Lessons we’re keeping
- Keep specs functional—strip out tech details as soon as they appear.
- Treat the constitution check as a gate, not a suggestion.
- Refresh AI context right after `/plan` so helpers generate code that matches the blueprint.

### What to do on upcoming features
1. Copy `.specify/` into new repos if you want SpecKit elsewhere.
2. Tailor the constitution and trim templates to match the project’s constraints.
3. Always run `/specify → /plan → /tasks` in order; skipping steps breaks the guardrails.
4. Keep documents living—update spec/plan when requirements shift instead of patching them later.


