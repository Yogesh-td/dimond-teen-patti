// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 2.0.19
// 

using Colyseus.Schema;
using Action = System.Action;

namespace TeenPatti.ColyseusStates {
	public partial class RoundStates : Schema {
		[Type(0, "int8")]
		public sbyte status = default(sbyte);

		[Type(1, "array", typeof(ArraySchema<RoundPlayerStates>))]
		public ArraySchema<RoundPlayerStates> players = new ArraySchema<RoundPlayerStates>();

		[Type(2, "int32")]
		public int pot_amount = default(int);

		[Type(3, "int32")]
		public int turn_player_index = default(int);

		[Type(4, "boolean")]
		public bool can_show = default(bool);

		[Type(5, "string")]
		public string joker_card_data = default(string);

		/*
		 * Support for individual property change callbacks below...
		 */

		protected event PropertyChangeHandler<sbyte> __statusChange;
		public Action OnStatusChange(PropertyChangeHandler<sbyte> __handler, bool __immediate = true) {
			if (__callbacks == null) { __callbacks = new SchemaCallbacks(); }
			__callbacks.AddPropertyCallback(nameof(this.status));
			__statusChange += __handler;
			if (__immediate && this.status != default(sbyte)) { __handler(this.status, default(sbyte)); }
			return () => {
				__callbacks.RemovePropertyCallback(nameof(status));
				__statusChange -= __handler;
			};
		}

		protected event PropertyChangeHandler<ArraySchema<RoundPlayerStates>> __playersChange;
		public Action OnPlayersChange(PropertyChangeHandler<ArraySchema<RoundPlayerStates>> __handler, bool __immediate = true) {
			if (__callbacks == null) { __callbacks = new SchemaCallbacks(); }
			__callbacks.AddPropertyCallback(nameof(this.players));
			__playersChange += __handler;
			if (__immediate && this.players != null) { __handler(this.players, null); }
			return () => {
				__callbacks.RemovePropertyCallback(nameof(players));
				__playersChange -= __handler;
			};
		}

		protected event PropertyChangeHandler<int> __pot_amountChange;
		public Action OnPot_amountChange(PropertyChangeHandler<int> __handler, bool __immediate = true) {
			if (__callbacks == null) { __callbacks = new SchemaCallbacks(); }
			__callbacks.AddPropertyCallback(nameof(this.pot_amount));
			__pot_amountChange += __handler;
			if (__immediate && this.pot_amount != default(int)) { __handler(this.pot_amount, default(int)); }
			return () => {
				__callbacks.RemovePropertyCallback(nameof(pot_amount));
				__pot_amountChange -= __handler;
			};
		}

		protected event PropertyChangeHandler<int> __turn_player_indexChange;
		public Action OnTurn_player_indexChange(PropertyChangeHandler<int> __handler, bool __immediate = true) {
			if (__callbacks == null) { __callbacks = new SchemaCallbacks(); }
			__callbacks.AddPropertyCallback(nameof(this.turn_player_index));
			__turn_player_indexChange += __handler;
			if (__immediate && this.turn_player_index != default(int)) { __handler(this.turn_player_index, default(int)); }
			return () => {
				__callbacks.RemovePropertyCallback(nameof(turn_player_index));
				__turn_player_indexChange -= __handler;
			};
		}

		protected event PropertyChangeHandler<bool> __can_showChange;
		public Action OnCan_showChange(PropertyChangeHandler<bool> __handler, bool __immediate = true) {
			if (__callbacks == null) { __callbacks = new SchemaCallbacks(); }
			__callbacks.AddPropertyCallback(nameof(this.can_show));
			__can_showChange += __handler;
			if (__immediate && this.can_show != default(bool)) { __handler(this.can_show, default(bool)); }
			return () => {
				__callbacks.RemovePropertyCallback(nameof(can_show));
				__can_showChange -= __handler;
			};
		}

		protected event PropertyChangeHandler<string> __joker_card_dataChange;
		public Action OnJoker_card_dataChange(PropertyChangeHandler<string> __handler, bool __immediate = true) {
			if (__callbacks == null) { __callbacks = new SchemaCallbacks(); }
			__callbacks.AddPropertyCallback(nameof(this.joker_card_data));
			__joker_card_dataChange += __handler;
			if (__immediate && this.joker_card_data != default(string)) { __handler(this.joker_card_data, default(string)); }
			return () => {
				__callbacks.RemovePropertyCallback(nameof(joker_card_data));
				__joker_card_dataChange -= __handler;
			};
		}

		protected override void TriggerFieldChange(DataChange change) {
			switch (change.Field) {
				case nameof(status): __statusChange?.Invoke((sbyte) change.Value, (sbyte) change.PreviousValue); break;
				case nameof(players): __playersChange?.Invoke((ArraySchema<RoundPlayerStates>) change.Value, (ArraySchema<RoundPlayerStates>) change.PreviousValue); break;
				case nameof(pot_amount): __pot_amountChange?.Invoke((int) change.Value, (int) change.PreviousValue); break;
				case nameof(turn_player_index): __turn_player_indexChange?.Invoke((int) change.Value, (int) change.PreviousValue); break;
				case nameof(can_show): __can_showChange?.Invoke((bool) change.Value, (bool) change.PreviousValue); break;
				case nameof(joker_card_data): __joker_card_dataChange?.Invoke((string) change.Value, (string) change.PreviousValue); break;
				default: break;
			}
		}
	}
}
