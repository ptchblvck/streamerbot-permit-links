using System;
using System.Collections.Generic;

public class CPHInline
{
    public bool Execute()
    {
        // Get message details
        if (!CPH.TryGetArg("user", out string username) || string.IsNullOrWhiteSpace(username))
            return true;

        CPH.TryGetArg("msgId", out string messageId);
        CPH.TryGetArg("message", out string message);

        // Normalize username
        username = username
            .Replace("@", "")
            .Trim()
            .ToLowerInvariant();

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // ============================================================
        // ALWAYS-PERMITTED LIST
        // ============================================================

        // Users listed in the `alwaysPermitUsers` global can always post
        // links and are exempt from monitoring.
        string alwaysPermitList = CPH.GetGlobalVar<string>("alwaysPermitUsers", true);

        if (!string.IsNullOrWhiteSpace(alwaysPermitList))
        {
            foreach (string entry in alwaysPermitList.Split(
                new[] { ',', ' ', ';' },
                StringSplitOptions.RemoveEmptyEntries
            ))
            {
                if (string.Equals(
                    entry.Replace("@", "").Trim(),
                    username,
                    StringComparison.OrdinalIgnoreCase
                ))
                {
                    CPH.LogInfo(
                        $"[LINK PERMIT] {username} is on the always-permitted list."
                    );

                    return false;
                }
            }
        }

        // ============================================================
        // LINK PERMIT
        // ============================================================

        // Build permit global name
        string globalName = $"permit_link_{username}";

        // Get persisted permit
        long? expiry = CPH.GetGlobalVar<long?>(globalName, true);

        // Valid permit — allow the link.
        if (expiry.HasValue && expiry.Value >= now)
        {
            // A raid permit is single-use: once the raider posts their
            // first link (e.g. their art), the permit is consumed.
            // A command permit lasts the full configured window.
            string source = CPH.GetGlobalVar<string>($"permit_source_{username}", true);

            if (source == "raid")
            {
                CPH.UnsetGlobalVar(globalName, true);
                CPH.UnsetGlobalVar($"permit_source_{username}", true);

                CPH.LogInfo(
                    $"[LINK PERMIT] Allowed link from raider {username}; permit consumed."
                );
            }
            else
            {
                CPH.LogInfo(
                    $"[LINK PERMIT] Allowed link from {username}"
                );
            }

            // Stop the rest of the Streamer.bot action.
            // The message is NOT deleted.
            return false;
        }

        // Permit expired — clean it up
        if (expiry.HasValue)
        {
            CPH.UnsetGlobalVar(globalName, true);
            CPH.UnsetGlobalVar($"permit_source_{username}", true);
        }

        // ============================================================
        // NO PERMIT
        // ============================================================

        CPH.TwitchDeleteChatMessage(messageId, true);

        CPH.LogInfo(
            $"[LINK BLOCK] {username} tried to post: {message}"
        );

        CPH.SendMessage(
            $"@{username}, links are not allowed unless you have a permit.",
            true,
            true
        );

        return true;
    }
}
