using System;
using UnityEngine;

namespace Unidice.SDK.Utilities
{
    /// <summary>
    /// Add this attribute to a field with <see cref="SerializeReference"/>, to allow swapping to other types of the base type via the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)] 
    public class SwappableAttribute : PropertyAttribute { }
}