﻿// 
// Teensy Bat Explorer - Copyright(C) 2017 Meinard Jean-Richard
//  
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//  
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//  
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace TeensyBatExplorer.Controls
{
    public sealed class HeaderControl : ContentControl
    {
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header", typeof(object), typeof(HeaderControl), new PropertyMetadata(default(object)));

        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public HeaderControl()
        {
            DefaultStyleKey = typeof(HeaderControl);
        }
    }

    public class IconButton : Button
    {
        public static readonly DependencyProperty SymbolProperty = DependencyProperty.Register(
            "Symbol", typeof(Symbol), typeof(IconButton), new PropertyMetadata(Symbol.Accept));

        public Symbol Symbol
        {
            get { return (Symbol)GetValue(SymbolProperty); }
            set { SetValue(SymbolProperty, value); }
        }

        public IconButton()
        {
            DefaultStyleKey = typeof(Button);
        }
    }
}