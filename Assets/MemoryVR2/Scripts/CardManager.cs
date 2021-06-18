using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public sealed class RangeExAttribute : PropertyAttribute
{
    public readonly int min;
    public readonly int max;
    public readonly int step;

    public RangeExAttribute(int min, int max, int step)
    {
        this.min = min;
        this.max = max;
        this.step = step;
    }
}

[CustomPropertyDrawer(typeof(RangeExAttribute))]
internal sealed class RangeExDrawer : PropertyDrawer
{
    private int value;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var rangeAttribute = (RangeExAttribute)base.attribute;

        if (property.propertyType == SerializedPropertyType.Integer)
        {
            value = EditorGUI.IntSlider(position, label, value, rangeAttribute.min, rangeAttribute.max);

            property.intValue = value % 2 == 0 ? value++ : value;
        }
        else
            EditorGUI.LabelField(position, label.text, "Use Range with float or int.");
    }
}
#endif

[ExecuteInEditMode]
public class CardManager : MonoBehaviour
{
    [Header("Default")]
    public GameObject CardPrefab;
    public List<Material> FaceMaterials;

    [Header("Spawn")]
#if UNITY_EDITOR
    [RangeEx(3, 15, 2)]
#endif
    public int Pairs = 3;
    public bool Spawn;

    [Header("Place")]
#if UNITY_EDITOR
    [Range(2, 4)]
#endif
    public int Rows = 3;
    public bool InitCards;

    private void Update()
    {
        if (Spawn)
        {
            Spawn = false;

            SpawnCards();
            SetCardColor();
        }

        if (InitCards)
        {
            InitCards = false;

            CreateCardGrid();
            CenterCards();
            ShuffleCards();
        }
    }

    public void CreateCardGrid()
    {
        float newX = 0.0f;
        float newY = -0.2f * CardPrefab.transform.localScale.x;
        float newZ = 0.0f;        

        for (int i = 0; i < transform.childCount; i++)
        {
            if (i % (transform.childCount / Rows) == 0)
            {
                newX = 0.0f;
                newY += 0.2f * CardPrefab.transform.localScale.x;
            }
            else
                newX += 0.1f * CardPrefab.transform.localScale.x;

            transform.GetChild(i).localPosition = new Vector3(newX, newY, newZ);
            transform.GetChild(i).localRotation = Quaternion.Euler(270.0f, 0.0f, 180.0f);
        }
    }

    public void CenterCards()
    {
        float newX = transform.GetChild(transform.childCount / Rows - 1).localPosition.x / -2;
        float newY = transform.GetChild(transform.childCount / Rows).localPosition.y / -2;

        transform.localPosition = new Vector3(newX, 1.0f + newY, -0.001f);
    }

    public void ShuffleCards()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, transform.childCount - 1);

            Transform currentCard = transform.GetChild(i);
            Transform otherCard = transform.GetChild(randomIndex);

            Vector3 temp = currentCard.localPosition;

            currentCard.localPosition = otherCard.localPosition;
            otherCard.localPosition = temp;
        }
    }

    private void SpawnCards()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject currentCard = transform.GetChild(i).gameObject;
            DestroyImmediate(currentCard);
        }

        for (int i = 0; i < Pairs * 2; i++)
        {
            GameObject currentCard = Instantiate(CardPrefab);
            currentCard.transform.parent = transform;
            currentCard.transform.localPosition = Vector3.zero;
        }
    }

    private void SetCardColor()
    {
        int count = 0;
        int number = 0;
        int materialIndex = 0;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform currentCard = transform.GetChild(i);

            if (count == 0)
                currentCard.name = "Card_" + number + "_a";
            else
                currentCard.name = "Card_" + number + "_b";

            currentCard.GetChild(0).GetChild(0).GetComponent<Text>().text = number.ToString("00");

            currentCard.GetComponent<NetworkedCard>().FaceMaterial = FaceMaterials[materialIndex];

            if (++count >= 2)
            {
                count = 0;
                number++;
                materialIndex++;

                if (materialIndex >= FaceMaterials.Count)
                    materialIndex = 0;
            }
        }
    }
}
