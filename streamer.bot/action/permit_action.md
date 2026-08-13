# Create the Permit Action

The **Permit Action** contains the central logic used to grant a user permission to post links.

### Triggers

Do not add a trigger.

### Sub-Actions

Create a new sub-action:

1. Select **Add → Core → C# → Execute C# Code**.
2. Copy the code from [`action/permit_action.cs`](./action/permit_action.cs) into the C# action.
3. Save the action as `permit action`.

This action is subsequently called by both the `!permit` command and the raid action.