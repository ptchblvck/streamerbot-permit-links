# Streamer.bot Permit Link Management

A collection of [Streamer.bot](https://streamer.bot/) C# actions for managing Twitch link permissions, including temporary link permits, raid-based permits, and automatic link monitoring.

The system is designed to allow trusted users to post links temporarily without requiring broad, permanent link permissions. It can also automatically permit links for incoming raids and monitor regular chatters for link spam.

## Features

* **`!permit` command** — Temporarily allows a user to post links.
* **Raid permits** — Automatically permits links for users arriving through a Twitch raid.
* **Link monitoring** — Detects links posted by regular chatters and handles them through the permit system.
* **Configurable link-spam protection** — Limits how many links can be posted within a configurable time window and applies a configurable timeout.
* **Role-aware monitoring** — Moderators, VIPs, and the streamer are excluded from regular link monitoring.
* **Always-permitted users** — An easily adjustable list of users who can always post links.
* **Configurable raid behavior** — Optionally permit links indefinitely for raiders.
* **Centralized configuration** — Important settings are stored as Streamer.bot global variables.

## Repository Structure

```text
streamer.bot/
├── action/
│   ├── permit_action.cs
│   └── permit_action.md
├── monitor/
│   ├── permit_monitor_without_linkprotection.cs
│   ├── permit_monitor.cs
│   └── permit_monitor.md
├── permit/
│   ├── permit.cs
│   ├── permit.md
│   └── permit.old.cs
├── raid/
│   ├── permit_raid.cs
│   └── permit_raid.md
├── setup/
│   └── setup.md
├── LICENSE
└── README.md
```

The `.cs` files contain the C# code to be used with Streamer.bot. The individual `.md` files contain setup instructions for the corresponding action.

> **Note:** `permit_monitor_without_linkprotection.cs` is an alternative monitor implementation without the link-spam protection. It is not required for the standard setup described below.

## Requirements

* [Streamer.bot](https://streamer.bot/)
* A Twitch account connected to Streamer.bot
* Twitch moderation permissions sufficient for the bot to perform the required moderation actions
* Streamer.bot configured to receive Twitch chat and raid events

This repository assumes familiarity with the basic Streamer.bot interface, actions, triggers, sub-actions, and C# execution.

---

# Quick Import

If you don't want to build the Streamer.bot actions manually, the repository includes a ready-to-import Streamer.bot export:

[`streamerbot_export.md`](./streamerbot_export.md)

Importing the export is the quickest way to get the complete setup into Streamer.bot.

## Import the Export

1. Open **Streamer.bot**.
2. Open the **Import** functionality.
3. Select [`streamerbot_export.md`](./streamerbot_export.md).
4. Import the actions, commands, and configuration contained in the export.
5. Review the imported actions and make sure the settings match your channel's requirements.
6. Verify the global variables, particularly:

   * `permitDuration`
   * `alwaysPermitLinksForRaiders`
   * `alwaysPermitUsers`
   * `linkSpamLimit`
   * `linkSpamWindow`
   * `linkSpamTimeout`
7. Test `!permit`, link monitoring, and raid handling before using the setup live.

> **Recommended:** The export is the easiest installation method. The manual setup instructions in this README are provided for users who want to understand the individual actions or configure the system themselves.

If the export does not contain a particular setting or you want to customize the setup, use the [manual installation instructions](#installation) instead.

---

# Installation

There are two parts to setting up the system:

1. Configure the global variables.
2. Create the Streamer.bot actions and triggers.

The recommended setup is described below.

## 1. Configure the Global Variables

See [`setup/setup.md`](./streamer.bot/setup/setup.md) for the instructions.

---

# 2. Create the Permit Action

See [`action/permit_action.md`](./streamer.bot/action/permit_action.md) for the instructions.

---

# 3. Create the `!permit` Command

See [`permit/permit.md`](./streamer.bot/permit/permit.md) for the `permit` instructions.

---

# 4. Configure Raid Permits

See [`raid/permit_raid.md`](./streamer.bot/raid/permit_raid.md) for the raid instructions.

---

# 5. Configure the Link Monitor

See [`monitor/permit_monitor.md`](./monitor/permit_monitor.md) for the monitor setup instructions.

---

# How It Works

The system is split into several components that work together.

```text
                         ┌─────────────────┐
                         │   Twitch Chat   │
                         └────────┬────────┘
                                  │
                         Chat Message Trigger
                                  │
                                  ▼
                         ┌─────────────────┐
                         │  Role Switch    │
                         └────────┬────────┘
                                  │
                    ┌─────────────┼─────────────┐
                    │             │             │
                  Streamer       Mod           VIP
                    │             │             │
                    └─────────────┴─────────────┘
                                  │
                              Break
                                  │
                           Regular chatter
                                  │
                                  ▼
                         ┌─────────────────┐
                         │   URL Regex     │
                         └────────┬────────┘
                                  │
                               Link found
                                  │
                                  ▼
                         ┌─────────────────┐
                         │ Permit Monitor  │
                         └────────┬────────┘
                                  │
                                  ▼
                         ┌─────────────────┐
                         │  Permit Action  │
                         └─────────────────┘
```

There are two other ways the shared permit action can be reached:

```text
!permit command
      │
      ▼
 permit.cs
      │
      ▼
permit action


Twitch Raid
     │
     ▼
permit_raid.cs
     │
     ▼
permit action
```

This keeps the actual permit behavior in one shared action rather than duplicating it across the command and raid implementations.

---

# Link Spam Protection

The monitor includes protection against users repeatedly posting links.

Three settings control this behavior:

### `linkSpamLimit`

The maximum number of links allowed within the configured window.

For example:

```text
linkSpamLimit = 3
```

allows three links within the window; the next link after that triggers the configured timeout behavior.

### `linkSpamWindow`

The amount of time, in seconds, over which link messages are counted.

For example:

```text
linkSpamWindow = 10
```

means the monitor looks at link activity occurring within a 10-second period.

### `linkSpamTimeout`

The timeout duration, in seconds, applied when the link-spam threshold is exceeded.

For example:

```text
linkSpamTimeout = 60
```

will result in a 60-second timeout when the configured spam threshold is exceeded.

The values can be changed through the Streamer.bot global variables without modifying the C# source code.

---

# Raider Configuration

Raid behavior is controlled by:

```text
alwaysPermitLinksForRaiders
```

Set it to:

```text
false
```

to use the normal permit behavior.

Set it to:

```text
true
```

if you want links from raiders to be permitted according to the raid-specific logic.

The raid trigger itself can also be configured with a minimum raid size so that small raids do not necessarily invoke the action.

### How raid permits work

When `alwaysPermitLinksForRaiders` is `true`, the raider receives a link permit that lasts up to `permitDuration` seconds (default `300`, i.e. 5 minutes).

Raid permits are **single-use**: the raider's first link is allowed and immediately ends the permit. This is intended for channels that expect raiders to post their art ("art tax") and then be done. If the raider never posts a link, the permit simply expires after `permitDuration`.

`!permit` remains a pure time window — a command-permitted user can keep posting links until the duration expires.

---

# Alternative Monitor

The repository also contains:

[`monitor/permit_monitor_without_linkprotection.cs`](./monitor/permit_monitor_without_linkprotection.cs)

This is an alternative monitor implementation without the link-spam protection.

Use this version if you specifically want link detection without the additional spam-protection behavior.

For normal installations, [`monitor/permit_monitor.cs`](./monitor/permit_monitor.cs) is the recommended monitor.

---

# Customization

Most of the behavior can be changed without editing the C# code.

## Change Permit Duration

Change the `permitDuration` global. This is the default permit length used by `!permit`, raids, and when no explicit duration is given:

```text
permitDuration
```

For example:

```text
permitDuration = 120
```

would configure a 120-second permit duration.

A permit lasts for the full configured duration, so a permitted user can keep posting links until it expires.

## Always-Permitted Users

Users on the `alwaysPermitUsers` list can always post links and are exempt from monitoring and the link-spam protection.

Set it to a comma-separated list of usernames:

```text
alwaysPermitUsers = friend1,friend2
```

For example:

```text
alwaysPermitUsers = mymod,editorbot
```

would allow `mymod` and `editorbot` to post links without a permit. Leave the variable empty to disable the list.

## Change Link Spam Threshold

Change:

```text
linkSpamLimit
linkSpamWindow
linkSpamTimeout
```

For example:

```text
linkSpamLimit = 5
linkSpamWindow = 15
linkSpamTimeout = 120
```

would allow a larger number of links over a longer window before applying a longer timeout.

## Change Raid Behavior

Change:

```text
alwaysPermitLinksForRaiders
```

between:

```text
true
false
```

depending on how you want raiders to be handled.

---

# Troubleshooting

## `!permit` does nothing

Check that:

* The `!permit` command exists in Streamer.bot.
* The command has a **Command Triggered** trigger.
* The trigger contains the C# code from [`permit/permit.cs`](./streamer.bot/permit/permit.cs).
* The action also runs the shared `permit action`.
* The shared `permit action` contains the code from [`action/permit_action.cs`](./streamer.bot/action/permit_action.cs).

## Links are not detected

Check that:

* The monitor uses **Twitch → Chat → Chat Message** as its trigger.
* **Get User Info For Target** runs before the role Switch.
* The Switch input is `%role%`.
* Cases `4`, `3`, and `2` have a Break.
* The URL regular expression is configured correctly.
* The monitor C# code comes from [`monitor/permit_monitor.cs`](./streamer.bot/monitor/permit_monitor.cs).

## Moderators or VIPs are being monitored

Make sure the Switch contains:

```text
4
3
2
```

and that each case contains a **Break** sub-action.

These cases are intended to stop processing for the streamer, moderators, and VIPs.

## Raid permits are not working

Check that:

* The trigger is **Twitch → Raid → Raid**.
* The raid action contains the code from [`raid/permit_raid.cs`](./streamer.bot/raid/permit_raid.cs).
* The raid action runs `permit action`.
* The raid trigger's minimum raid settings are not preventing the action from firing.
* `alwaysPermitLinksForRaiders` is configured as intended.

## Link spam protection behaves unexpectedly

Check the global variables and especially:

```text
linkSpamLimit
linkSpamWindow
linkSpamTimeout
```

Make sure they contain numeric values and that `linkSpamWindow` is at least `10` seconds.

Remember that users with a valid permit or on the `alwaysPermitUsers` list are exempt from the link-spam protection, so spam timeouts only apply to users without a permit.

---

# Individual Documentation

More focused instructions are available alongside each component:

* [`action/permit_action.md`](./streamer.bot/action/permit_action.md) — Shared permit action
* [`permit/permit.md`](./streamer.bot/permit/permit.md) — `!permit` command
* [`monitor/permit_monitor.md`](./streamer.bot/monitor/permit_monitor.md) — Link monitoring
* [`raid/permit_raid.md`](./streamer.bot/raid/permit_raid.md) — Raid permits
* [`setup/setup.md`](./streamer.bot/setup/setup.md) — Global variable configuration

These files are useful if you only need to configure or modify one part of the system.

---

# Security and Moderation Considerations

This project is intended to assist with Twitch chat moderation. Review the behavior carefully before deploying it on a production channel.

In particular:

* Test the configuration with a small `linkSpamTimeout` first.
* Make sure moderators and VIPs are correctly excluded from the monitor.
* Review the URL-matching regular expression if your channel has unusual link formats.
* Test raid behavior before enabling automatic permissions for raiders.
* Keep the Streamer.bot instance and its integrations up to date.
* Do not blindly copy C# code from untrusted modifications of this repository into a production Streamer.bot installation.

---

# Contributing

Contributions and improvements are welcome.

When modifying the project:

1. Keep the individual actions separated by responsibility.
2. Update the relevant `.md` documentation when changing an action's setup requirements.
3. Keep configurable values in Streamer.bot globals where practical.
4. Test changes in a non-production environment before deploying them to a live Twitch channel.

---

# License

This project is licensed under the **MIT License**.

See [`LICENSE`](./LICENSE) for the complete license text.

## MIT License

Copyright (c) the contributors of this project.

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files, to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, subject to the conditions of the MIT License.

The software is provided **"as is"**, without warranty of any kind.

See the repository's [`LICENSE`](./LICENSE) file for the complete terms and conditions.