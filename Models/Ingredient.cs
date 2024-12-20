using System;
using Terraria;

namespace CookingOverhaul.Models
{
    /// <summary>
    ///     Represents food categories of food items, including their type and possible sources.
    ///     Use <see cref="FoodCategories.Types"/> and <see cref="FoodCategories.Sources"/> for predefined constants.
    /// </summary>
    public struct FoodCategories
    {
        /// <summary>
        ///     Contains predefined constants for the types of food, such as Meat, Vegetable, and Fruit.
        /// </summary>
        public struct Types
        {
            public const string Meat = "Meat";
            public const string Vegetable = "Vegetable";
            public const string Fruit = "Fruit";
            public const string Dairy = "Dairy";
            public const string Grain = "Grain";
        }

        /// <summary>
        ///     Contains predefined constants for the sources of food, such as Fishing, Farming, and Foraging.
        /// </summary>
        public struct Sources
        {
            public const string Fishing = "Fishing";
            public const string Farming = "Farming";
            public const string Foraging = "Foraging";
            public const string Crafting = "Crafting";
        }
    }

    /// <summary>
    ///     Represents an ingredient used in cooking, with details about its item, category, and source.
    ///     This struct is immutable and supports value-based equality.
    /// </summary>
    /// <remarks>
    ///     Use the <see cref="Ingredient"/> struct to represent individual ingredients for cooking. 
    ///     The <see cref="FoodCategories.Types"/> and <see cref="FoodCategories.Sources"/> provide predefined values 
    ///     for the <see cref="Category"/> and <see cref="Source"/> properties, respectively.
    /// </remarks>
    public readonly struct Ingredient(Item item, string category, string source) : IEquatable< Ingredient >
    {
        /// <summary>
        ///     The in-game item that represents this ingredient.
        /// </summary>
        public readonly Item Item { get; init; } = item;

        /// <summary>
        ///     The category of this ingredient, such as Meat, Vegetable, or Fruit.
        ///     See <see cref="FoodCategories.Types"/> for predefined categories.
        /// </summary>
        public readonly string Category { get; init; } = category;

        /// <summary>
        ///     The source of this ingredient, such as Fishing, Farming, or Foraging.
        ///     See <see cref="FoodCategories.Sources"/> for predefined sources.
        /// </summary>
        public readonly string Source { get; init; } = source;

        public override string ToString()
        {
            return string.Create(null, $"Ingredient [Name: {Item.Name}, Category: {Category}, Source: {Source}"); 
        }

        public override bool Equals(object obj)
        {
            return obj is Ingredient other &&
                Item.type == other.Item.type &&
                Category == other.Category &&
                Source == other.Source;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 31 + Item.type.GetHashCode();
                hash = hash * 31 + (Category?.GetHashCode() ?? 0);
                hash = hash * 31 + (Source?.GetHashCode() ?? 0);
                return hash;
            }
        }

        public static bool operator ==(Ingredient left, Ingredient right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Ingredient left, Ingredient right)
        {
            return !(left == right);
        }

        public bool Equals(Ingredient other)
        {
            return Equals(Item, other.Item) && Category == other.Category && Source == other.Source;
        }
    }
}
