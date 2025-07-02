using UnityEngine;

namespace TinyWalnutGames.UITKTemplates.Tools
{
    /// <summary>
	/// This script defines a static class that contains predefined colors for various purposes.
	/// </summary>
    /// <remarks>
    /// Each of these color properties must remain in this order.
	/// If you insert any more color types, you must make sure to add them to the MaterialColorCustomizer script. 
	/// There are two places: the PredefinedColorType enum and the UpdatePredefinedColors() method.
    /// </remarks>
    public static class PredefinedColors
    {
        /// <summary>
        /// Array of colors representing body art colors.
        /// </summary>
        public static readonly Color[] BodyArtColors = new Color[]
	    {
		    Color.black, // (0.0, 0.0, 0.0)
		    new(0.1f, 0.1f, 0.1f), // Charcoal Gray
		    new(0.2f, 0.2f, 0.2f), // Dark Gray
		    new(0.3f, 0.3f, 0.3f), // Medium Dark Gray
		    new(0.4f, 0.4f, 0.4f), // Medium Gray
		    new(0.5f, 0.5f, 0.5f), // Medium Light Gray
		    new(0.6f, 0.6f, 0.6f), // Light Gray
		    new(0.7f, 0.7f, 0.7f), // Light Gray
		    new(0.8f, 0.8f, 0.8f), // Very Light Gray
		    new(0.9f, 0.9f, 0.9f), // Lightest Gray
		    Color.white, // (1.0, 1.0, 1.0)
		    Color.red, // (1.0, 0.0, 0.0)
		    new(0.65f, 0.16f, 0.16f), // Dark Red
		    new(1.0f, 0.65f, 0.0f), // Orange
		    Color.yellow, // (1.0, 1.0, 0.0)
		    Color.green, // (0.0, 1.0, 0.0)
		    Color.blue, // (0.0, 0.0, 1.0)
		    Color.cyan, // (0.0, 1.0, 1.0)
		    new(1.0f, 0.75f, 0.8f), // Light Pink
		    Color.magenta // (1.0, 0.0, 1.0)
	    };

        /// <summary>
        /// Array of colors representing eye colors.
        /// </summary>
        public static readonly Color[] EyeColors = new Color[]
	    {
		    Color.black, // (0.0, 0.0, 0.0)
		    Color.blue, // (0.0, 0.0, 1.0)
		    new(0.65f, 0.16f, 0.16f), // Dark Red
		    Color.green, // (0.0, 1.0, 0.0)
		    new(0.5f, 0.35f, 0.05f), // Hazel
		    Color.gray, // (0.5, 0.5, 0.5)
		    new(0.8f, 0.5f, 0.3f), // Amber
		    new(0.6f, 0.4f, 0.2f), // Light Brown
		    new(0.7f, 0.5f, 0.3f), // Brown
		    new(0.9f, 0.8f, 0.6f), // Light Hazel
		    new(0.5f, 0.25f, 0.1f), // Dark Brown
		    new(0.9f, 0.7f, 0.5f), // Light Amber
		    new(0.8f, 0.7f, 0.5f), // Light Blonde
		    Color.red, // (1.0, 0.0, 0.0)
		    new(0.5f, 0.0f, 0.5f), // Purple
		    Color.cyan, // (0.0, 1.0, 1.0)
		    Color.magenta, // (1.0, 0.0, 1.0)
		    Color.yellow, // (1.0, 1.0, 0.0)
		    Color.white, // (1.0, 1.0, 1.0)
		    new(1.0f, 0.75f, 0.8f) // Light Pink
	    };

