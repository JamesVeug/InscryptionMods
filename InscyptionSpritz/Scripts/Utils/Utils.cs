using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DiskCardGame;
using UnityEngine;
using SpritzMod.Scripts;

namespace SpritzMod
{
    public static class Utils
    {
        public static Texture2D GetTextureFromPath(string path)
        {
            byte[] imgBytes = File.ReadAllBytes(Path.Combine(Plugin.Directory, path));
            Texture2D tex = new Texture2D(2,2);
            tex.LoadImage(imgBytes);
            tex.filterMode = FilterMode.Point;

            return tex;
        }
        
        public static void PrintHierarchy(GameObject go, bool printParents)
        {
            string prefix = "";
            if (printParents)
            {
                List<Transform> hierarchy = new List<Transform>();
                
                Transform t = go.transform.parent;
                while (t != null)
                {
                    hierarchy.Add(t);
                    t = t.parent;
                }

                for (int i = hierarchy.Count - 1; i >= 0; i--)
                {
                    Transform tran = hierarchy[i];
                    string text = prefix + tran.gameObject.name + "(" + tran.gameObject.GetInstanceID() + ")";
                    Plugin.Log.LogInfo(prefix + text);

                    prefix += "\t";
                }
            }

            PrintGameObject(go, prefix);
        }

        private static void PrintGameObject(GameObject go, string prefix = "")
        {
            string text = prefix + go.name + "(" + go.GetInstanceID() + ")";
            Plugin.Log.LogInfo(prefix + text);
            Plugin.Log.LogInfo(prefix + "- Components: " + go.transform.childCount);
            foreach (Component component in go.GetComponents<Component>())
            {
                Plugin.Log.LogInfo(prefix + "-- " + component.GetType());
                if (component is SpriteRenderer spriteRenderer)
                {
                    Plugin.Log.LogInfo(prefix + "-- Name: " + spriteRenderer.name);
                    Plugin.Log.LogInfo(prefix + "-- Sprite Name: " + spriteRenderer.sprite.name);
                }
            }

            Plugin.Log.LogInfo(prefix + "- Children: " + go.transform.childCount);
            for (int i = 0; i < go.transform.childCount; i++)
            {
                PrintGameObject(go.transform.GetChild(i).gameObject, prefix + "- -\t");
            }
        }
        
        /// <summary>
        /// Some cards do not have Card.Slot assigned. So this is a work around
        /// </summary>
        public static CardSlot GetSlot(PlayableCard cardToGetSlot)
        {
            if (cardToGetSlot.Slot != null)
            {
                return cardToGetSlot.Slot;
            }

            CardSlot cardSlot = cardToGetSlot.transform.parent.GetComponent<CardSlot>();
            if (cardSlot != null)
            {
                return cardSlot;
            }

            List<CardSlot> allSlots = new List<CardSlot>();
            allSlots.AddRange(Singleton<BoardManager>.Instance.GetSlots(false));
            allSlots.AddRange(Singleton<BoardManager>.Instance.GetSlots(true));

            for (int i = 0; i < allSlots.Count; i++)
            {
                CardSlot slot = allSlots[i];
                if (slot.Index != 2)
                {
                    continue;
                }
                
                PlayableCard card = slot.Card;
                if (card == null)
                    continue;
                
                if (card.gameObject == cardToGetSlot.gameObject)
                {
                    return slot;
                }
            }

            return null;
        }
        
        public static List<CardSlot> GetAdjacentSlots(CardSlot slot)
        {
            int slotIndex = slot.Index;
            List<CardSlot> allSlots = Singleton<BoardManager>.Instance.GetSlots(!slot.opposingSlot);
            List<CardSlot> slots = new List<CardSlot>();
            for (int i = 0; i < allSlots.Count; i++)
            {
                CardSlot cardSlot = allSlots[i];
                if (cardSlot == null || cardSlot.Card == null || cardSlot.Card.Dead || cardSlot.Card.FaceDown)
                {
                    continue;
                }
                
                if (cardSlot.Index == slotIndex - 1 || cardSlot.Index == slotIndex + 1)
                {
                    slots.Add(cardSlot);
                }
            }

            return slots;
        }

