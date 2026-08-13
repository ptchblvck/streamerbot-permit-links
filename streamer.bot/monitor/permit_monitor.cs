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
        // LINK SPAM SETTINGS
        // ============================================================

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



        // ============================================================
        // LINK SPAM PROTECTION
        // ============================================================

        string spamGlobalName = $"link_spam_{username}";

        // Get existing link timestamps.
        // Non-persisted because spam tracking does not need to survive
        // a Streamer.bot restart. (it also would just block a lot of variable slots and that wouldn't really be efficient)
        List<long> linkTimes =
            CPH.GetGlobalVar<List<long>>(spamGlobalName, false)
            ?? new List<long>();

        // Remove timestamps older than the 10-second window
        linkTimes.RemoveAll(timestamp => timestamp <= now - windowSeconds);

        // Count link attempt for the current link sent
        linkTimes.Add(now);

        // Save updated timestamps
        CPH.SetGlobalVar(
            spamGlobalName,
            linkTimes,
            false
        );

        // 3 links within 10 seconds = timeout (that's the default. this won't apply if you setup different things in the setup section)
        if (linkTimes.Count >= linkLimit)
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
        // LINK PERMIT
        // ============================================================

        string globalName = $"permit_link_{username}";

        // Get persisted permit
        long? expiry = CPH.GetGlobalVar<long?>(globalName, true);

        // No permit exists
        if (!expiry.HasValue)
        {
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

        // Permit expired
        if (expiry.Value < now)
        {
            CPH.UnsetGlobalVar(globalName, true);

            CPH.TwitchDeleteChatMessage(messageId, true);

            CPH.LogInfo(
                $"[LINK BLOCK] {username} tried to post with an expired permit: {message}"
            );

            CPH.SendMessage(
                $"@{username}, links are not allowed unless you have a permit.",
                true,
                true
            );

            return true;
        }

        // Valid permit — consume it
        CPH.UnsetGlobalVar(globalName, true);

        CPH.LogInfo(
            $"[LINK PERMIT] Allowed link from {username}"
        );

        // Stop the rest of the Streamer.bot action.
        // The message is NOT deleted.
        return false;
    }
}