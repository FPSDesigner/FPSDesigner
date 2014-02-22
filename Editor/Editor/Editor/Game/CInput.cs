using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Engine.Game
{
    /// <summary>
    /// CInput manage the different inputs and controlers
    /// </summary>
    class CInput
    {
        private string _clipboardStack = "";

        /// <summary>
        /// Constructor, Initialize the class
        /// </summary>
        public CInput()
        {
        }

        /// <summary>
        /// Get the content of the clipboard
        /// </summary>
        /// <returns>The content of the clipboard</returns>
        public String GetClipboardText()
        {
            Thread t = new Thread(getClipboard);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            while (t.IsAlive) {}
            return _clipboardStack;
        }

        /// <summary>
        /// Used internally by a Thread with GetClipboardText()
        /// </summary>
        private void getClipboard()
        {
            if (System.Windows.Forms.Clipboard.ContainsText())
            {
                _clipboardStack = System.Windows.Forms.Clipboard.GetText();
            }
        }

        /// <summary>
        /// Returns the keyboard layout the player is using.
        /// Example: AZERTY, QWERTY, ...
        /// </summary>
        /// <returns>The player's keyboard layout</returns>
        public string getKeyboardType()
        {
            if (System.Windows.Forms.InputLanguage.CurrentInputLanguage.Culture.Name == "fr-FR")
                return "AZERTY";
            else
                return "QWERTY";
        }

        /// <summary>
        /// Returns a string with the real typed key, using the player keyboard's layout
        /// </summary>
        /// <param name="key">The key we want to check</param>
        /// <param name="caps">True if caps are enabled</param>
        /// <returns></returns>
        public string getRealTypedKey(Keys key, bool caps)
        {
            // No localization needed keys:
            switch (key)
            {
                case Keys.A: if (caps) return "A"; else return "a";
                case Keys.B: if (caps) return "B"; else return "b";
                case Keys.C: if (caps) return "C"; else return "c";
                case Keys.D: if (caps) return "D"; else return "d";
                case Keys.E: if (caps) return "E"; else return "e";
                case Keys.F: if (caps) return "F"; else return "f";
                case Keys.G: if (caps) return "G"; else return "g";
                case Keys.H: if (caps) return "H"; else return "h";
                case Keys.I: if (caps) return "I"; else return "i";
                case Keys.J: if (caps) return "J"; else return "j";
                case Keys.K: if (caps) return "K"; else return "k";
                case Keys.L: if (caps) return "L"; else return "l";
                case Keys.M: if (caps) return "M"; else return "m";
                case Keys.N: if (caps) return "N"; else return "n";
                case Keys.O: if (caps) return "O"; else return "o";
                case Keys.P: if (caps) return "P"; else return "p";
                case Keys.Q: if (caps) return "Q"; else return "q";
                case Keys.R: if (caps) return "R"; else return "r";
                case Keys.S: if (caps) return "S"; else return "s";
                case Keys.T: if (caps) return "T"; else return "t";
                case Keys.U: if (caps) return "U"; else return "u";
                case Keys.V: if (caps) return "V"; else return "v";
                case Keys.W: if (caps) return "W"; else return "w";
                case Keys.X: if (caps) return "X"; else return "x";
                case Keys.Y: if (caps) return "Y"; else return "y";
                case Keys.Z: if (caps) return "Z"; else return "z";

                case Keys.NumPad0: return "0";
                case Keys.NumPad1: return "1";
                case Keys.NumPad2: return "2";
                case Keys.NumPad3: return "3";
                case Keys.NumPad4: return "4";
                case Keys.NumPad5: return "5";
                case Keys.NumPad6: return "6";
                case Keys.NumPad7: return "7";
                case Keys.NumPad8: return "8";
                case Keys.NumPad9: return "9";

                case Keys.Space: return " ";
                case Keys.Add: return "+";
                case Keys.Subtract: return "-";
                case Keys.Multiply: return "*";
                case Keys.Divide: return "/";
                case Keys.Tab: return "    ";
                case Keys.Decimal: return ".";
            }

            // Localization needed
            string keyboardType = getKeyboardType();
            if (keyboardType == "QWERTY")
            {
                switch (key)
                {
                    case Keys.D0: if (caps) return ")"; else return "0";
                    case Keys.D1: if (caps) return "!"; else return "1";
                    case Keys.D2: if (caps) return "@"; else return "2";
                    case Keys.D3: if (caps) return "#"; else return "3";
                    case Keys.D4: if (caps) return "$"; else return "4";
                    case Keys.D5: if (caps) return "%"; else return "5";
                    case Keys.D6: if (caps) return "^"; else return "6";
                    case Keys.D7: if (caps) return "&"; else return "7";
                    case Keys.D8: if (caps) return "*"; else return "8";
                    case Keys.D9: if (caps) return "("; else return "9";

                    case Keys.OemTilde: if (caps) return "~"; else return "`"; 
                    case Keys.OemSemicolon: if (caps) return ":"; else return ";"; 
                    case Keys.OemQuotes: if (caps) return "\""; else return "'"; 
                    case Keys.OemQuestion: if (caps) return "?"; else return "/"; 
                    case Keys.OemPlus: if (caps) return "+"; else return "="; 
                    case Keys.OemPipe: if (caps) return "|"; else return "\\"; 
                    case Keys.OemPeriod: if (caps) return ">"; else return "."; 
                    case Keys.OemOpenBrackets: if (caps) return ""; else return "["; 
                    case Keys.OemCloseBrackets: if (caps) return "}"; else return "]"; 
                    case Keys.OemMinus: if (caps) return "_"; else return "-"; 
                    case Keys.OemComma: if (caps) return "<"; else return ","; 
                }
            }
            else if (keyboardType == "AZERTY")
            {
                switch (key)
                {
                    case Keys.D0: if (caps) return "0"; else return "à";
                    case Keys.D1: if (caps) return "1"; else return "&";
                    case Keys.D2: if (caps) return "2"; else return "é";
                    case Keys.D3: if (caps) return "3"; else return "\"";
                    case Keys.D4: if (caps) return "4"; else return "'";
                    case Keys.D5: if (caps) return "5"; else return "(";
                    case Keys.D6: if (caps) return "6"; else return "-";
                    case Keys.D7: if (caps) return "7"; else return "è";
                    case Keys.D8: if (caps) return "8"; else return "_";
                    case Keys.D9: if (caps) return "9"; else return "ç";

                    case Keys.OemTilde: if (caps) return "%"; else return "ù"; 
                    case Keys.OemSemicolon: if (caps) return "$"; else return "£"; 
                    case Keys.OemQuotes: if (caps) return "²"; else return "²"; 
                    case Keys.OemQuestion: if (caps) return "/"; else return ":"; 
                    case Keys.OemPlus: if (caps) return "+"; else return "="; 
                    case Keys.OemPipe: if (caps) return "µ"; else return "*"; 
                    case Keys.OemPeriod: if (caps) return "."; else return ";"; 
                    case Keys.OemOpenBrackets: if (caps) return "°"; else return ")"; 
                    case Keys.OemCloseBrackets: if (caps) return "¨"; else return "^"; 
                    case Keys.OemMinus: if (caps) return "_"; else return "-"; 
                    case Keys.OemComma: if (caps) return "?"; else return ",";
                    case Keys.Oem8: if (caps) return "§"; else return "!";
                }
            }


            return "";
        }



    }
}
