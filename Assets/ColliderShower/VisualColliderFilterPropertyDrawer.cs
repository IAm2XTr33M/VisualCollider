using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(VisualColliderFilter))]
public class VisualColliderFilterPropertyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var container = new VisualElement();

        var filterModeField = new PropertyField(property.FindPropertyRelative("filterMode"));
        container.Add(filterModeField);

        PropertyField positionField = CreatePropertyField(property.FindPropertyRelative("positionMode"));
        PropertyField isActiveField = CreatePropertyField(property.FindPropertyRelative("isActive"));
        PropertyField excludeField = CreatePropertyField(property.FindPropertyRelative("exclude"));

        bool first = true;

        isActiveField.RegisterValueChangeCallback((x) =>
        {
            Write();
        });
        positionField.RegisterValueChangeCallback((x) =>
        {
            Write();
        });
        filterModeField.RegisterValueChangeCallback((x) =>
        {
            Write();
        });
        excludeField.RegisterValueChangeCallback((x) =>
        {
            Write();
        });


        void Write()
        {
            container.Clear();

            SerializedObject serializedObject = property.serializedObject;
            SerializedProperty listProperty = serializedObject.FindProperty("VisualColliderFilters");

            if (listProperty != null)
            {
                int index = GetIndexOfPropertyInList(listProperty, property);

                Label title = new Label();
                title.text = "filter: " + (index + 1).ToString();
                title.style.alignSelf = Align.Center;
                container.Add(title);
            }


            container.Add(filterModeField);

            var propertyType = property.FindPropertyRelative("filterMode");
            var mode = (VisualColliderFilter.FilterMode)propertyType.enumValueIndex;

            var activeType = property.FindPropertyRelative("isActive");
            var isActivated = (bool)activeType.boolValue;

            if (first)
            {
                activeType.boolValue = true;
                isActivated = true;
                first = false;
            }

            if (isActivated)
            {
                switch (mode)
                {
                    case VisualColliderFilter.FilterMode.layer:
                        container.Add(CreatePropertyField(property.FindPropertyRelative("layer")));
                        break;
                    case VisualColliderFilter.FilterMode.tag:
                        container.Add(CreatePropertyField(property.FindPropertyRelative("tag")));
                        container.Add(CreatePropertyField(property.FindPropertyRelative("nameMode")));
                        container.Add(CreatePropertyField(property.FindPropertyRelative("CapitalSensitive")));
                        break;
                    case VisualColliderFilter.FilterMode.mesh:
                        container.Add(CreatePropertyField(property.FindPropertyRelative("mesh")));
                        break;
                    case VisualColliderFilter.FilterMode.prefab:
                        container.Add(CreatePropertyField(property.FindPropertyRelative("prefab")));
                        break;
                    case VisualColliderFilter.FilterMode.name:
                        container.Add(CreatePropertyField(property.FindPropertyRelative("name")));
                        container.Add(CreatePropertyField(property.FindPropertyRelative("nameMode")));
                        container.Add(CreatePropertyField(property.FindPropertyRelative("CapitalSensitive")));
                        break;
                    case VisualColliderFilter.FilterMode.position:

                        var positionModeType = property.FindPropertyRelative("positionMode");
                        var positionMode = (VisualColliderFilter.PositionMode)positionModeType.enumValueIndex;

                        container.Add(positionField);
                        container.Add(CreatePropertyField(property.FindPropertyRelative("positionAxis")));
                        if(positionMode != VisualColliderFilter.PositionMode.InBetween)
                        {
                            container.Add(CreatePropertyField(property.FindPropertyRelative("position")));
                        }
                        else
                        {
                            container.Add(CreatePropertyField(property.FindPropertyRelative("positionInbetween")));
                        }
                        break;
                }
            }

            var excludetype = property.FindPropertyRelative("exclude");
            var isExcluded = (bool)excludetype.boolValue;

            if (!isExcluded)
            {
                PropertyField color = CreatePropertyField(property.FindPropertyRelative("materialColor"));
                container.Add(color);
            }

            container.Add(excludeField);
            container.Add(isActiveField);

            var line = new VisualElement();

            line.style.backgroundColor = new StyleColor(Color.gray);
            line.style.height = 4;
            line.style.marginBottom = 5;
            line.style.marginTop = 5;

            container.Add(line);
        }

        return container;
    }

    PropertyField CreatePropertyField(SerializedProperty property)
    {
        var propertyField = new PropertyField(property);
        propertyField.BindProperty(property);
        return propertyField;
    }

    int GetIndexOfPropertyInList(SerializedProperty listProperty, SerializedProperty targetProperty)
    {
        for (int i = 0; i < listProperty.arraySize; i++)
        {
            SerializedProperty elementProperty = listProperty.GetArrayElementAtIndex(i);
            if (SerializedProperty.EqualContents(elementProperty, targetProperty))
            {
                return i;
            }
        }

        return -1;
    }
}
