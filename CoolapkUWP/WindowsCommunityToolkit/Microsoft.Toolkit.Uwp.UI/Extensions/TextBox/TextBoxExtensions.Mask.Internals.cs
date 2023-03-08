// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Toolkit.Uwp.UI
{
    /// <inheritdoc cref="TextBoxExtensions"/>
    public static partial class TextBoxExtensions
    {
        private const string DefaultPlaceHolder = "_";
        private const char EscapeChar = '\\';
        private static readonly KeyValuePair<char, string> AlphaCharacterRepresentation = new KeyValuePair<char, string>('a', "[A-Za-z]");
        private static readonly KeyValuePair<char, string> NumericCharacterRepresentation = new KeyValuePair<char, string>('9', "[0-9]");
        private static readonly KeyValuePair<char, string> AlphaNumericRepresentation = new KeyValuePair<char, string>('*', "[A-Za-z0-9]");

        private static void InitTextBoxMask(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBox textbox))
            {
                return;
            }

            textbox.SelectionChanged -= Textbox_SelectionChanged;
            textbox.TextChanging -= Textbox_TextChanging;
            textbox.Paste -= Textbox_Paste;
            textbox.Loaded -= Textbox_Loaded;
            textbox.GotFocus -= Textbox_GotFocus_Mask;
            textbox.Loaded += Textbox_Loaded;
        }

        private static void Textbox_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;

            // In case no value is provided, use it as normal textbox
            string mask = textbox.GetValue(MaskProperty) as string;
            if (string.IsNullOrWhiteSpace(mask))
            {
                return;
            }

            string placeHolderValue = textbox.GetValue(MaskPlaceholderProperty) as string;
            if (string.IsNullOrEmpty(placeHolderValue))
            {
                throw new ArgumentException("PlaceHolder can't be null or empty");
            }

            List<int> escapedChars = new List<int>();

            StringBuilder builder = new StringBuilder(mask);
            for (int i = 0; i < builder.Length - 1; i++)
            {
                if (builder[i] == EscapeChar)
                {
                    escapedChars.Add(i);
                    builder.Remove(i, 1);
                }
            }

            string escapedMask = builder.ToString();

            textbox.SetValue(EscapedCharacterIndicesProperty, escapedChars);
            textbox.SetValue(EscapedMaskProperty, escapedMask);

            char placeHolder = placeHolderValue[0];

            Dictionary<char, string> representationDictionary = new Dictionary<char, string>
            {
                { AlphaCharacterRepresentation.Key, AlphaCharacterRepresentation.Value },
                { NumericCharacterRepresentation.Key, NumericCharacterRepresentation.Value },
                { AlphaNumericRepresentation.Key, AlphaNumericRepresentation.Value }
            };

            string customDictionaryValue = textbox.GetValue(CustomMaskProperty) as string;
            if (!string.IsNullOrWhiteSpace(customDictionaryValue))
            {
                string[] customRoles = customDictionaryValue.Split(',');
                foreach (string role in customRoles)
                {
                    string[] roleValues = role.Split(':');
                    if (roleValues.Length != 2)
                    {
                        throw new ArgumentException("Invalid CustomMask property");
                    }

                    string keyValue = roleValues[0];
                    string value = roleValues[1];

                    // an exception should be throw if the regex is not valid
                    Regex.Match(string.Empty, value);
                    if (!char.TryParse(keyValue, out char key))
                    {
                        throw new ArgumentException("Invalid CustomMask property, please validate the mask key");
                    }

                    representationDictionary.Add(key, value);
                }
            }

            textbox.SetValue(RepresentationDictionaryProperty, representationDictionary);

            StringBuilder displayTextBuilder = new StringBuilder(escapedMask);
            for (int i = 0; i < displayTextBuilder.Length; i++)
            {
                if (escapedChars.Contains(i))
                {
                    continue;
                }

                foreach (char key in representationDictionary.Keys)
                {
                    if (displayTextBuilder[i] == key)
                    {
                        displayTextBuilder[i] = placeHolder;
                    }
                }
            }

            string displayText = displayTextBuilder.ToString();

            if (string.IsNullOrEmpty(textbox.Text))
            {
                textbox.Text = displayText;
            }
            else
            {
                string textboxInitialValue = textbox.Text;
                textbox.Text = displayText;
                int oldSelectionStart = (int)textbox.GetValue(OldSelectionStartProperty);
                SetTextBoxValue(textboxInitialValue, textbox, escapedMask, escapedChars, representationDictionary, placeHolder, oldSelectionStart);
            }

            textbox.TextChanging += Textbox_TextChanging;
            textbox.SelectionChanged += Textbox_SelectionChanged;
            textbox.Paste += Textbox_Paste;
            textbox.GotFocus += Textbox_GotFocus_Mask;
            textbox.SetValue(OldTextProperty, textbox.Text);
            textbox.SetValue(DefaultDisplayTextProperty, displayText);
            textbox.SelectionStart = 0;
        }

        private static void Textbox_GotFocus_Mask(object sender, RoutedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            string mask = textbox?.GetValue(MaskProperty) as string;
            string placeHolderValue = textbox?.GetValue(MaskPlaceholderProperty) as string;
            if (string.IsNullOrWhiteSpace(mask) ||
                !(textbox?.GetValue(RepresentationDictionaryProperty) is Dictionary<char, string> representationDictionary) ||
                string.IsNullOrEmpty(placeHolderValue))
            {
                return;
            }

            char placeHolder = placeHolderValue[0];

            // if the textbox got focus and the textbox is empty (contains only mask) set the textbox cursor at the beginning to simulate normal TextBox behavior if it is empty.
            // if the textbox has value set the cursor to the first empty mask character
            string textboxText = textbox.Text;
            for (int i = 0; i < textboxText.Length; i++)
            {
                if (placeHolder == textboxText[i])
                {
                    textbox.SelectionStart = i;
                    break;
                }
            }
        }

        private static async void Textbox_Paste(object sender, TextControlPasteEventArgs e)
        {
            e.Handled = true;
            DataPackageView dataPackageView = Clipboard.GetContent();
            if (!dataPackageView.Contains(StandardDataFormats.Text))
            {
                return;
            }

            string pasteText = await dataPackageView.GetTextAsync();
            if (string.IsNullOrWhiteSpace(pasteText))
            {
                return;
            }

            TextBox textbox = (TextBox)sender;
            string mask = textbox.GetValue(MaskProperty) as string;
            string placeHolderValue = textbox.GetValue(MaskPlaceholderProperty) as string;
            if (string.IsNullOrWhiteSpace(mask) ||
            !(textbox?.GetValue(RepresentationDictionaryProperty) is Dictionary<char, string> representationDictionary) ||
            string.IsNullOrEmpty(placeHolderValue))
            {
                return;
            }

            string escapedMask = textbox.GetValue(EscapedMaskProperty) as string;
            List<int> escapedChars = textbox.GetValue(EscapedCharacterIndicesProperty) as List<int>;

            // to update the textbox text without triggering TextChanging text
            int oldSelectionStart = (int)textbox.GetValue(OldSelectionStartProperty);
            textbox.TextChanging -= Textbox_TextChanging;
            SetTextBoxValue(pasteText, textbox, escapedMask, escapedChars, representationDictionary, placeHolderValue[0], oldSelectionStart);
            textbox.SetValue(OldTextProperty, textbox.Text);
            textbox.TextChanging += Textbox_TextChanging;
        }

        private static void SetTextBoxValue(
            string newValue,
            TextBox textbox,
            string mask,
            List<int> escapedChars,
            Dictionary<char, string> representationDictionary,
            char placeholder,
            int oldSelectionStart)
        {
            int maxLength = (newValue.Length + oldSelectionStart) < mask.Length ? (newValue.Length + oldSelectionStart) : mask.Length;
            char[] textArray = textbox.Text.ToCharArray();

            for (int i = oldSelectionStart; i < maxLength; i++)
            {
                char maskChar = mask[i];
                char selectedChar = newValue[i - oldSelectionStart];

                // If dynamic character a,9,* or custom
                if (representationDictionary.ContainsKey(maskChar) && !escapedChars.Contains(i))
                {
                    string pattern = representationDictionary[maskChar];
                    textArray[i] = Regex.IsMatch(selectedChar.ToString(), pattern) ? selectedChar : placeholder;
                }
            }

            textbox.Text = new string(textArray);
            textbox.SelectionStart = maxLength;
        }

        private static void Textbox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            textbox.SetValue(OldSelectionStartProperty, textbox.SelectionStart);
            textbox.SetValue(OldSelectionLengthProperty, textbox.SelectionLength);
        }

        private static void Textbox_TextChanging(TextBox textbox, TextBoxTextChangingEventArgs args)
        {
            string escapedMask = textbox.GetValue(EscapedMaskProperty) as string;
            List<int> escapedChars = textbox.GetValue(EscapedCharacterIndicesProperty) as List<int>;

            string placeHolderValue = textbox?.GetValue(MaskPlaceholderProperty) as string;
            int oldSelectionStart = (int)textbox.GetValue(OldSelectionStartProperty);
            int oldSelectionLength = (int)textbox.GetValue(OldSelectionLengthProperty);
            if (string.IsNullOrWhiteSpace(escapedMask) ||
                !(textbox.GetValue(RepresentationDictionaryProperty) is Dictionary<char, string> representationDictionary) ||
                string.IsNullOrEmpty(placeHolderValue) ||
                !(textbox.GetValue(OldTextProperty) is string oldText))
            {
                return;
            }

            char placeHolder = placeHolderValue[0];
            bool isDeleteOrBackspace = false;
            int deleteBackspaceIndex = 0;

            // Delete or backspace is triggered
            // if the new length is less than or equal the old text - the old selection length then a delete or backspace is triggered with or without selection and no characters is added
            if (textbox.Text.Length < oldText.Length
                && textbox.Text.Length <= oldText.Length - oldSelectionLength)
            {
                isDeleteOrBackspace = true;
                if (oldSelectionLength == 0)
                {
                    // backspace else delete
                    if (oldSelectionStart != textbox.SelectionStart)
                    {
                        deleteBackspaceIndex++;
                    }
                }
            }

            // case adding data at the end of the textbox
            if (oldSelectionStart >= oldText.Length && !isDeleteOrBackspace)
            {
                // ignore change(s) if oldtext is a substring of new text value
                if (textbox.Text.Contains(oldText))
                {
                    textbox.Text = oldText;

                    if (oldText.Length >= 0)
                    {
                        textbox.SelectionStart = oldText.Length;
                    }

                    return;
                }
            }

            char[] textArray = oldText.ToCharArray();

            // detect if backspace or delete is triggered to handle the right removed character
            int newSelectionIndex = oldSelectionStart - deleteBackspaceIndex;

            // check if single selection
            bool isSingleSelection = oldSelectionLength != 0 && oldSelectionLength != 1;

            // for handling single key click add +1 to match length for selection = 1
            int singleOrMultiSelectionIndex = oldSelectionLength == 0 ? oldSelectionLength + 1 : oldSelectionLength;

            // Case change due to Text property is assigned a value (Ex Textbox.Text="value")
            if (textbox.SelectionStart == 0 && textbox.FocusState == FocusState.Unfocused)
            {
                string displayText = textbox.GetValue(DefaultDisplayTextProperty) as string ?? string.Empty;
                if (string.IsNullOrEmpty(textbox.Text))
                {
                    textbox.SetValue(OldTextProperty, displayText);
                    textbox.SetValue(OldSelectionStartProperty, 0);
                    textbox.SetValue(OldSelectionLengthProperty, 0);
                    textbox.Text = displayText;
                    return;
                }
                else
                {
                    string textboxInitialValue = textbox.Text;
                    textbox.Text = displayText;
                    SetTextBoxValue(textboxInitialValue, textbox, escapedMask, escapedChars, representationDictionary, placeHolderValue[0], 0);
                    textbox.SetValue(OldTextProperty, textbox.Text);
                    return;
                }
            }

            if (!isDeleteOrBackspace)
            {
                // In case the change happened due to user input
                char selectedChar = textbox.SelectionStart > 0 ?
                                    textbox.Text[textbox.SelectionStart - 1] :
                                    placeHolder;

                char maskChar = escapedMask[newSelectionIndex];

                // If dynamic character a,9,* or custom
                if (representationDictionary.ContainsKey(maskChar) && !escapedChars.Contains(newSelectionIndex))
                {
                    string pattern = representationDictionary[maskChar];
                    if (Regex.IsMatch(selectedChar.ToString(), pattern))
                    {
                        textArray[newSelectionIndex] = selectedChar;

                        // updating text box new index
                        newSelectionIndex++;
                    }

                    // character doesn't match the pattern get the old character
                    else
                    {
                        // if single press don't change
                        textArray[newSelectionIndex] = oldSelectionLength == 0 ? oldText[newSelectionIndex] : placeHolder;
                    }
                }

                // if fixed character
                else
                {
                    textArray[newSelectionIndex] = oldText[newSelectionIndex];

                    // updating text box new index
                    newSelectionIndex++;
                }
            }

            if (isSingleSelection || isDeleteOrBackspace)
            {
                for (int i = newSelectionIndex;
                    i < (oldSelectionStart - deleteBackspaceIndex + singleOrMultiSelectionIndex);
                    i++)
                {
                    char maskChar = escapedMask[i];

                    // If dynamic character a,9,* or custom
                    textArray[i] = representationDictionary.ContainsKey(maskChar) && !escapedChars.Contains(i) ? placeHolder : oldText[i];
                }
            }

            textbox.Text = new string(textArray);
            textbox.SetValue(OldTextProperty, textbox.Text);
            textbox.SelectionStart = isDeleteOrBackspace ? newSelectionIndex : GetSelectionStart(escapedMask, escapedChars, newSelectionIndex, representationDictionary);
        }

        private static int GetSelectionStart(string mask, List<int> escapedChars, int selectionIndex, Dictionary<char, string> representationDictionary)
        {
            for (int i = selectionIndex; i < mask.Length; i++)
            {
                char maskChar = mask[i];

                // If dynamic character a,9,* or custom
                if (representationDictionary.ContainsKey(maskChar) && !escapedChars.Contains(i))
                {
                    return i;
                }
            }

            return selectionIndex;
        }
    }
}
