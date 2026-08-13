using System;
using System.Collections.Generic;

public class CPHInline
{
    public bool Execute()
    {
        // ============================================================
        // GET MESSAGE DETAILS
        // ============================================================

        if (!CPH.TryGetArg("user", out string username) || string.IsNullOrWhiteSpace(username))
            return true;

        CPH.TryGetArg("msgId", out string messageId);
        CPH.TryGetArg("message", out string message);

        // username normalization to have it work with @ and without @
        username = username
            .Replace("@", "")
            .Trim()
            .ToLowerInvariant();

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // ============================================================
        // ALWAYS-PERMITTED LIST
        // ============================================================

        // Users listed in the `alwaysPermitUsers` global can always post
        // links and are exempt from monitoring and spam protection.
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
        // LINK SPAM PROTECTION
        // ============================================================

        // Only users without a valid permit or whitelist entry reach here.
        int linkLimit = CPH.GetGlobalVar<int>("linkSpamLimit", true);
        int windowSeconds = CPH.GetGlobalVar<int>("linkSpamWindow", true);
        int timeoutSeconds = CPH.GetGlobalVar<int>("linkSpamTimeout", true);

        // Safety fallback if a setting has not been configured
        if (linkLimit <= 0)
        {
            linkLimit = 3;
        }

        if (windowSeconds <= 0)
        {
            windowSeconds = 10;
        }

        if (timeoutSeconds <= 0)
        {
            timeoutSeconds = 60;
        }

        string spamGlobalName = $"link_spam_{username}";

        // Get existing link timestamps.
        // Non-persisted because spam tracking does not need to survive
        // a Streamer.bot restart.
        List<long> linkTimes =
            CPH.GetGlobalVar<List<long>>(spamGlobalName, false)
            ?? new List<long>();

        // Remove timestamps older than the spam window
        linkTimes.RemoveAll(timestamp => timestamp <= now - windowSeconds);

        // Count link attempt for the current link sent
        linkTimes.Add(now);

        // Save updated timestamps
        CPH.SetGlobalVar(
            spamGlobalName,
            linkTimes,
            false
        );

        // linkLimit is the number of links allowed within the window;
        // the next link after that triggers the timeout.
        if (linkTimes.Count > linkLimit)
        {
            CPH.TwitchDeleteChatMessage(messageId, true);

            CPH.LogInfo(
                $"[LINK SPAM] {username} sent {linkTimes.Count} links within {windowSeconds} seconds. Timing out for {timeoutSeconds} seconds."
            );

            // attempt timeout
            bool timeoutResult = CPH.TwitchTimeoutUser(
                username,
                timeoutSeconds,
                "Link spam",
                true
            );

            CPH.LogInfo(
                $"[LINK SPAM] Timeout result for {username}: {timeoutResult}"
            );

            // Reset spam counter
            CPH.UnsetGlobalVar(spamGlobalName, false);

            return true;
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
