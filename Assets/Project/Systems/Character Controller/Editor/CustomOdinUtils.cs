namespace Project.Systems.Gameplay.Editor
{
    public static class CustomOdinUtils
    {
        public static string GetPropertyName(string name)
        {
            return $"<{name}>k__BackingField";
        }
    }
}