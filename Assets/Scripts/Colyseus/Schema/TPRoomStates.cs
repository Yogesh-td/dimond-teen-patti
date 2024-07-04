// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 2.0.19
// 

using Colyseus.Schema;
using Action = System.Action;

namespace TeenPatti.ColyseusStates {
	public partial class TPRoomStates : Schema {
		[Type(0, "string")]
		public string table_id = default(string);

		[Type(1, "map", typeof(MapSchema<PlayerStates>))]
		public MapSchema<PlayerStates> all_players = new MapSchema<PlayerStates>();

		[Type(2, "array", typeof(ArraySchema<string>), "string")]
		public ArraySchema<string> all_seats = new ArraySchema<string>();

		[Type(3, "int32")]
		public int turn_time = default(int);

		[Type(4, "ref", typeof(RoundStates))]
		public RoundStates current_round = new RoundStates();

		/*
		 * Support for individual property change callbacks below...
		 */

		protected event PropertyChangeHandler<string> __table_idChange;
		public Action OnTable_idChange(PropertyChangeHandler<string> __handler, bool __immediate = true) {
			if (__callbacks == null) { __callbacks = new SchemaCallbacks(); }
			__callbacks.AddPropertyCallback(nameof(this.table_id));
			__table_idChange += __handler;
			if (__immediate && this.table_id != default(string)) { __handler(this.table_id, default(string)); }
			return () => {
				__callbacks.RemovePropertyCallback(nameof(table_id));
				__table_idChange -= __handler;
			};
		}

		protected event PropertyChangeHandler<MapSchema<PlayerStates>> __all_playersChange;
		public Action OnAll_playersChange(PropertyChangeHandler<MapSchema<PlayerStates>> __handler, bool __immediate = true) {
			if (__callbacks == null) { __callbacks = new SchemaCallbacks(); }
			__callbacks.AddPropertyCallback(nameof(this.all_players));
			__all_playersChange += __handler;
			if (__immediate && this.all_players != null) { __handler(this.all_players, null); }
			return () => {
				__callbacks.RemovePropertyCallback(nameof(all_players));
				__all_playersChange -= __handler;
			};
		}

		protected event PropertyChangeHandler<ArraySchema<string>> __all_seatsChange;
		public Action OnAll_seatsChange(PropertyChangeHandler<ArraySchema<string>> __handler, bool __immediate = true) {
			if (__callbacks == null) { __callbacks = new SchemaCallbacks(); }
			__callbacks.AddPropertyCallback(nameof(this.all_seats));
			__all_seatsChange += __handler;
			if (__immediate && this.all_seats != null) { __handler(this.all_seats, null); }
			return () => {
				__callbacks.RemovePropertyCallback(nameof(all_seats));
				__all_seatsChange -= __handler;
			};
		}

		protected event PropertyChangeHandler<int> __turn_timeChange;
		public Action OnTurn_timeChange(PropertyChangeHandler<int> __handler, bool __immediate = true) {
			if (__callbacks == null) { __callbacks = new SchemaCallbacks(); }
			__callbacks.AddPropertyCallback(nameof(this.turn_time));
			__turn_timeChange += __handler;
			if (__immediate && this.turn_time != default(int)) { __handler(this.turn_time, default(int)); }
			return () => {
				__callbacks.RemovePropertyCallback(nameof(turn_time));
				__turn_timeChange -= __handler;
			};
		}

		protected event PropertyChangeHandler<RoundStates> __current_roundChange;
		public Action OnCurrent_roundChange(PropertyChangeHandler<RoundStates> __handler, bool __immediate = true) {
			if (__callbacks == null) { __callbacks = new SchemaCallbacks(); }
			__callbacks.AddPropertyCallback(nameof(this.current_round));
			__current_roundChange += __handler;
			if (__immediate && this.current_round != null) { __handler(this.current_round, null); }
			return () => {
				__callbacks.RemovePropertyCallback(nameof(current_round));
				__current_roundChange -= __handler;
			};
		}

		protected override void TriggerFieldChange(DataChange change) {
			switch (change.Field) {
				case nameof(table_id): __table_idChange?.Invoke((string) change.Value, (string) change.PreviousValue); break;
				case nameof(all_players): __all_playersChange?.Invoke((MapSchema<PlayerStates>) change.Value, (MapSchema<PlayerStates>) change.PreviousValue); break;
				case nameof(all_seats): __all_seatsChange?.Invoke((ArraySchema<string>) change.Value, (ArraySchema<string>) change.PreviousValue); break;
				case nameof(turn_time): __turn_timeChange?.Invoke((int) change.Value, (int) change.PreviousValue); break;
				case nameof(current_round): __current_roundChange?.Invoke((RoundStates) change.Value, (RoundStates) change.PreviousValue); break;
				default: break;
			}
		}
	}
}
