using System;
using UnityEngine;

namespace Unidice.SDK.Utilities
{
    /// <summary>
    ///     Add this attribute to a field to replace it with a button in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ButtonAttribute : PropertyAttribute
    {
        public ButtonAttribute(string methodName)
        {
            MethodName = methodName;
        }

        public ButtonAttribute(string methodName, params object[] parameters)
        {
            MethodName = methodName;
            Parameter = parameters;
        }

        public object[] Parameter { get; }
        public string MethodName { get; }
    }
}