        /// <summary>
        /// Array of colors representing hair colors.
        /// </summary>
        public static readonly Color[] HairColors = new Color[]
        {
            Color.black, // (0.0, 0.0, 0.0)
			Color.white, // (1.0, 1.0, 1.0)
			new(0.5f, 0.25f, 0.1f), // Dark Brown
			new(0.6f, 0.4f, 0.2f), // Brown
			new(0.8f, 0.5f, 0.3f), // Light Brown
			new(0.7f, 0.5f, 0.3f), // Dark Blonde
			new(0.9f, 0.8f, 0.6f), // Blonde
			new(0.8f, 0.7f, 0.5f), // Light Blonde
			new(0.9f, 0.6f, 0.4f), // Auburn
			new(0.9f, 0.7f, 0.5f), // Strawberry Blonde
			Color.red, // (1.0, 0.0, 0.0)
			Color.green, // (0.0, 1.0, 0.0)
			Color.blue, // (0.0, 0.0, 1.0)
			Color.yellow, // (1.0, 1.0, 0.0)
			Color.cyan, // (0.0, 1.0, 1.0)
			Color.magenta // (1.0, 0.0, 1.0)
		};

        /// <summary>
        /// Array of colors representing dark leather colors.
        /// </summary>
        public static readonly Color[] LeatherColorsDark = new Color[]
        {
            new(0.45f, 0.22f, 0.05f), // Dark Brown
			new(0.28f, 0.20f, 0.15f), // Darker Brown
			new(0.70f, 0.45f, 0.20f), // Dark Tan
			new(0.40f, 0.30f, 0.20f), // Dark Light Brown
			new(0.25f, 0.15f, 0.10f)  // Dark Chocolate
		};

        /// <summary>
        /// Array of colors representing primary leather colors.
        /// </summary>
        public static readonly Color[] LeatherColorsPrimary = new Color[]
	    {
		    new(0.55f, 0.27f, 0.07f), // Brown
		    new(0.36f, 0.25f, 0.20f), // Dark Brown
		    new(0.80f, 0.52f, 0.25f), // Tan
		    new(0.50f, 0.35f, 0.25f), // Light Brown
		    new(0.30f, 0.20f, 0.15f)  // Chocolate
	    };

        /// <summary>
        /// Array of colors representing secondary leather colors.
        /// </summary>
        public static readonly Color[] LeatherColorsSecondary = new Color[]
	    {
		    new(0.45f, 0.22f, 0.05f), // Dark Brown
		    new(0.28f, 0.20f, 0.15f), // Darker Brown
		    new(0.70f, 0.45f, 0.20f), // Dark Tan
		    new(0.40f, 0.30f, 0.20f), // Dark Light Brown
		    new(0.25f, 0.15f, 0.10f)  // Dark Chocolate
	    };

        /// <summary>
        /// Array of colors representing dark metal colors.
        /// </summary>
        public static readonly Color[] MetalColorsDark = new Color[]
	    {
		    new(0.60f, 0.60f, 0.60f), // Dark Silver
		    new(0.70f, 0.58f, 0.18f), // Dark Gold
		    new(0.45f, 0.45f, 0.47f), // Dark Steel
		    new(0.60f, 0.38f, 0.15f), // Dark Bronze
		    new(0.65f, 0.40f, 0.15f)  // Dark Copper
	    };

        /// <summary>
        /// Array of colors representing primary metal colors.
        /// </summary>
        public static readonly Color[] MetalColorsPrimary = new Color[]
	    {
		    new(0.75f, 0.75f, 0.75f), // Silver
		    new(0.83f, 0.69f, 0.22f), // Gold
		    new(0.56f, 0.56f, 0.58f), // Steel
		    new(0.72f, 0.45f, 0.20f), // Bronze
		    new(0.80f, 0.50f, 0.20f)  // Copper
	    };

        /// <summary>
        /// Array of colors representing secondary metal colors.
        /// </summary>
        public static readonly Color[] MetalColorsSecondary = new Color[]
        {
            new(0.85f, 0.85f, 0.85f), // Light Silver
			new(0.93f, 0.79f, 0.32f), // Light Gold
			new(0.66f, 0.66f, 0.68f), // Light Steel
			new(0.82f, 0.55f, 0.30f), // Light Bronze
			new(0.90f, 0.60f, 0.30f)  // Light Copper
		};

        /// <summary>
        /// Array of colors representing primary colors.
        /// </summary>
        public static readonly Color[] PrimaryColors = new Color[]
	    {
		    Color.red, // Red (RGB: 1, 0, 0)
		    Color.green, // Green (RGB: 0, 1, 0)
		    Color.blue, // Blue (RGB: 0, 0, 1)
		    Color.yellow, // Yellow (RGB: 1, 1, 0)
		    Color.cyan, // Cyan (RGB: 0, 1, 1)
		    Color.magenta  // Magenta (RGB: 1, 0, 1)
	    };

