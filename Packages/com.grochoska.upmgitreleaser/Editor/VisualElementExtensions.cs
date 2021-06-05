using UnityEngine.UIElements;

namespace Editor
{
    public static class VisualElementExtensions
    {
        public static void SetVisibility(this VisualElement element, bool visible)
        {
            if(visible)
                element.Show();
            else element.Hide();
        }
        
        public static void Show(this VisualElement element)
        {
            element.style.display = DisplayStyle.Flex;
        }
        
        public static void Hide(this VisualElement element)
        {
            element.style.display = DisplayStyle.None;
        }
    }
}
