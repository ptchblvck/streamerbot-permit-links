using System;

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

        // Build permit global name
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

        // Current Unix timestamp
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

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