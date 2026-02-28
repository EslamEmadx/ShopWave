using System.Security.Claims;

namespace backend.Helpers;

/// <summary>
/// Shared helper for safely extracting user identity claims from JWT tokens.
/// Replaces duplicated GetUserId() methods across controllers.
/// </summary>
public static class ClaimsHelper
{
    /// <summary>
    /// Attempts to extract the UserId (ClaimTypes.NameIdentifier) from the claims principal.
    /// Returns null if the claim is missing or the value is not a valid integer.
    /// </summary>
    public static int? TryGetUserId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null)
            return null;

        return int.TryParse(claim.Value, out var userId) ? userId : null;
    }
}
