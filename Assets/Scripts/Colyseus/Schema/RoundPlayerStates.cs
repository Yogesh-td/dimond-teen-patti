// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 2.0.19
// 

using Colyseus.Schema;
using Action = System.Action;

namespace TeenPatti.ColyseusStates {
	public partial class RoundPlayerStates : Schema {
		[Type(0, "string")]
		public string social_id = default(string);

		[Type(1, "int32")]
		public int balance = default(int);

		[Type(2, "int32")]
		public int current_bet_amount = default(int);

		[Type(3, "int32")]
		public int last_bet_amount = default(int);

		[Type(4, "boolean")]
		public bool isDealer = default(bool);

		[Type(5, "boolean")]
		public bool isblind = default(bool);

		[Type(6, "boolean")]
		public bool isbetlimitreached = default(bool);

		[Type(7, "boolean")]
		public bool ispack = default(bool);

		[Type(8, "boolean")]
		public bool cansideshow = default(bool);

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

		protected event PropertyChangeHandler<int> __balanceChange;
		public Action OnBalanceChange(PropertyChangeHandler<int> __handler, bool __immediate = true) {
			if (__callbacks == null) { __callbacks = new SchemaCallbacks(); }
			__callbacks.AddPropertyCallback(nameof(this.balance));
			__balanceChange += __handler;
			if (__immediate && this.balance != default(int)) { __handler(this.balance, default(int)); }
			return () => {
				__callbacks.RemovePropertyCallback(nameof(balance));
				__balanceChange -= __handler;
			};
		}

		protected event PropertyChangeHandler<int> __current_bet_amountChange;
		public Action OnCurrent_bet_amountChange(PropertyChangeHandler<int> __handler, bool __immediate = true) {
			if (__callbacks == null) { __callbacks = new SchemaCallbacks(); }
			__callbacks.AddPropertyCallback(nameof(this.current_bet_amount));
			__current_bet_amountChange += __handler;
			if (__immediate && this.current_bet_amount != default(int)) { __handler(this.current_bet_amount, default(int)); }
			return () => {
				__callbacks.RemovePropertyCallback(nameof(current_bet_amount));
				__current_bet_amountChange -= __handler;
			};
		}

		protected event PropertyChangeHandler<int> __last_bet_amountChange;
		public Action OnLast_bet_amountChange(PropertyChangeHandler<int> __handler, bool __immediate = true) {
			if (__callbacks == null) { __callbacks = new SchemaCallbacks(); }
			__callbacks.AddPropertyCallback(nameof(this.last_bet_amount));
			__last_bet_amountChange += __handler;
			if (__immediate && this.last_bet_amount != default(int)) { __handler(this.last_bet_amount, default(int)); }
			return () => {
				__callbacks.RemovePropertyCallback(nameof(last_bet_amount));
				__last_bet_amountChange -= __handler;
			};
		}

		protected event PropertyChangeHandler<bool> __isDealerChange;
		public Action OnIsDealerChange(PropertyChangeHandler<bool> __handler, bool __immediate = true) {
			if (__callbacks == null) { __callbacks = new SchemaCallbacks(); }
			__callbacks.AddPropertyCallback(nameof(this.isDealer));
			__isDealerChange += __handler;
			if (__immediate && this.isDealer != default(bool)) { __handler(this.isDealer, default(bool)); }
			return () => {
				__callbacks.RemovePropertyCallback(nameof(isDealer));
				__isDealerChange -= __handler;
			};
		}

		protected event PropertyChangeHandler<bool> __isblindChange;
		public Action OnIsblindChange(PropertyChangeHandler<bool> __handler, bool __immediate = true) {
			if (__callbacks == null) { __callbacks = new SchemaCallbacks(); }
			__callbacks.AddPropertyCallback(nameof(this.isblind));
			__isblindChange += __handler;
			if (__immediate && this.isblind != default(bool)) { __handler(this.isblind, default(bool)); }
			return () => {
				__callbacks.RemovePropertyCallback(nameof(isblind));
				__isblindChange -= __handler;
			};
		}

		protected event PropertyChangeHandler<bool> __isbetlimitreachedChange;
		public Action OnIsbetlimitreachedChange(PropertyChangeHandler<bool> __handler, bool __immediate = true) {
			if (__callbacks == null) { __callbacks = new SchemaCallbacks(); }
			__callbacks.AddPropertyCallback(nameof(this.isbetlimitreached));
			__isbetlimitreachedChange += __handler;
			if (__immediate && this.isbetlimitreached != default(bool)) { __handler(this.isbetlimitreached, default(bool)); }
			return () => {
				__callbacks.RemovePropertyCallback(nameof(isbetlimitreached));
				__isbetlimitreachedChange -= __handler;
			};
		}

		protected event PropertyChangeHandler<bool> __ispackChange;
		public Action OnIspackChange(PropertyChangeHandler<bool> __handler, bool __immediate = true) {
			if (__callbacks == null) { __callbacks = new SchemaCallbacks(); }
			__callbacks.AddPropertyCallback(nameof(this.ispack));
			__ispackChange += __handler;
			if (__immediate && this.ispack != default(bool)) { __handler(this.ispack, default(bool)); }
			return () => {
				__callbacks.RemovePropertyCallback(nameof(ispack));
				__ispackChange -= __handler;
			};
		}

		protected event PropertyChangeHandler<bool> __cansideshowChange;
		public Action OnCansideshowChange(PropertyChangeHandler<bool> __handler, bool __immediate = true) {
			if (__callbacks == null) { __callbacks = new SchemaCallbacks(); }
			__callbacks.AddPropertyCallback(nameof(this.cansideshow));
			__cansideshowChange += __handler;
			if (__immediate && this.cansideshow != default(bool)) { __handler(this.cansideshow, default(bool)); }
			return () => {
				__callbacks.RemovePropertyCallback(nameof(cansideshow));
				__cansideshowChange -= __handler;
			};
		}

		protected override void TriggerFieldChange(DataChange change) {
			switch (change.Field) {
				case nameof(social_id): __social_idChange?.Invoke((string) change.Value, (string) change.PreviousValue); break;
				case nameof(balance): __balanceChange?.Invoke((int) change.Value, (int) change.PreviousValue); break;
				case nameof(current_bet_amount): __current_bet_amountChange?.Invoke((int) change.Value, (int) change.PreviousValue); break;
				case nameof(last_bet_amount): __last_bet_amountChange?.Invoke((int) change.Value, (int) change.PreviousValue); break;
				case nameof(isDealer): __isDealerChange?.Invoke((bool) change.Value, (bool) change.PreviousValue); break;
				case nameof(isblind): __isblindChange?.Invoke((bool) change.Value, (bool) change.PreviousValue); break;
				case nameof(isbetlimitreached): __isbetlimitreachedChange?.Invoke((bool) change.Value, (bool) change.PreviousValue); break;
				case nameof(ispack): __ispackChange?.Invoke((bool) change.Value, (bool) change.PreviousValue); break;
				case nameof(cansideshow): __cansideshowChange?.Invoke((bool) change.Value, (bool) change.PreviousValue); break;
				default: break;
			}
		}
	}
}
