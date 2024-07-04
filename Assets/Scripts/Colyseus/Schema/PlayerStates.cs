// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 2.0.19
// 

using Colyseus.Schema;
using Action = System.Action;

namespace TeenPatti.ColyseusStates {
	public partial class PlayerStates : Schema {
		[Type(0, "string")]
		public string social_id = default(string);

		[Type(1, "string")]
		public string username = default(string);

		[Type(2, "int32")]
		public int avatar_index = default(int);

		/*
		 * Support for individual property change callbacks below...
		 */

		protected event PropertyChangeHandler<string> __social_idChange;
		public Action OnSocial_idChange(PropertyChangeHandler<string> __handler, bool __immediate = true) {
			if (__callbacks == null) { __callbacks = new SchemaCallbacks(); }
			__callbacks.AddPropertyCallback(nameof(this.social_id));
			__social_idChange += __handler;
			if (__immediate && this.social_id != default(string)) { __handler(this.social_id, default(string)); }
			return () => {
				__callbacks.RemovePropertyCallback(nameof(social_id));
				__social_idChange -= __handler;
			};
		}

		protected event PropertyChangeHandler<string> __usernameChange;
		public Action OnUsernameChange(PropertyChangeHandler<string> __handler, bool __immediate = true) {
			if (__callbacks == null) { __callbacks = new SchemaCallbacks(); }
			__callbacks.AddPropertyCallback(nameof(this.username));
			__usernameChange += __handler;
			if (__immediate && this.username != default(string)) { __handler(this.username, default(string)); }
			return () => {
				__callbacks.RemovePropertyCallback(nameof(username));
				__usernameChange -= __handler;
			};
		}

		protected event PropertyChangeHandler<int> __avatar_indexChange;
		public Action OnAvatar_indexChange(PropertyChangeHandler<int> __handler, bool __immediate = true) {
			if (__callbacks == null) { __callbacks = new SchemaCallbacks(); }
			__callbacks.AddPropertyCallback(nameof(this.avatar_index));
			__avatar_indexChange += __handler;
			if (__immediate && this.avatar_index != default(int)) { __handler(this.avatar_index, default(int)); }
			return () => {
				__callbacks.RemovePropertyCallback(nameof(avatar_index));
				__avatar_indexChange -= __handler;
			};
		}

		protected override void TriggerFieldChange(DataChange change) {
			switch (change.Field) {
				case nameof(social_id): __social_idChange?.Invoke((string) change.Value, (string) change.PreviousValue); break;
				case nameof(username): __usernameChange?.Invoke((string) change.Value, (string) change.PreviousValue); break;
				case nameof(avatar_index): __avatar_indexChange?.Invoke((int) change.Value, (int) change.PreviousValue); break;
				default: break;
			}
		}
	}
}
