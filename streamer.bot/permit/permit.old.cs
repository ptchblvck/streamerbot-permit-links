using System;

public class CPHInline
{
    public bool Execute()
    {
        // Get everything after !permit
        string rawInput = args["rawInput"]?.ToString()?.Trim() ?? "";

        // Split input into username + optional duration
        string[] parts = rawInput.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        // Username is required
        if (parts.Length == 0)
        {
            CPH.SendMessage($"@{args["user"]} Usage: !permit <username> [duration in seconds]");
            return false;
        }

        // Get and normalize username
        string username = parts[0]
            .Replace("@", "")
            .Trim()
            .ToLowerInvariant();

        // Make sure username isn't empty after normalization
        if (string.IsNullOrEmpty(username))
        {
            CPH.SendMessage($"@{args["user"]} Usage: !permit <username> [duration in seconds]");
            return false;
        }

        // Default duration: 60 seconds
        int duration = 60;

        // Optional duration
        if (parts.Length >= 2)
        {
            if (!int.TryParse(parts[1], out duration))
            {
                CPH.SendMessage(
                    $"@{args["user"]} Invalid duration. Please use a number of seconds (minimum 60)."
                );
                return false;
            }

            // Minimum duration is 60 seconds
            if (duration < 60)
            {
                CPH.SendMessage(
                    $"@{args["user"]} Duration must be at least 60 seconds."
                );
                return false;
            }
        }

        // Create global name
        string globalName = $"permit_link_{username}";

        // Calculate expiry
        long expiry = DateTimeOffset.UtcNow
            .AddSeconds(duration)
            .ToUnixTimeSeconds();

        // Save persisted global
        CPH.SetGlobalVar(globalName, expiry, true);

        // Confirm
        CPH.SendMessage(
            $"@{args["user"]} permitted @{username} to post links for {duration} seconds."
        );

        return true;
    }
}

