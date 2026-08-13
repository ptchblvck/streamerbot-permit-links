# Configure Raid Permits

The raid action can automatically provide link permissions to users arriving through a Twitch raid.

### Trigger

Create a new trigger using:

**Add → Twitch → Raid → Raid**

Adjust the raid trigger parameters according to your preferences. A minimum raid size can be configured if you do not want the action to run for every raid.

### Sub-Actions

Create the following sub-actions:

1. **Add → Core → C# → Execute C# Code**

   * Copy the code from [`raid/permit_raid.cs`](./raid/permit_raid.cs).

2. **Add → Core → Actions → Run Action**

   * Select `permit action`.

The `alwaysPermitLinksForRaiders` global controls whether raiders should receive special link permissions.

The duration of a raid permit is taken from the `permitDuration` global.

Raid permits are **single-use**: the raider's first posted link is allowed and immediately ends the permit. If they never post a link, the permit expires after `permitDuration`.