import format from 'date-fns/format';
import addDays from 'date-fns/addDays';

class DateRangeCommonDates {
	public static dateFormat: string = 'YYYY-MM-DD';
	public static today: Date = new Date();
	public static todayStr = format(DateRangeCommonDates.today, DateRangeCommonDates.dateFormat);
	public static prevWeek = format(addDays(DateRangeCommonDates.today, -7), DateRangeCommonDates.dateFormat);
	public static prevMonth = format(addDays(DateRangeCommonDates.today, -30), DateRangeCommonDates.dateFormat);
	public static prevYear = format(addDays(DateRangeCommonDates.today, -365), DateRangeCommonDates.dateFormat);

	public static userDisplayDateFormat: string = 'DD.MM.YYYY';
	public static formatDate(dateObj: Date): string { return format(dateObj, this.userDisplayDateFormat); }
}

class OptionPresets {
	public static DefaultPresets: OptionPresets[] = [
		{ label: 'Текущие', range: [DateRangeCommonDates.todayStr, DateRangeCommonDates.todayStr] },
		{ label: 'Неделя', range: [DateRangeCommonDates.prevWeek, DateRangeCommonDates.todayStr] },
		{ label: 'Месяц', range: [DateRangeCommonDates.prevMonth, DateRangeCommonDates.todayStr] },
		{ label: 'Год', range: [DateRangeCommonDates.prevYear, DateRangeCommonDates.todayStr] },
	];

	public label: string;
	public range: string[];
}

export default class DateRangeOptions {
	public static DefaultOptions: DateRangeOptions = {
		startDate: DateRangeCommonDates.todayStr,
		endDate: DateRangeCommonDates.todayStr,
		format: DateRangeCommonDates.userDisplayDateFormat,
		maxDate: DateRangeCommonDates.todayStr,
		minDate: DateRangeCommonDates.prevYear,
		presets: OptionPresets.DefaultPresets,
	};

	public static Format(date: Date): string {
		return format(date, DateRangeCommonDates.dateFormat);
	}

	public startDate: string;
	public endDate: string;
	public minDate: string;
	public maxDate: string;
	public format: string;
	public presets: OptionPresets[];
}

class DateRangeLabel {
	public static DefaultLabels: DateRangeLabel = {
		start: 'Дата с',
		end: 'Дата по',
		preset: 'Наборы',
	};

	public start: string;
	public end: string;
	public preset: string;
}

export { DateRangeLabel, OptionPresets, DateRangeCommonDates };
