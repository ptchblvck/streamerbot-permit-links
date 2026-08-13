## Configure the Global Variables

Create the following five Streamer.bot global variables using:

**Add → Core → Globals → Global (Set)**

| Variable                      | Recommended value | Description                                                                    |
| ----------------------------- | ----------------: | ------------------------------------------------------------------------------ |
| `permitDuration`              |              `60` | Duration of a permit, in seconds.                                              |
| `alwaysPermitLinksForRaiders` |           `false` | Whether raiders should always be allowed to post links.                        |
| `linkSpamLimit`               |               `3` | Number of links a user may send within the spam window before action is taken. |
| `linkSpamWindow`              |              `10` | Time window used to detect link spam, in seconds.                              |
| `linkSpamTimeout`             |              `60` | Timeout duration applied when the link-spam limit is exceeded, in seconds.     |

### Recommended configuration

A reasonable starting configuration is:

```text
permitDuration = 60
alwaysPermitLinksForRaiders = false
linkSpamLimit = 3
linkSpamWindow = 10
linkSpamTimeout = 60
```

You can adjust these values to match the moderation policy of your channel.

`permitDuration` and `linkSpamWindow` should be at least `60` and `10` seconds respectively according to the intended configuration.