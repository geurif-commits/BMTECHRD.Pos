using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BMTECHRD.Pos.App.Helpers;

public static class NumericTextBoxBehavior
{
    public static readonly DependencyProperty IsNumericOnlyProperty = DependencyProperty.RegisterAttached(
        "IsNumericOnly", typeof(bool), typeof(NumericTextBoxBehavior), new PropertyMetadata(false, OnIsNumericOnlyChanged));

    public static readonly DependencyProperty MaxDigitsProperty = DependencyProperty.RegisterAttached(
        "MaxDigits", typeof(int), typeof(NumericTextBoxBehavior), new PropertyMetadata(0));

    public static readonly DependencyProperty AllowNegativeProperty = DependencyProperty.RegisterAttached(
        "AllowNegative", typeof(bool), typeof(NumericTextBoxBehavior), new PropertyMetadata(false));

    public static void SetIsNumericOnly(DependencyObject element, bool value) => element.SetValue(IsNumericOnlyProperty, value);
    public static bool GetIsNumericOnly(DependencyObject element) => (bool)element.GetValue(IsNumericOnlyProperty);

    public static void SetMaxDigits(DependencyObject element, int value) => element.SetValue(MaxDigitsProperty, value);
    public static int GetMaxDigits(DependencyObject element) => (int)element.GetValue(MaxDigitsProperty);

    public static void SetAllowNegative(DependencyObject element, bool value) => element.SetValue(AllowNegativeProperty, value);
    public static bool GetAllowNegative(DependencyObject element) => (bool)element.GetValue(AllowNegativeProperty);

    private static void OnIsNumericOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox tb) return;
        var was = (bool)e.OldValue;
        var need = (bool)e.NewValue;
        if (was && !need)
        {
            tb.PreviewTextInput -= OnPreviewTextInput;
            DataObject.RemovePastingHandler(tb, new DataObjectPastingEventHandler(OnPasting));
            tb.TextChanged -= OnTextChanged;
        }
        if (!was && need)
        {
            tb.PreviewTextInput += OnPreviewTextInput;
            DataObject.AddPastingHandler(tb, new DataObjectPastingEventHandler(OnPasting));
            tb.TextChanged += OnTextChanged;
        }
    }

    private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is not TextBox tb) return;
        var max = GetMaxDigits(tb);
        var allowNeg = GetAllowNegative(tb);

        // Handle minus sign
        if (e.Text == "-")
        {
            if (!allowNeg)
            {
                e.Handled = true;
                return;
            }
            // Only allow '-' at the beginning (position 0)
            if (tb.SelectionStart != 0 || tb.Text.StartsWith("-"))
            {
                e.Handled = true;
                return;
            }
            e.Handled = false;
            return;
        }

        // Allow only digits
        if (!Regex.IsMatch(e.Text, "^[0-9]+$"))
        {
            e.Handled = true;
            return;
        }

        if (max > 0)
        {
            var selectionLen = tb.SelectionLength;
            // Count only digits, not the minus sign
            var digitCount = Regex.Matches(tb.Text, "[0-9]").Count;
            var newDigitCount = digitCount - selectionLen + e.Text.Length;
            if (newDigitCount > max) e.Handled = true;
        }
    }

    private static void OnPasting(object sender, DataObjectPastingEventArgs e)
    {
        if (sender is not TextBox tb) return;
        if (!e.DataObject.GetDataPresent(DataFormats.Text))
        {
            e.CancelCommand();
            return;
        }
        var paste = e.DataObject.GetData(DataFormats.Text) as string ?? string.Empty;
        var allowNeg = GetAllowNegative(tb);

        // Clean up pasted text: remove invalid characters
        var cleaned = paste.Trim();
        if (allowNeg && cleaned.StartsWith("-"))
        {
            // Keep leading minus and digits only
            cleaned = "-" + Regex.Replace(cleaned.Substring(1), "[^0-9]", string.Empty);
        }
        else
        {
            // Keep digits only
            cleaned = Regex.Replace(cleaned, "[^0-9]", string.Empty);
        }

        if (string.IsNullOrEmpty(cleaned))
        {
            e.CancelCommand();
            return;
        }

        var max = GetMaxDigits(tb);
        if (max > 0)
        {
            var digits = Regex.Matches(cleaned, "[0-9]").Count;
            if (digits > max)
            {
                var digitStr = Regex.Replace(cleaned, "[^0-9]", string.Empty);
                digitStr = digitStr.Substring(0, max);
                cleaned = (cleaned.StartsWith("-") ? "-" : "") + digitStr;
            }
        }

        e.DataObject.SetData(DataFormats.Text, cleaned);
    }

    private static void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox tb) return;
        var original = tb.Text;
        var allowNeg = GetAllowNegative(tb);
        var max = GetMaxDigits(tb);

        // Sanitize: extract leading minus (if allowed) and digits only
        var hasNegative = allowNeg && original.StartsWith("-");
        var digits = Regex.Replace(original, "[^0-9]", string.Empty);

        if (max > 0 && digits.Length > max)
        {
            digits = digits.Substring(0, max);
        }

        var sanitized = (hasNegative ? "-" : "") + digits;

        if (sanitized != original)
        {
            var sel = tb.SelectionStart;
            tb.Text = sanitized;
            tb.SelectionStart = Math.Min(sel, tb.Text.Length);
        }
    }
}
