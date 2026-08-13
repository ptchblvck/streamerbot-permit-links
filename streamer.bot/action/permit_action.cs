using System;

public class CPHInline
{
    public bool Execute()
    {
        // --------------------------------------------------
        // Get standardized arguments from the calling action
        // --------------------------------------------------

        if (!CPH.TryGetArg("permitUsername", out string username) ||
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

        // Get source
        string source = "command";

        if (CPH.TryGetArg("permitSource", out string sourceArg) &&
            !string.IsNullOrWhiteSpace(sourceArg))
        {
            source = sourceArg.Trim().ToLowerInvariant();
        }


        // --------------------------------------------------
        // Raid permission setting
        // --------------------------------------------------

        // If this permit comes from a raid, check whether
        // automatic raid permits are enabled.
        if (source == "raid")
        {
            // Default to FALSE if the setting doesn't exist.
            bool alwaysPermitLinksForRaiders =
                CPH.GetGlobalVar<bool>("alwaysPermitLinksForRaiders", true);

            if (!alwaysPermitLinksForRaiders)
            {
                return false;
            }
        }


        // --------------------------------------------------
        // Get and validate duration
        // --------------------------------------------------

        // Default comes from the `permitDuration` global so it can be
        // adjusted in Streamer.bot without touching code.
        int duration = CPH.GetGlobalVar<int>("permitDuration", true);

        // Fallback if the global is missing or invalid
        if (duration < 60)
        {
            duration = 60;
        }

        // An explicit duration argument (e.g. `!permit <user> 120`)
        // overrides the global default.
        if (CPH.TryGetArg("permitDuration", out string durationString) &&
            !string.IsNullOrWhiteSpace(durationString))
        {
            if (!int.TryParse(durationString, out duration))
            {
                return false;
            }
        }

        // Minimum permit duration is 60 seconds
        if (duration < 60)
        {
            return false;
        }


        // --------------------------------------------------
        // Create permit
        // --------------------------------------------------

        string globalName = $"permit_link_{username}";

        long expiry = DateTimeOffset.UtcNow
            .AddSeconds(duration)
            .ToUnixTimeSeconds();

        // Persist the permit
        CPH.SetGlobalVar(globalName, expiry, true);

        // Store the source so the monitor can treat raid permits as
        // single-use: a raid permit ends after the raider posts their
        // first link, while a command permit lasts the full window.
        string sourceGlobalName = $"permit_source_{username}";
        CPH.SetGlobalVar(sourceGlobalName, source, true);


        // --------------------------------------------------
        // Confirmation message
        // --------------------------------------------------

        if (source == "raid")
        {
            CPH.SendMessage(
                $"Welcome raid from @{username}! You have {duration} seconds to post links."
            );
        }
        else
        {
            string issuer = "";

            if (CPH.TryGetArg("user", out string userArg) &&
                !string.IsNullOrWhiteSpace(userArg))
            {
                issuer = userArg;
            }

            if (!string.IsNullOrEmpty(issuer))
            {
                CPH.SendMessage(
                    $"@{issuer} permitted @{username} to post links for {duration} seconds."
                );
            }
            else
            {
                CPH.SendMessage(
                    $"@{username} has been permitted to post links for {duration} seconds."
                );
            }
        }

        return true;
    }
}