using SerializedPropertyType = UnityEditor.SerializedPropertyType;
using SerializedProperty = UnityEditor.SerializedProperty;

namespace ReferenceFinder.Extensions
{
    /// <summary>
    /// Static class with extension methods related to the SerializedProperty class.
    /// </summary>
    public static class SerializedPropertyExtensions
    {
        /// <summary>
        /// Extension method that checks if the serialized property is an
        /// object reference.
        /// </summary>
        /// <param name="serializedProperty">
        /// SerializedProperty to check if is object reference
        /// </param>
        /// <returns>
        /// True if it is an object reference, false otherwise
        /// </returns>
        public static bool IsObjectReference(this SerializedProperty serializedProperty)
        {
            return serializedProperty.propertyType == SerializedPropertyType.ObjectReference;
        }
        
        /// <summary>
        /// Extension method that checks if the serialized property is a
        /// <see cref="UnityEvent"/>.
        /// </summary>
        /// <param name="serializedProperty">
        /// SerializedProperty to check if is unity event
        /// </param>
        /// <returns>
        /// True if it is a <see cref="UnityEvent"/>, false otherwise
        /// </returns>
        public static bool IsUnityEvent(this SerializedProperty serializedProperty)
        {
            return serializedProperty.propertyType == SerializedPropertyType.Generic
                   && serializedProperty.type == "UnityEvent";
        }
    }
}
