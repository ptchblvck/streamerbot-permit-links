# Configure the Link Monitor

The monitor watches Twitch chat for links posted by regular chatters.

### Trigger

Create a new trigger:

**Add → Twitch → Chat → Chat Message**

### Get the User's Role

Add:

**Add → Twitch → User → Get User Info For Target**

Then add:

**Add → Core → Logic → Switch**

Set the `Input` of the Switch to:

```text
%role%
```

The role value is used to prevent the monitor from treating trusted channel roles as ordinary chatters.

### Add Role Cases

Create Switch cases for:

```text
4
3
2
```

These correspond to:

| Role      | Value |
| --------- | ----: |
| Streamer  |   `4` |
| Moderator |   `3` |
| VIP       |   `2` |

Add a **Break** to each of these cases:

**Add → Core → Logic → Break**

This prevents the link-monitoring logic from continuing for the streamer, moderators, and VIPs.

### Configure the Default Case

In the `default` case, add:

**Add → Core → Logic → If/Else**

Configure it as follows:

| Setting   | Value           |                    |                                       |
| --------- | --------------- | ------------------ | ------------------------------------- |
| Input     | `%message%`     |                    |                                       |
| Operation | `Regex Match`   |                    |                                       |
| Auto Type | Enabled         |                    |                                       |
| Value     | `\b(?:https?:// | [www](http://www). | [A-Za-z0-9-]+.[A-Za-z]{2,})(?:/\S*)?` |

The regular expression detects common URLs and domain-style links in chat messages.

### Handle Detected Links

In the **True Result** of the If/Else block, add:

**Add → Core → C# → Execute C# Code**

Copy the code from:

[`monitor/permit_monitor.cs`](./monitor/permit_monitor.cs)

Optionally, add a sound notification:

**Add → Core → Sounds → Play Sound**

and select the sound file you want Streamer.bot to play.