        /// <summary>
        /// Array of colors representing scar colors, derived from skin tones.
        /// </summary>
        public static readonly Color[] ScarColors = new Color[]
	    {
		    new(0.95f, 0.75f, 0.65f), // Light Scar - tint of red
		    new(0.95f, 0.65f, 0.55f), // Fair Scar - tint of red
		    new(0.85f, 0.55f, 0.45f), // Medium Light Scar - tint of red
		    new(0.75f, 0.45f, 0.35f), // Medium Scar - tint of red
		    new(0.65f, 0.35f, 0.25f), // Medium Dark Scar - tint of red
		    new(0.55f, 0.25f, 0.25f), // Dark Scar; lighter than the skin tone in this index - tint of red
		    new(0.45f, 0.15f, 0.15f), // Very Dark Scar; lighter than the skin tone in this index - tint of red
		    new(0.35f, 0.15f, 0.15f), // Deep Dark Scar; lighter than the skin tone in this index - tint of red
		    new(0.25f, 0.15f, 0.15f), // Deepest Dark Scar; lighter than the skin tone in this index - tint of red
		    new(0.15f, 0.1f, 0.1f) // Ultra Dark Scar; lighter than the skin tone in this index - tint of red
	    };

        /// <summary>
        /// Array of colors representing secondary colors.
        /// </summary>
        public static readonly Color[] SecondaryColors = new Color[]
	    {
		    new(1.0f, 0.5f, 0.0f), // Orange
		    new(0.5f, 0.0f, 0.5f), // Purple
		    new(0.0f, 0.5f, 0.5f), // Teal
		    new(0.5f, 0.5f, 0.0f), // Olive
		    new(0.5f, 0.0f, 0.0f), // Maroon
		    new(0.0f, 0.5f, 0.0f)  // Dark Green
	    };

        /// <summary>
        /// Array of colors representing skin tones, derived from the lightest skin tone.
        /// </summary>
        public static readonly Color[] SkinTones = new Color[]
	    {
		    new(1.0f, 0.8f, 0.6f), // Light Skin
		    new(1.0f, 0.7f, 0.5f), // Fair Skin
		    new(0.9f, 0.6f, 0.4f), // Medium Light Skin
		    new(0.8f, 0.5f, 0.3f), // Medium Skin
		    new(0.7f, 0.4f, 0.2f), // Medium Dark Skin
		    new(0.6f, 0.3f, 0.2f), // Dark Skin
		    new(0.5f, 0.2f, 0.1f), // Very Dark Skin
		    new(0.4f, 0.2f, 0.1f), // Deep Dark Skin
		    new(0.3f, 0.2f, 0.1f), // Deepest Dark Skin
		    new(0.2f, 0.1f, 0.05f) // Ultra Dark Skin
	    };

        /// <summary>
        /// Array of colors representing stubble colors, derived from hair colors with 20% alpha.
        /// </summary>
        public static readonly Color[] StubbleColors;

        /// <summary>
        /// Static constructor to initialize the StubbleColors array based on HairColors.
        /// </summary>
        static PredefinedColors()
	    {
		    float alpha = 0.2f; // 20% alpha
		    StubbleColors = new Color[HairColors.Length];
		    for (int i = 0; i < HairColors.Length; i++)
		    {
			    StubbleColors[i] = new(HairColors[i].r, HairColors[i].g, HairColors[i].b, alpha);
		    }
	    }

        /// <summary>
        /// Blends two colors together based on the alpha value.
        /// </summary>
        /// <param name="baseColor"></param>
        /// <param name="overlayColor"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        private static Color Blend(Color baseColor, Color overlayColor, float alpha)
        {
            return new(
                baseColor.r * (1 - alpha) + overlayColor.r * alpha,
                baseColor.g * (1 - alpha) + overlayColor.g * alpha,
                baseColor.b * (1 - alpha) + overlayColor.b * alpha
            );
        }
    }
}
