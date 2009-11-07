﻿using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;

namespace ICSharpCode.Core.Presentation
{
	/// <summary>
	/// Describes full key gesture or part of a key gesture
	/// 
	/// This class is is designed to react to key events even it descibes only part of them
	/// For example if event argument holds modifiers Ctrl, Shift and key C then partial template
	/// with single modifier Ctrl and a key C will match this event
	/// </summary>
	[TypeConverter(typeof(PartialKeyGestureConverter))]
	public class PartialKeyGesture : KeyGesture
	{
		private readonly Key _key;
		private readonly ModifierKeys _modifiers;
		
		/// <summary>
		/// Gets key associated with partial key gesture
		/// </summary>
		public new Key Key
		{
			get {
				return _key;
			}
		}
		
		/// <summary>
		/// Gets modifier keys associated with partial key gesture
		/// </summary>
		public new ModifierKeys Modifiers
		{
			get {
				return _modifiers;
			}
		}
		
		/// <summary>
		/// Create new instance of <see cref="PartialKeyGesture"/> from <see cref="KeyEventArgs"/>
		/// </summary>
		/// <param name="keyEventArgs">Arguments generated by key event</param>
		public PartialKeyGesture(KeyEventArgs keyEventArgs)
			: base(Key.None, ModifierKeys.None)
		{
			var keyboardDevice = (KeyboardDevice)keyEventArgs.Device;
			
			var enteredKey = keyEventArgs.Key == Key.System ? keyEventArgs.SystemKey : keyEventArgs.Key;
			var enteredModifiers = keyboardDevice.Modifiers;
			
			if(Array.IndexOf(new[] {  Key.LeftAlt, Key.RightAlt, 
			   Key.LeftShift, Key.RightShift,
			   Key.LeftCtrl, Key.RightCtrl,
			   Key.LWin, Key.RWin}, enteredKey) >= 0) {
				_key = Key.None;
				_modifiers = enteredModifiers;
			} else {
				_key = enteredKey;
				_modifiers = enteredModifiers;
			}
		}
		
		/// <summary>
		/// Create new instance of <see cref="PartialKeyGesture"/> from <see cref="KeyGesture"/> 
		/// </summary>
		/// <param name="gesture">Key gesture</param>
		public PartialKeyGesture(KeyGesture gesture)
			: base(Key.None, ModifierKeys.None)
		{
			var partialKeyGesture = gesture as PartialKeyGesture;
			
			if (partialKeyGesture != null) {
				_key = partialKeyGesture.Key;
				_modifiers = partialKeyGesture.Modifiers;
			} else if (gesture is MultiKeyGesture) {
				throw new ArgumentException("Can not create partial key gesture from multi-key gesture");
			} else {
				_key = gesture.Key;
				_modifiers = gesture.Modifiers;
			}
		}
		
		/// <summary>
		/// Create new instance of <see cref="PartialKeyGesture"/> having key and modifiers
		/// </summary>
		/// <param name="key">The key associated with partial key gesture</param>
		/// <param name="modifiers">Modifier keys associated with partial key gesture</param>
		public PartialKeyGesture(Key key, ModifierKeys modifiers)
			: base(Key.None, ModifierKeys.None)
		{
			_key = key;
			_modifiers = modifiers;
		}
		
		/// <summary>
		/// Create new instance of<see cref="PartialKeyGesture"/> having only key and no modifiers
		/// </summary>
		/// <param name="key">The key associated with partial key gesture</param>
		public PartialKeyGesture(Key key)
		: base(Key.None, ModifierKeys.None)
		{
			_key = key;
			_modifiers = ModifierKeys.None;
		}
		
		/// <summary>
		/// Create new instance of<see cref="PartialKeyGesture"/> having only key and no modifiers
		/// </summary>
		/// <param name="modifiers">Modifier keys associated with partial key gesture</param>
		public PartialKeyGesture(ModifierKeys modifiers)
		: base(Key.None, ModifierKeys.None)
		{
			_key = Key.None;
			_modifiers = modifiers;
		}
		
		
		/// <summary>
		/// Determines whether input event supporting data strictly (all modifiers and keys) matches this instance of <see cref="PartialKeyGesture"/>
		/// </summary>
		/// <param name="targetElement">The target</param>
		/// <param name="inputEventArgs">Input event arguments</param>
		/// <returns><code>true</code> if event data matches partial gesture; otherwise <code>false</code></returns>
		public bool StrictlyMatches(object targetElement, InputEventArgs args)
		{
			var keyEventArgs = args as KeyEventArgs;
			if(keyEventArgs == null) {
				return false;
			}
			
			var keyboard = (KeyboardDevice)keyEventArgs.Device;
			
			// If system key is pressed
			if (keyEventArgs.Key == Key.System) {
				return keyboard.Modifiers == Modifiers && keyEventArgs.SystemKey == Key;
			}
			
			return keyboard.Modifiers == Modifiers && keyEventArgs.Key == Key;
		}
		
