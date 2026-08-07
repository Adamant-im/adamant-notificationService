# ADAMANT Notification Service: AI Agent Operating Manual

This document defines how AI agents must work in this repository.

It is intentionally brief and does not cover project-specific technical details (architecture, module map, build/test commands). The current implementation is outdated and due for review, so agents should derive technical context from the current code and tests rather than from a document that would go stale immediately. This file will grow a technical section once that review happens.

## Mission

ADAMANT Notification Service (ANS) delivers push notifications for ADAMANT apps without ANS or Apple learning who is messaging whom. Agent output must optimize for:

1. Security and privacy — device tokens, keys, and message contents must never leak
2. Reliability of the notification delivery pipeline
3. Open-source maintainability and contributor clarity

If a tradeoff is needed, preserve security and privacy first.

## Language Policy

- Developers may communicate with AI in any language
- All repository artifacts must be in English only
- Write all code, comments, commit messages, docs, and PR text in English

## Writing Style

- In bullet and numbered lists, do not add a trailing period when an item contains one sentence
- If an item contains two or more sentences, end every sentence with a period
- For every Markdown list, keep one blank line before the list and one blank line after it
- Keep a blank line between a heading and the list that follows it

## Sources of Truth

Use these sources when implementing or reviewing changes:

- This repository: `README.md`, current code, and passing tests
- ADAMANT Node guidelines baseline: <https://github.com/Adamant-im/adamant/blob/dev/AGENTS.md>
- Org-wide issue/label governance: <https://github.com/Adamant-im/.github>
- Recommended issue title prefixes: <https://github.com/orgs/Adamant-im/discussions/5>
- Recommended labels for issues/discussions: <https://github.com/orgs/Adamant-im/discussions/1>
- ADAMANT docs: <https://docs.adamant.im>
- Node/API schema: <https://schema.adamant.im> and <https://github.com/Adamant-im/adamant-schema>
- AIPs: <https://aips.adamant.im> and <https://github.com/Adamant-im/AIPs>

If sources disagree, treat current repository behavior and passing tests as implementation truth, and document the mismatch instead of silently changing behavior.

## Issue, Label, and PR Conventions

Follow the organization-wide conventions:

- Governance repository: <https://github.com/Adamant-im/.github>
- Prefix guidance: <https://github.com/orgs/Adamant-im/discussions/5>
- Label guidance: <https://github.com/orgs/Adamant-im/discussions/1>

### Issue workflow

1. Search existing issues first to avoid duplicates
2. Use the org issue forms (Bug / Feature request / Task) where they fit
3. Use a concise, prefixed title
4. Apply labels from the org label catalog (`Adamant-im/.github/labels.json`)
5. Link related issues and PRs explicitly

### Title prefixes

Use one or two prefixes maximum.

- `[Bug]` — bug, crash, wrong behavior
- `[Feat]` — new functionality
- `[Enhancement]` — improvement of existing functionality
- `[Refactor]` — internal refactoring without behavior change
- `[Docs]` — documentation updates
- `[Test]` — testing work
- `[Chore]` — maintenance and routine technical tasks
- `[Task]` — general task, including non-coding work
- `[Composite]` — multi-part task with sub-tasks
- `[UX/UI]` — user experience or interface work
- `[Proposal]`, `[Idea]`, `[Discussion]` — idea-level, usually better suited to Discussions

### Label policy

- `labels.json` in `Adamant-im/.github` is the source of truth for label names, colors, and descriptions
- Apply a minimal but informative set: one type/status label (for example `bug`, `enhancement`, `Task`) plus one or more domain labels (for example `Security`, `Nodes`, `Mobile`)
- Add an optional priority label (for example `High priority`) only when it is actually urgent

### PR conventions

- Use the org PR template sections (`Description`, `Related issue`, `How to test`, `Checklist`, etc.)
- Reference issues with closing keywords where applicable (`Closes #<id>`)
- Use `Type: Short summary` for PR titles (for example: `Docs: Add AGENTS.md`)
- Do not use issue-style square-bracket prefixes in PR titles — those are for Issues only
- Keep the PR title type aligned with the issue intent (`Docs:`, `Fix:`, `Feat:`, `Refactor:`, `Test:`, `Chore:`)
- Include testing/verification steps and call out risk areas (security, privacy, data integrity)

## Security and Privacy Rules

- Never log, print, or expose device tokens, passphrases, private keys, or decrypted message contents
- Keep input validation strict on all public and inter-service boundaries
- Do not introduce dynamic code execution, unsafe deserialization, or unvalidated shell execution paths
- Minimize new dependencies, especially cryptography and networking dependencies
- Do not add analytics, telemetry, or tracking that could deanonymize users

## Documentation Drift Policy

AI agents are allowed and expected to propose documentation updates when mismatches are found.

1. Document the mismatch with exact file/reference pointers
2. Propose a synchronized fix across code, this repo's docs, and related ADAMANT docs/spec repos when relevant
3. If a cross-repo change cannot be done immediately, open a linked follow-up issue with clear scope

## AI Change Workflow

1. Read the relevant modules end-to-end before editing
2. Identify invariants that must stay unchanged
3. Make the smallest safe change; prefer targeted fixes over broad rewrites in this outdated codebase
4. Add or update tests
5. Run the validation commands available in this repository and report exactly what was run
6. Report risks, assumptions, and remaining gaps

## Working with Command-Line Tools

When a CLI tool accepts multi-line input, use a temporary file instead of an inline multi-line shell string.

- Prefer file-based flags such as `gh pr create --body-file`, `gh issue create --body-file`, and `git commit -F`
- This avoids shell-escaping issues and is easier to review than inline heredocs

## Definition of Done

A change is done only when all of the following hold:

- Security and privacy priorities remain intact or improved
- Relevant validation commands were run, or a blocker is explicitly reported
- All repository artifacts are in English
- Documentation is updated for any behavioral change

## When to Escalate to Maintainers

Stop and request human review before proceeding if:

- The change affects notification delivery guarantees, encryption, or how device tokens are stored or transmitted
- Behavior differs between environments (for example sandbox vs. production push credentials)
- You cannot prove the change preserves user privacy guarantees
