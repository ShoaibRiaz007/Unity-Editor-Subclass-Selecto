#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif
using UnityEngine;

#if UNITY_EDITOR
[AttributeUsage(AttributeTargets.All)]
#endif
/// <summary>
/// Generic Assign property to its child classes.
/// Must be used with [SerializeReference] attribute.
/// </summary>
public class IsSubClass : PropertyAttribute
{
    public IsSubClass()
    {
#if UNITY_EDITOR
        Debug.LogWarning("IsSubClass attribute should be used together with [SerializeReference]");
#endif
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(IsSubClass))]
public class IsSubClassDrawer : PropertyDrawer
{
    private static Dictionary<Type, List<Type>> typeCache = new();

    private static List<Type> GetSubclasses(Type baseType)
    {
        if (!typeCache.TryGetValue(baseType, out var subclasses))
        {
            subclasses = Assembly.GetAssembly(baseType)
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t))
                .ToList();
            typeCache[baseType] = subclasses;
        }
        return subclasses;
    }

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var container = new VisualElement
        {
            style = { flexDirection = FlexDirection.Row }
        };
        var dropdownButton = new Button(() =>
        {
            Type fieldType = fieldInfo.FieldType;

            if (fieldType == null)
                return;

            var subclasses = GetSubclasses(fieldType);
            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                new SubclassSearchProvider(subclasses, selectedType =>
                {
                    property.serializedObject.Update();
                    property.managedReferenceValue = Activator.CreateInstance(selectedType);
                    property.serializedObject.ApplyModifiedProperties();
                }));
        })
        {
            text = "▼",
            style =
        {
            width = 20,
            height = 20,
            unityTextAlign = TextAnchor.MiddleCenter,
            marginLeft = 5 // Add some spacing between the label and the button
        }
        };
        var propertyField = new PropertyField(property)
        {
            style = { flexGrow = 1, marginLeft = 10 }
        };
        container.Add(propertyField);

        container.Add(dropdownButton);
        return container;
    }

    private string GetTypeName(SerializedProperty property)
    {
        var typeName = property.managedReferenceValue?.GetType().Name ?? "None";
        return $"{typeName}";
    }

    private class SubclassSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        private readonly List<Type> subclasses;
        private readonly Action<Type> onTypeSelected;

        public SubclassSearchProvider(List<Type> subclasses, Action<Type> onTypeSelected)
        {
            this.subclasses = subclasses;
            this.onTypeSelected = onTypeSelected;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Select Type"), 0)
            };

            foreach (var subclass in subclasses)
            {
                tree.Add(new SearchTreeEntry(new GUIContent(subclass.Name))
                {
                    level = 1,
                    userData = subclass
                });
            }

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            if (entry.userData is Type selectedType)
            {
                onTypeSelected(selectedType);
                return true;
            }
            return false;
        }
    }
}
#endif
