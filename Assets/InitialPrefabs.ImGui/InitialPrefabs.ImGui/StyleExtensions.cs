using InitialPrefabs.NimGui.Text;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
namespace InitialPrefabs.NimGui {
    public static class StyleExtensions {
        public static ref ImButtonStyle WithBackground(this ref ImButtonStyle style, Color32 background) {
            style.Background = background;
            return ref style;
        }
        public static ref ImButtonStyle WithHover(this ref ImButtonStyle style, Color32 hover) {
            style.Hover = hover;
            return ref style;
        }
        public static ref ImButtonStyle WithPressed(this ref ImButtonStyle style, Color32 pressed) {
            style.Pressed = pressed;
            return ref style;
        }
        public static ref ImButtonStyle WithText(this ref ImButtonStyle style, Color32 text) {
            style.Text = text;
            return ref style;
        }
        public static ref ImButtonStyle WithFontSize(this ref ImButtonStyle style, ushort fontsize) {
            style.FontSize = fontsize;
            return ref style;
        }
        public static ref ImButtonStyle WithPadding(this ref ImButtonStyle style, float2 padding) {
            style.Padding = padding;
            return ref style;
        }
        public static ref ImButtonStyle WithColumn(this ref ImButtonStyle style, HorizontalAlignment column) {
            style.Column = column;
            return ref style;
        }
        public static ref ImButtonStyle WithRow(this ref ImButtonStyle style, VerticalAlignment row) {
            style.Row = row;
            return ref style;
        }
        public static ref ImDropDownStyle WithBackground(this ref ImDropDownStyle style, Color32 background) {
            style.Background = background;
            return ref style;
        }
        public static ref ImDropDownStyle WithHover(this ref ImDropDownStyle style, Color32 hover) {
            style.Hover = hover;
            return ref style;
        }
        public static ref ImDropDownStyle WithPressed(this ref ImDropDownStyle style, Color32 pressed) {
            style.Pressed = pressed;
            return ref style;
        }
        public static ref ImDropDownStyle WithText(this ref ImDropDownStyle style, Color32 text) {
            style.Text = text;
            return ref style;
        }
        public static ref ImDropDownStyle WithColumn(this ref ImDropDownStyle style, HorizontalAlignment column) {
            style.Column = column;
            return ref style;
        }
        public static ref ImDropDownStyle WithRow(this ref ImDropDownStyle style, VerticalAlignment row) {
            style.Row = row;
            return ref style;
        }
        public static ref ImDropDownStyle WithFontSize(this ref ImDropDownStyle style, ushort fontsize) {
            style.FontSize = fontsize;
            return ref style;
        }
        public static ref ImDropDownStyle WithPadding(this ref ImDropDownStyle style, float2 padding) {
            style.Padding = padding;
            return ref style;
        }
        public static ref ImLineStyle WithColor(this ref ImLineStyle style, Color32 color) {
            style.Color = color;
            return ref style;
        }
        public static ref ImLineStyle WithPadding(this ref ImLineStyle style, float padding) {
            style.Padding = padding;
            return ref style;
        }
        public static ref ImPaneStyle WithTitleBar(this ref ImPaneStyle style, Color32 titlebar) {
            style.TitleBar = titlebar;
            return ref style;
        }
        public static ref ImPaneStyle WithText(this ref ImPaneStyle style, Color32 text) {
            style.Text = text;
            return ref style;
        }
        public static ref ImPaneStyle WithPane(this ref ImPaneStyle style, Color32 pane) {
            style.Pane = pane;
            return ref style;
        }
        public static ref ImPaneStyle WithDefaultButtonBackground(this ref ImPaneStyle style, Color32 defaultbuttonbackground) {
            style.DefaultButtonBackground = defaultbuttonbackground;
            return ref style;
        }
        public static ref ImPaneStyle WithDefaultButtonHover(this ref ImPaneStyle style, Color32 defaultbuttonhover) {
            style.DefaultButtonHover = defaultbuttonhover;
            return ref style;
        }
        public static ref ImPaneStyle WithDefaultButtonPress(this ref ImPaneStyle style, Color32 defaultbuttonpress) {
            style.DefaultButtonPress = defaultbuttonpress;
            return ref style;
        }
        public static ref ImPaneStyle WithCollapseDefaultFg(this ref ImPaneStyle style, Color32 collapsedefaultfg) {
            style.CollapseDefaultFg = collapsedefaultfg;
            return ref style;
        }
        public static ref ImPaneStyle WithCollapseHoverFg(this ref ImPaneStyle style, Color32 collapsehoverfg) {
            style.CollapseHoverFg = collapsehoverfg;
            return ref style;
        }
        public static ref ImPaneStyle WithCollapsePressedFg(this ref ImPaneStyle style, Color32 collapsepressedfg) {
            style.CollapsePressedFg = collapsepressedfg;
            return ref style;
        }
        public static ref ImPaneStyle WithCloseDefaultFg(this ref ImPaneStyle style, Color32 closedefaultfg) {
            style.CloseDefaultFg = closedefaultfg;
            return ref style;
        }
        public static ref ImPaneStyle WithCloseHoverFg(this ref ImPaneStyle style, Color32 closehoverfg) {
            style.CloseHoverFg = closehoverfg;
            return ref style;
        }
        public static ref ImPaneStyle WithClosePressedFg(this ref ImPaneStyle style, Color32 closepressedfg) {
            style.ClosePressedFg = closepressedfg;
            return ref style;
        }
        public static ref ImPaneStyle WithPadding(this ref ImPaneStyle style, float2 padding) {
            style.Padding = padding;
            return ref style;
        }
        public static ref ImPaneStyle WithDefaultFontSize(this ref ImPaneStyle style, ushort defaultfontsize) {
            style.DefaultFontSize = defaultfontsize;
            return ref style;
        }
        public static ref ImPaneStyle WithTitleFontSize(this ref ImPaneStyle style, ushort titlefontsize) {
            style.TitleFontSize = titlefontsize;
            return ref style;
        }
        public static ref ImPaneStyle WithColumn(this ref ImPaneStyle style, HorizontalAlignment column) {
            style.Column = column;
            return ref style;
        }
        public static ref ImPaneStyle WithRow(this ref ImPaneStyle style, VerticalAlignment row) {
            style.Row = row;
            return ref style;
        }
        public static ref ImProgressBarStyle WithBackground(this ref ImProgressBarStyle style, Color32 background) {
            style.Background = background;
            return ref style;
        }
        public static ref ImProgressBarStyle WithForeground(this ref ImProgressBarStyle style, Color32 foreground) {
            style.Foreground = foreground;
            return ref style;
        }
        public static ref ImProgressBarStyle WithTextColor(this ref ImProgressBarStyle style, Color32 textcolor) {
            style.TextColor = textcolor;
            return ref style;
        }
        public static ref ImProgressBarStyle WithFontSize(this ref ImProgressBarStyle style, ushort fontsize) {
            style.FontSize = fontsize;
            return ref style;
        }
        public static ref ImProgressBarStyle WithColumn(this ref ImProgressBarStyle style, HorizontalAlignment column) {
            style.Column = column;
            return ref style;
        }
        public static ref ImProgressBarStyle WithRow(this ref ImProgressBarStyle style, VerticalAlignment row) {
            style.Row = row;
            return ref style;
        }
        public static ref ImScrollAreaStyle WithScrollButtonWidth(this ref ImScrollAreaStyle style, float scrollbuttonwidth) {
            style.ScrollButtonWidth = scrollbuttonwidth;
            return ref style;
        }
        public static ref ImScrollAreaStyle WithPadding(this ref ImScrollAreaStyle style, float2 padding) {
            style.Padding = padding;
            return ref style;
        }
        public static ref ImScrollAreaStyle WithButtonDefault(this ref ImScrollAreaStyle style, Color32 buttondefault) {
            style.ButtonDefault = buttondefault;
            return ref style;
        }
        public static ref ImScrollAreaStyle WithButtonHover(this ref ImScrollAreaStyle style, Color32 buttonhover) {
            style.ButtonHover = buttonhover;
            return ref style;
        }
        public static ref ImScrollAreaStyle WithButtonPressed(this ref ImScrollAreaStyle style, Color32 buttonpressed) {
            style.ButtonPressed = buttonpressed;
            return ref style;
        }
        public static ref ImScrollAreaStyle WithScrollBarBackground(this ref ImScrollAreaStyle style, Color32 scrollbarbackground) {
            style.ScrollBarBackground = scrollbarbackground;
            return ref style;
        }
        public static ref ImScrollAreaStyle WithScrollBarPanel(this ref ImScrollAreaStyle style, Color32 scrollbarpanel) {
            style.ScrollBarPanel = scrollbarpanel;
            return ref style;
        }
        public static ref ImScrollAreaStyle WithDeltaTime(this ref ImScrollAreaStyle style, float deltatime) {
            style.DeltaTime = deltatime;
            return ref style;
        }
        public static ref ImScrollAreaStyle WithScrollSpeed(this ref ImScrollAreaStyle style, float scrollspeed) {
            style.ScrollSpeed = scrollspeed;
            return ref style;
        }
        public static ref ImSkipLineStyle WithPadding(this ref ImSkipLineStyle style, float2 padding) {
            style.Padding = padding;
            return ref style;
        }
        public static ref ImSkipLineStyle WithFontSize(this ref ImSkipLineStyle style, ushort fontsize) {
            style.FontSize = fontsize;
            return ref style;
        }
        public static ref ImSliderStyle WithBackground(this ref ImSliderStyle style, Color32 background) {
            style.Background = background;
            return ref style;
        }
        public static ref ImSliderStyle WithButtonDefault(this ref ImSliderStyle style, Color32 buttondefault) {
            style.ButtonDefault = buttondefault;
            return ref style;
        }
        public static ref ImSliderStyle WithButtonHover(this ref ImSliderStyle style, Color32 buttonhover) {
            style.ButtonHover = buttonhover;
            return ref style;
        }
        public static ref ImSliderStyle WithButtonPressed(this ref ImSliderStyle style, Color32 buttonpressed) {
            style.ButtonPressed = buttonpressed;
            return ref style;
        }
        public static ref ImSliderStyle WithTextColor(this ref ImSliderStyle style, Color32 textcolor) {
            style.TextColor = textcolor;
            return ref style;
        }
        public static ref ImSliderStyle WithFontSize(this ref ImSliderStyle style, ushort fontsize) {
            style.FontSize = fontsize;
            return ref style;
        }
        public static ref ImSliderStyle WithPadding(this ref ImSliderStyle style, float2 padding) {
            style.Padding = padding;
            return ref style;
        }
        public static ref ImTextFieldStyle WithFontSize(this ref ImTextFieldStyle style, ushort fontsize) {
            style.FontSize = fontsize;
            return ref style;
        }
        public static ref ImTextFieldStyle WithText(this ref ImTextFieldStyle style, Color32 text) {
            style.Text = text;
            return ref style;
        }
        public static ref ImTextFieldStyle WithBackground(this ref ImTextFieldStyle style, Color32 background) {
            style.Background = background;
            return ref style;
        }
        public static ref ImTextFieldStyle WithHover(this ref ImTextFieldStyle style, Color32 hover) {
            style.Hover = hover;
            return ref style;
        }
        public static ref ImTextFieldStyle WithColumn(this ref ImTextFieldStyle style, HorizontalAlignment column) {
            style.Column = column;
            return ref style;
        }
        public static ref ImTextFieldStyle WithRow(this ref ImTextFieldStyle style, VerticalAlignment row) {
            style.Row = row;
            return ref style;
        }
        public static ref ImTextFieldStyle WithPadding(this ref ImTextFieldStyle style, float2 padding) {
            style.Padding = padding;
            return ref style;
        }
        public static ref ImTextStyle WithFontSize(this ref ImTextStyle style, ushort fontsize) {
            style.FontSize = fontsize;
            return ref style;
        }
        public static ref ImTextStyle WithColumn(this ref ImTextStyle style, HorizontalAlignment column) {
            style.Column = column;
            return ref style;
        }
        public static ref ImTextStyle WithRow(this ref ImTextStyle style, VerticalAlignment row) {
            style.Row = row;
            return ref style;
        }
        public static ref ImTextStyle WithTextColor(this ref ImTextStyle style, Color32 textcolor) {
            style.TextColor = textcolor;
            return ref style;
        }
        public static ref ImTextStyle WithPadding(this ref ImTextStyle style, float2 padding) {
            style.Padding = padding;
            return ref style;
        }
    }
}