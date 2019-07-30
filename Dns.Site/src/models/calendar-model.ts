interface ICalendarDate {
	date: string;
	day: number;
	future: boolean;
	hasDay: boolean;
	hasTime: boolean;
	hour: number;
	minute: number;
	month: number;
	past: boolean;
	present: boolean;
	time: string;
	weekday: number;
	year: number;
}
interface ICalendarEvent {
	id: number;
	date: string;

	groupNew: number;
	groupThreat: number;
	groupDynamic: number;
	groupAttack: number;
	groupComplete: number;
	groupTotal: number;

	attackNew: number;
	attackIntersection: number;
	attackComplete: number;
	attackTotal: number;
}
export { ICalendarEvent, ICalendarDate };