		/// <summary>
		/// Determines whether input event arguments partly matches (part of modifiers or/and a key) this instance of <see cref="PartialKeyGesture"/>
		/// </summary>
		/// <param name="targetElement">The target</param>
		/// <param name="inputEventArgs">Input event arguments</param>
		/// <returns><code>true</code> if event data matches partial gesture; otherwise <code>false</code></returns>
		public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
		{
			var keyEventArgs = inputEventArgs as KeyEventArgs;
			if(keyEventArgs == null) {
			return false;
			}
			
			var keyboard = (KeyboardDevice)keyEventArgs.Device;
			
			// When system key (Alt) is pressed real key is moved to SystemKey property
			var enteredKey = keyEventArgs.Key == Key.System ? keyEventArgs.SystemKey : keyEventArgs.Key;
			
			var keyMatches = Key == enteredKey;
			
			// Determine whether template contains only part of modifier keys contained in
			// gesture. For example if template contains Control modifier, but gesture contains
			// Control and Alt true will be returned
			var modifierMatches = keyboard.Modifiers - (keyboard.Modifiers ^ Modifiers) >= 0;
			
			// Template contains no modifiers compare only keys
			if (Modifiers == ModifierKeys.None) {
			return keyMatches;
			}
			
			// If template has modifiers but key is one of modifier keys return true if
			// modifiers match. This is used because when user presses modifier key it is 
			// presented in Key property and Modifiers property
			if (Array.IndexOf(new[] {  Key.LeftAlt, Key.RightAlt, 
			   Key.LeftShift, Key.RightShift,
			   Key.LeftCtrl, Key.RightCtrl,
			   Key.LWin, Key.RWin,
			   Key.System}, enteredKey) >= 0) {
				return modifierMatches;
			}
			
			return modifierMatches && keyMatches;
		}
		
		/// <summary>
		/// Gets value indicating whether this InputGesture is completed
		/// 
		/// Incomplete gestures are ones which have only modifiers or only
		/// keys assigned
		/// </summary>
		public bool IsFull
		{
			get {
				if(Key == Key.None) {
					return false;
				}
				
				// and function keys are valid without modifier
				if (Key >= Key.F1 && Key <= Key.F24) {
					return true;
				}
				
				// Modifiers alone are not valid
				if (Array.IndexOf(new[] { Key.LeftAlt, Key.RightAlt, 
				   Key.LeftShift, Key.RightShift,
				   Key.LeftCtrl, Key.RightCtrl,
				   Key.LWin, Key.RWin,
				   Key.System}, Key) > -1) {
					return false;
				}
				
				// All other gestures must have modifier (except shift alone because it would mean uppercase)
				if((Modifiers & (ModifierKeys.Windows | ModifierKeys.Control | ModifierKeys.Alt)) != ModifierKeys.None) {
					return true;   	
				}
				
				return false;
			}
		}
		
		/// <summary>
		/// Creates string that represents <see cref="PartialKeyGesture"/>
		/// </summary>
		/// <returns>String representation of this object</returns>
		public override string ToString()
		{
			var pressedButton = new StringBuilder();
			
			if (Modifiers != ModifierKeys.None) {
			pressedButton.AppendFormat("{0}+", new ModifierKeysConverter().ConvertToInvariantString(Modifiers));
			}
			
			// Filter modifier keys from being displayed twice (example: Ctrl + LeftCtrl)
			if (Array.IndexOf(new[] { Key.LeftAlt, Key.RightAlt, 
			   Key.LeftShift, Key.RightShift,
			   Key.LeftCtrl, Key.RightCtrl,
			   Key.LWin, Key.RWin,
			   Key.System}, Key) < 0) {
				pressedButton.Append(new KeyConverter().ConvertToInvariantString(Key));
			}
			
			return pressedButton.ToString();
		}
	}
}
