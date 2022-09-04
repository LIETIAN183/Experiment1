using System.Runtime.CompilerServices;
using InitialPrefabs.NimGui.Text;
using Unity.Mathematics;
using UnityEngine;

namespace InitialPrefabs.NimGui {
    /// <summary>
    /// Stores the color states for the Pane.
    /// </summary>
    public partial struct ImPaneStyle : IStyle {

        /// <summary>
        /// Color of the top bar.
        /// </summary>
        public Color32 TitleBar;

        /// <summary>
        /// Text color. 
        /// </summary>
        public Color32 Text;

        /// <summary>
        /// Background color.
        /// </summary>
        public Color32 Pane;

        /// <summary>
        /// Default color of the button.
        /// </summary>
        public Color32 DefaultButtonBackground;

        /// <summary>
        /// Color of the button when the mouse is over it. 
        /// </summary>
        public Color32 DefaultButtonHover;

        /// <summary>
        /// Color of the button when the mouse clicks it.
        /// </summary>
        public Color32 DefaultButtonPress;

        /// <summary>
        /// Default collapse button color.
        /// </summary>
        public Color32 CollapseDefaultFg;

        /// <summary>
        /// Collapse button color when the mouse is over it.
        /// </summary>
        public Color32 CollapseHoverFg;

        /// <summary>
        /// Collapse button color when the mouse clicks it.
        /// </summary>
        public Color32 CollapsePressedFg;

        /// <summary>
        /// Default close button color.
        /// </summary>
        public Color32 CloseDefaultFg;

        /// <summary>
        /// Close button color when the mouse is over it.
        /// </summary>
        public Color32 CloseHoverFg;

        /// <summary>
        /// Close pressed color when the mouse clicks it.
        /// </summary>
        public Color32 ClosePressedFg;

        /// <summary>
        /// Amount of spacing between the current and next widget.
        /// </summary>
        public float2 Padding;

        /// <summary>
        /// The default size of the font.
        /// </summary>
        public int DefaultFontSize;

        /// <summary>
        /// The size of the font for the title (top bar).
        /// </summary>
        public int TitleFontSize;

        /// <summary>
        /// Column wise alignment.
        /// </summary>
        public HorizontalAlignment Column;

        /// <summary>
        /// Row wise alignment.
        /// </summary>
        public VerticalAlignment Row;

        /// <summary>
        /// Constructs a new instance of the PaneStyle with default settings.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImPaneStyle New() {
            return new ImPaneStyle {
                TitleBar                = DefaultStyles.Default,
                Text                    = DefaultStyles.Text,
                Pane                    = DefaultStyles.Pane,
                DefaultButtonBackground = DefaultStyles.Clear,
                DefaultButtonHover      = DefaultStyles.Hover,
                DefaultButtonPress      = DefaultStyles.Pressed,
                CollapseDefaultFg       = DefaultStyles.CollapseDefaultFg,
                CollapseHoverFg         = DefaultStyles.HoverForeground,
                CollapsePressedFg       = DefaultStyles.PressedForeground,
                CloseDefaultFg          = DefaultStyles.CloseDefaultFg,
                CloseHoverFg            = DefaultStyles.HoverForeground,
                ClosePressedFg          = DefaultStyles.PressedForeground,
                Padding                 = DefaultStyles.Padding,
                TitleFontSize           = DefaultStyles.TitleFontSize,
                DefaultFontSize         = DefaultStyles.DefaultFontSize
            };
        }
    }

    public static class ImPaneStyleExtensions {
        
        /// <summary>
        /// Gets the implicit ImButtonStyle from the ImPaneStyle.
        /// </summary>
        /// <param name="pane">The pane style to reference.</param>
        /// <returns>An instance of the ImButtonStyle</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImButtonStyle GetButtonStyle(this in ImPaneStyle pane) {
            return new ImButtonStyle {
                Background = pane.DefaultButtonBackground,
                Hover      = pane.DefaultButtonHover,
                Pressed    = pane.DefaultButtonPress,
                Padding    = pane.Padding,
                Text       = pane.Text,
            };
        }

        /// <summary>
        /// Gets the implicit ImTextStyle from the ImPaneStyle.
        /// </summary>
        /// <param name="pane">The pane style to reference.</param>
        /// <returns>An instance of the ImTextStyle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ImTextStyle GetTextStyle(this in ImPaneStyle pane) {
            return new ImTextStyle {
                Column    = pane.Column,
                Row       = pane.Row,
                FontSize  = pane.TitleFontSize,
                Padding   = pane.Padding,
                TextColor = pane.Text
            };
        }

        /// <summary>
        /// Sets the implicit ImButtonStyle in the ImPaneStyle.
        /// </summary>
        /// <param name="pane">The pane style to reference.</param>
        /// <param name="style">The desired ImButtonStyle.</param>
        /// <returns>An instance of the ImPaneStyle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref ImPaneStyle WithButtonStyle(this ref ImPaneStyle pane, in ImButtonStyle style) {
            pane.DefaultButtonBackground = style.Background;
            pane.DefaultButtonHover      = style.Hover;
            pane.DefaultButtonPress      = style.Pressed;
            pane.Text                    = style.Text;
            pane.Padding                 = style.Padding;
            return ref pane;
        }
    }
}
