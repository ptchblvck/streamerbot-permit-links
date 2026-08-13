using System;

public class CPHInline
{
    public bool Execute()
    {
        // Get the raider's username
        if (!CPH.TryGetArg("user", out string username) ||
            string.IsNullOrWhiteSpace(username))
        {
            return false;
        }

        // Normalize username
        username = username
            .Replace("@", "")
            .Trim()
            .ToLowerInvariant();

        if (string.IsNullOrEmpty(username))
        {
            return false;
        }

        // Prepare standardized arguments for the central permit action
        CPH.SetArgument("permitUsername", username);
        CPH.SetArgument("permitDuration", 60);
        CPH.SetArgument("permitSource", "raid");

        return true;
    }
}