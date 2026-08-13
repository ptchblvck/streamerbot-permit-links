## Configure the Global Variables

Create the following six Streamer.bot global variables using:

**Add → Core → Globals → Global (Set)**

| Variable                      | Recommended value | Description                                                                    |
| ----------------------------- | ----------------: | ------------------------------------------------------------------------------ |
| `permitDuration`              |             `300` | Default duration of a permit, in seconds. Used by `!permit`, raids, and as a fallback when no explicit duration is given. Minimum `60`. |
| `alwaysPermitLinksForRaiders` |            `true` | Whether raiders should automatically receive a link permit when a raid is received. |
| `alwaysPermitUsers`           |                 | Comma-separated list of usernames that are always allowed to post links (e.g. `friend1,friend2`). Leave empty to disable. |
| `linkSpamLimit`               |               `3` | Number of links a user may send within the spam window before action is taken. The next link after this limit triggers the timeout. |
| `linkSpamWindow`              |              `10` | Time window used to detect link spam, in seconds. Minimum `10`.               |
| `linkSpamTimeout`             |              `60` | Timeout duration applied when the link-spam limit is exceeded, in seconds.     |

### Recommended configuration

A reasonable starting configuration is:

```text
permitDuration = 300
alwaysPermitLinksForRaiders = true
alwaysPermitUsers =
linkSpamLimit = 3
linkSpamWindow = 10
linkSpamTimeout = 60
```

You can adjust these values to match the moderation policy of your channel.

`permitDuration` and `linkSpamWindow` should be at least `60` and `10` seconds respectively according to the intended configuration.

Users with a valid (unexpired) permit or on the `alwaysPermitUsers` list are exempt from the link-spam protection.