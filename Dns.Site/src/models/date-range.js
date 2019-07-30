import format from 'date-fns/format';
import addDays from 'date-fns/add_days';
var DateRangeCommonDates = /** @class */ (function () {
    function DateRangeCommonDates() {
    }
    DateRangeCommonDates.formatDate = function (dateObj) { return format(dateObj, this.userDisplayDateFormat); };
    DateRangeCommonDates.dateFormat = 'YYYY-MM-DD';
    DateRangeCommonDates.today = new Date();
    DateRangeCommonDates.todayStr = format(DateRangeCommonDates.today, DateRangeCommonDates.dateFormat);
    DateRangeCommonDates.prevWeek = format(addDays(DateRangeCommonDates.today, -7), DateRangeCommonDates.dateFormat);
    DateRangeCommonDates.prevMonth = format(addDays(DateRangeCommonDates.today, -30), DateRangeCommonDates.dateFormat);
    DateRangeCommonDates.prevYear = format(addDays(DateRangeCommonDates.today, -365), DateRangeCommonDates.dateFormat);
    DateRangeCommonDates.userDisplayDateFormat = 'DD.MM.YYYY';
    return DateRangeCommonDates;
}());
var OptionPresets = /** @class */ (function () {
    function OptionPresets() {
    }
    OptionPresets.DefaultPresets = [
        { label: 'Текущие', range: [DateRangeCommonDates.todayStr, DateRangeCommonDates.todayStr] },
        { label: 'Неделя', range: [DateRangeCommonDates.prevWeek, DateRangeCommonDates.todayStr] },
        { label: 'Месяц', range: [DateRangeCommonDates.prevMonth, DateRangeCommonDates.todayStr] },
        { label: 'Год', range: [DateRangeCommonDates.prevYear, DateRangeCommonDates.todayStr] },
    ];
    return OptionPresets;
}());
var DateRangeOptions = /** @class */ (function () {
    function DateRangeOptions() {
    }
    DateRangeOptions.Format = function (date) {
        return format(date, DateRangeCommonDates.dateFormat);
    };
    DateRangeOptions.DefaultOptions = {
        startDate: DateRangeCommonDates.todayStr,
        endDate: DateRangeCommonDates.todayStr,
        format: DateRangeCommonDates.userDisplayDateFormat,
        maxDate: DateRangeCommonDates.todayStr,
        minDate: DateRangeCommonDates.prevYear,
        presets: OptionPresets.DefaultPresets,
    };
    return DateRangeOptions;
}());
export default DateRangeOptions;
var DateRangeLabel = /** @class */ (function () {
    function DateRangeLabel() {
    }
    DateRangeLabel.DefaultLabels = {
        start: 'Дата с',
        end: 'Дата по',
        preset: 'Наборы',
    };
    return DateRangeLabel;
}());
export { DateRangeLabel, OptionPresets, DateRangeCommonDates };
//# sourceMappingURL=date-range.js.map