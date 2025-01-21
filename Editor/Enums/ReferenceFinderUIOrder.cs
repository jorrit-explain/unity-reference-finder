using System;

namespace ReferenceFinder.Enums
{
    /// <summary>
    /// Helper enum with the order of each part of the tool. 
    /// </summary>
    [Serializable]
    public enum ReferenceFinderUIOrder
    {
        Toolbar = 0,
        SearchTypeEnumField = 1,
        ObjectField = 2,
        FindReferencesButton = 3,
        ReferencesLists = 4,
    }
}
