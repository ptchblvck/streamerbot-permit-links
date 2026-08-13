using System;

public class CPHInline
{
    public bool Execute()
    {
        // Get everything after !permit
        if (!CPH.TryGetArg("rawInput", out string rawInput))
        {
            CPH.SendMessage($"@{args["user"]} Usage: !permit <username> [duration in seconds]");
            return false;
        }

        rawInput = rawInput?.Trim() ?? "";

        // Split into username + optional duration
        string[] parts = rawInput.Split(
            new[] { ' ' },
            StringSplitOptions.RemoveEmptyEntries
        );

        // Username is required
        if (parts.Length == 0)
        {
            CPH.SendMessage($"@{args["user"]} Usage: !permit <username> [duration in seconds]");
            return false;
        }

        // We only allow username + optional duration
        if (parts.Length > 2)
        {
            CPH.SendMessage(
                $"@{args["user"]} Usage: !permit <username> [duration in seconds]"
            );
            return false;
        }

        // Get username
        string username = parts[0]
            .Replace("@", "")
            .Trim()
            .ToLowerInvariant();

        if (string.IsNullOrEmpty(username))
        {
            CPH.SendMessage(
                $"@{args["user"]} Usage: !permit <username> [duration in seconds]"
            );
            return false;
        }

        // Default duration
        int duration = 60;

        // Optional duration
        if (parts.Length == 2)
        {
            if (!int.TryParse(parts[1], out duration))
            {
                CPH.SendMessage(
                    $"@{args["user"]} Invalid duration. Please use a number of seconds (minimum 60)."
                );
                return false;
            }

            // Minimum duration
            if (duration < 60)
            {
                CPH.SendMessage(
                    $"@{args["user"]} Duration must be at least 60 seconds."
                );
                return false;
            }
        }

        // Prepare standardized arguments for the central permit action
        CPH.SetArgument("permitUsername", username);
        CPH.SetArgument("permitDuration", duration);
        CPH.SetArgument("permitSource", "command");

        return true;
    }
}