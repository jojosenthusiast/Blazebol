using Microsoft.AspNetCore.Http;
using System;

namespace ControlTorneosFutbol.Helpers
{
    public static class SessionHelper
    {
        private const string UserIdKey = "UserId";
        private const string UserRoleKey = "UserRole";
        private const string UserNameKey = "UserName";
        private const string UserEmailKey = "UserEmail";

        public static void SetUserSession(this ISession session, Guid userId, string role, string name, string email)
        {
            session.SetString(UserIdKey, userId.ToString());
            session.SetString(UserRoleKey, role);
            session.SetString(UserNameKey, name);
            session.SetString(UserEmailKey, email);
        }

        public static Guid? GetUserId(this ISession session)
        {
            var userIdString = session.GetString(UserIdKey);
            return Guid.TryParse(userIdString, out var userId) ? userId : null;
        }

        public static string? GetUserRole(this ISession session)
        {
            return session.GetString(UserRoleKey);
        }

        public static string? GetUserName(this ISession session)
        {
            return session.GetString(UserNameKey);
        }

        public static string? GetUserEmail(this ISession session)
        {
            return session.GetString(UserEmailKey);
        }

        public static bool IsAuthenticated(this ISession session)
        {
            return session.GetUserId().HasValue;
        }

        public static void ClearUserSession(this ISession session)
        {
            session.Clear();
        }

        public static bool IsInRole(this ISession session, string role)
        {
            var userRole = session.GetUserRole();
            return !string.IsNullOrEmpty(userRole) && userRole.Equals(role, StringComparison.OrdinalIgnoreCase);
        }
    }
}