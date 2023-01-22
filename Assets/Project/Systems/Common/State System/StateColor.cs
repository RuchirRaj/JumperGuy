using UnityEngine;

namespace RR.StateSystem
{
    public static class StateColor
    {
        public static readonly Color EnabledColor = new Color(0.64f, 1f, 0.64f);
        public static readonly Color DisabledColor = new Color(1, 1f, 1);
        public static readonly Color BlockedColor = new Color(1f, 0.5f, 0.48f);
        public static readonly Color BlockedDisabledColor = new Color(1f, 0.8f, 0.8f);
        public static readonly Color BlockingStateColor = new Color(1f, 0.41f, 0.4f);
        public static readonly Color EmptyStateColor = new Color(1f, 0.95f, 0.65f);
        public static readonly Color ValidStateColor = new Color(0.9f, 0.94f, 1f);
        public static readonly Color RenameButtonColor = new Color(0.73f, 0.88f, 1f);
    }
}