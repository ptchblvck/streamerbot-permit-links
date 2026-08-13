# Create the `!permit` Command

The `!permit` command allows a moderator or other permitted user to manually grant a temporary link permit.

### Trigger

Create a new trigger using:

**Add → Core → Commands → Command Triggered**

Select an existing `!permit` command, or create a new command named:

```text
!permit
```

### Sub-Actions

Create the following sub-actions:

1. **Add → Core → C# → Execute C# Code**

   * Copy the code from [`permit/permit.cs`](./permit/permit.cs).

2. **Add → Core → Actions → Run Action**

   * Select `permit action`.

The C# code handles the command-specific logic, while the shared `permit action` performs the actual permit operation.

### Usage

```text
!permit <username> [duration in seconds]
```

The duration is optional. If omitted, the `permitDuration` global is used. The minimum duration is `60` seconds.

