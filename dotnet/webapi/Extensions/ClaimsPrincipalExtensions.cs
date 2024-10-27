﻿using System.Security.Claims;

namespace WebApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetUsername(this ClaimsPrincipal user)
    {
        var username = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new Exception("Cannot get username from token.");

        return username;
    }
}