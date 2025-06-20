using System;
using System.Collections.Generic;

namespace SkillSnap.Client.Services
{
    public class UserSessionService
    {
        public string? UserId { get; set; }
        public string? Role { get; set; }
        public int? CurrentEditingProjectId { get; set; }
        public Dictionary<string, object> State { get; } = new();

        public event Action? OnChange;

        public void SetUser(string? userId, string? role)
        {
            UserId = userId;
            Role = role;
            NotifyStateChanged();
        }

        public void SetCurrentEditingProject(int? projectId)
        {
            CurrentEditingProjectId = projectId;
            NotifyStateChanged();
        }

        public void SetState(string key, object value)
        {
            State[key] = value;
            NotifyStateChanged();
        }

        public void Clear()
        {
            UserId = null;
            Role = null;
            CurrentEditingProjectId = null;
            State.Clear();
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