        public static int GetRandomWeight(int totalWeight)
        {
            int randomSeed = SaveManager.SaveFile.GetCurrentRandomSeed() + Singleton<GlobalTriggerHandler>.Instance.NumTriggersThisBattle + 1;
            return SeededRandom.Range(0, totalWeight, randomSeed);
        }

        public static CardInfo GetRandomWeightedCard(List<WeightData> weights, int totalWeight)
        {
            int expectedWeight = Utils.GetRandomWeight(totalWeight);
            int currentWeight = 0;
            for (var i = 0; i < weights.Count; i++)
            {
                WeightData data = weights[i];
                currentWeight += data.weight;
                if (currentWeight >= expectedWeight)
                {
                    return CardLoader.GetCardByName(data.cardName);
                }
            }

            string cardName = weights[weights.Count - 1].cardName;
            return CardLoader.GetCardByName(cardName);
        }
        
        /// <summary>
        /// Returns a _private_ Property Value from a given Object. Uses Reflection.
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <typeparam name="T">Type of the Property</typeparam>
        /// <param name="obj">Object from where the Property Value is returned</param>
        /// <param name="propName">Propertyname as string.</param>
        /// <returns>PropertyValue</returns>
        public static T GetPrivatePropertyValue<T>(this object obj, string propName)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            PropertyInfo pi = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (pi == null) throw new ArgumentOutOfRangeException("propName", string.Format("Property {0} was not found in Type {1}", propName, obj.GetType().FullName));
            return (T)pi.GetValue(obj, null);
        }

        /// <summary>
        /// Returns a private Property Value from a given Object. Uses Reflection.
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <typeparam name="T">Type of the Property</typeparam>
        /// <param name="obj">Object from where the Property Value is returned</param>
        /// <param name="propName">Propertyname as string.</param>
        /// <returns>PropertyValue</returns>
        public static T GetPrivateFieldValue<T>(this object obj, string propName)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            Type t = obj.GetType();
            FieldInfo fi = null;
            while (fi == null && t != null)
            {
                fi = t.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                t = t.BaseType;
            }
            if (fi == null) throw new ArgumentOutOfRangeException("propName", string.Format("Field {0} was not found in Type {1}", propName, obj.GetType().FullName));
            return (T)fi.GetValue(obj);
        }

        /// <summary>
        /// Sets a _private_ Property Value from a given Object. Uses Reflection.
        /// Throws a ArgumentOutOfRangeException if the Property is not found.
        /// </summary>
        /// <typeparam name="T">Type of the Property</typeparam>
        /// <param name="obj">Object from where the Property Value is set</param>
        /// <param name="propName">Propertyname as string.</param>
        /// <param name="val">Value to set.</param>
        /// <returns>PropertyValue</returns>
        public static void SetPrivatePropertyValue<T>(this object obj, string propName, T val)
        {
            Type t = obj.GetType();
            if (t.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) == null)
                throw new ArgumentOutOfRangeException("propName", string.Format("Property {0} was not found in Type {1}", propName, obj.GetType().FullName));
            t.InvokeMember(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, null, obj, new object[] { val });
        }

        /// <summary>
        /// Set a private Property Value on a given Object. Uses Reflection.
        /// </summary>
        /// <typeparam name="T">Type of the Property</typeparam>
        /// <param name="obj">Object from where the Property Value is returned</param>
        /// <param name="propName">Propertyname as string.</param>
        /// <param name="val">the value to set</param>
        /// <exception cref="ArgumentOutOfRangeException">if the Property is not found</exception>
        public static void SetPrivateFieldValue<T>(this object obj, string propName, T val)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            Type t = obj.GetType();
            FieldInfo fi = null;
            while (fi == null && t != null)
            {
                fi = t.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                t = t.BaseType;
            }
            if (fi == null) throw new ArgumentOutOfRangeException("propName", string.Format("Field {0} was not found in Type {1}", propName, obj.GetType().FullName));
            fi.SetValue(obj, val);
        }

        public static GameObject FindObjectInChildren(GameObject parent, string name)
        {
            foreach (Transform child in parent.transform)
            {
                if (child.gameObject.name == name)
                {
                    return child.gameObject;
                }
            }
            
            foreach (Transform child in parent.transform)
            {
                GameObject foundChild = FindObjectInChildren(child.gameObject, name);
                if (foundChild != null)
                {
                    return foundChild;
                }
            }

            return null;
        }
    }
}