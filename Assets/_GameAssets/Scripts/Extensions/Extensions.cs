using UnityEngine;



namespace SpeedyBoat
{
    public static class Extensions
    {
        public static Color32 ToColor32(this Color color)
        {
            return new Color32((byte)(color.r * 255), (byte)(color.g * 255), (byte)(color.b * 255), (byte)(color.a * 255));
        }



        public static Color MoveTowards(this Color color, Color targetColor, float amount)
        {
            var val = Vector4.MoveTowards(new Vector4(color.r, color.g, color.b, color.a), new Vector4(targetColor.r, targetColor.g, targetColor.b, targetColor.a), amount);
            return new Color(val.x, val.y, val.z, val.w);
        }



        // Unitys version will only find in the immediate parent, this searches continuously
        public static T GetComponentInParents<T>(this Transform t) where T : Component
        {
            var p = t;
            while (p != null)
            {
                var component = p.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
                p = p.parent;
            }
            return null;
        }



        // Usage example 1: transform.FindIncludingInactive("MyObjectName")
        // Usage example 2: transform.FindIncludingInactive("MyObjectParentsName/MyObjectName")
        public static Transform FindIncludingInactive(this Transform parent, string name)
        {
            if (name.Contains("/"))
            {
                Transform result = parent;
                var pathParts = name.Split(new[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (var pathPart in pathParts)
                {
                    result = result.FindIncludingInactive_Internal(pathPart);
                    if (result == null)
                    {
                        break;
                    }
                }
                return result;
            }

            return FindIncludingInactive_Internal(parent, name);
        }



        private static Transform FindIncludingInactive_Internal(this Transform parent, string name)
        {
            for (int i = 0; i < parent.childCount; ++i)
            {
                var child = parent.GetChild(i);
                if (child.name == name)
                {
                    return child;
                }

                var result = child.FindIncludingInactive_Internal(name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}