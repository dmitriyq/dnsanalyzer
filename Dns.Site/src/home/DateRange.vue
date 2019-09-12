<template>
	<v-container fluid wrap grid-list-sm class="pb-0">
		<v-layout row wrap fluid align-start class="date-range">
			<v-flex xs12 sm2 v-if="!noPresets"
					class="date-range__presets">
				<v-list :dark="dark">
					<v-subheader>{{ labels.preset }}</v-subheader>
					<v-list-tile v-for="(preset, index) in presets"
								 v-model="isPresetActive[index]"
								 :key="index"
								 @click="onPresetSelect(index)">
						<v-list-tile-content>
							{{ preset.label }}
						</v-list-tile-content>
					</v-list-tile>
				</v-list>
			</v-flex>
			<v-flex xs12 sm3 class="date-range__picker date-range__pickers--start m-width-300">
				<v-text-field v-model="formattedStartDate"
							  :label="`${labels.start}(${format})`"
							  name="startDate"
							  class="date-range__pickers-input"
							  prepend-icon="event"
							  readonly />
				<v-date-picker :allowed-dates="allowedDates"
							   :next-icon="nextIcon"
							   :prev-icon="prevIcon"
							   :dark="dark"
							   :events="highlightRange ? dateRange.dates : events"
							   :event-color="highlightRange ? dateRange.colors : eventColor"
							   v-model="startDate"
							   :min="options.minDate"
							   :max="endDate"
							   :locale="locale"
							   :first-day-of-week="firstDayOfWeek"
							   no-title
							   :day-format="dayFormat"
							   :picker-date.sync="pickerDateStart"
							   @change="onDateRangeChange" />
			</v-flex>
			<v-flex xs12 sm3 class="date-range__picker date-range__picker--end m-width-300">
				<v-text-field :label="`${labels.end}(${format})`"
							  v-model="formattedEndDate"
							  name="endDate"
							  class="date-range__pickers-input"
							  readonly />
				<v-date-picker :next-icon="nextIcon"
							   :prev-icon="prevIcon"
							   :dark="dark"
							   :min="startDate"
							   :max="maxDate"
							   :events="highlightRange ? dateRange.dates : events"
							   :event-color="highlightRange ? dateRange.colors : eventColor"
							   v-model="endDate"
							   :locale="locale"
							   :first-day-of-week="firstDayOfWeek"
							   no-title
							   :day-format="dayFormat"
							   :picker-date.sync="pickerDateEnd"
							   @change="onDateRangeChange" />
			</v-flex>
		</v-layout>
	</v-container>
</template>

<script lang="ts">
	import Vue from 'vue';
	import { format } from 'date-fns';
	import { Component, Prop, Watch } from 'vue-property-decorator';
	import DateRangeOptions, { DateRangeLabel, OptionPresets } from '@/models/date-range';

	@Component({name: 'v-daterange'})
	export default class DateRange extends Vue {
		@Prop({ type: Object, required: true }) public options: DateRangeOptions;
		@Prop({ type: Boolean, required: false, default: false }) public noPresets: boolean;
		@Prop({ type: Boolean, required: false, default: false }) public dark: boolean;
		@Prop({ type: String, required: false, default: 'chevron_right' }) public nextIcon: string;
		@Prop({ type: String, required: false, default: 'chevron_left' }) public prevIcon: string;
		@Prop({
			type: Object, required: false, default:
				{ start: 'Start Date', end: 'End Date', preset: 'Presets' },
		}) public labels: DateRangeLabel;
		@Prop({ type: [Array, Object, Function], required: false, default: () => null })
		public events: [] | object | (() => null);
		@Prop({ type: [String, Function, Object], required: false, default: 'warning' })
		public eventColor: string | object | (() => string | object);
		@Prop({ type: Boolean, required: false, default: true }) public highlightRange: boolean;
		@Prop({ type: String, required: false, default: '' }) public highlightColors: string;
		@Prop({ type: String, required: false, default: 'en-us' }) public locale: string;
		@Prop({ type: Number, required: false, default: 0 }) public firstDayOfWeek: number;
		@Prop({ type: Array, required: true }) public calendarColoredDates: Array<({ date: string, colors: string[] })>;

		public startDate: string = this.options.startDate;
		public endDate: string = this.options.endDate;
		public format: string = this.options.format;
		public presets: OptionPresets[] = this.options.presets;
		public dateRange: ({ dates: [], colors: {} }) = { dates: [], colors: {} };
		public allowedDates: (() => string)| null = null;
		public pickerDateStart: string | null = null;
		public pickerDateEnd: string | null = null;

		get formattedStartDate(): string {
			return format(new Date(this.startDate), this.format);
		}
		get formattedEndDate(): string {
			return format(new Date(this.startDate), this.format);
		}
		get highlightColorClasses(): string {
	 		if (this.highlightColors) {
	 			return this.highlightColors;
	 		}
	 		return this.dark ? 'blue-grey darken-1' : 'blue lighten-5';
		}
		get isPresetActive(): boolean[] {
			return this.presets.map(
				(preset) => {
					return preset.range[0] === this.startDate && preset.range[1] === this.endDate;
				},
			);
		}
		get today(): string {
			return format(new Date(), 'YYYY-MM-DD');
		}
		get maxDate(): string {
			return this.options.maxDate || this.today;
		}

		public mounted() {
	 		if (this.highlightRange) {
	 			this.setInRangeData();
	 		}
		}

		public onDateRangeChange(): void {
	 		if (this.highlightRange) {
	 			this.setInRangeData();
	 		}
	 		this.$emit('input', [this.startDate, this.endDate]);
		}

		public dayFormat(date: string): string {
			const coloredDate = this.calendarColoredDates.find((val) => val.date === date);
			let additionIcons = '';
			if (!!coloredDate) {
				const colors = coloredDate.colors;
				if (!!colors && colors.length > 0) {
					colors.forEach((val, indx) => {
						additionIcons += `<span class="calendar-day-style-icon ${val}" style="top:${indx * 6}px"></span>`;
					});
				}
			}
			return `<span class="calendar-day-style">${additionIcons}${date.substr(8, 2).replace(/^0/, '')}</span>`;
		}

		@Watch('startDate')
		private onStartDateChange(val: string, oldVal: string) {
			this.onDateRangeChange();
		}
		@Watch('endDate')
		private onEndDateChange(val: string, oldVal: string) {
			this.onDateRangeChange();
		}
		@Watch('pickerDateStart')
		private onPickerDateStartChange(val: string | null, oldVal: string | null) {
			this.$emit('month-changed', val);
		}
		@Watch('pickerDateEnd')
		private onPickerDateEndChange(val: string | null, oldVal: string | null) {
			this.$emit('month-changed', val);
		}

		private onPresetSelect(presetIndex: number): void {
			this.startDate = this.presets[presetIndex].range[0];
			this.pickerDateStart = format(new Date(this.startDate), 'YYYY-MM');
			this.endDate = this.presets[presetIndex].range[1];
			this.pickerDateEnd = format(new Date(this.endDate), 'YYYY-MM');
		}

		private setInRangeData(): void {
			const inRangeData = {
				dates: [],
	 			colors: {},
	 		};
			if (this.highlightRange) {
				const startDate = new Date(this.startDate);
				const endDate = new Date(this.endDate);
				const diffDays = (+endDate - +startDate) / (1000 * 3600 * 24);

				for (let i = 0; i <= diffDays; i += 1) {
					const date = this.addDays(startDate.toDateString(), i);
					// @ts-ignore
					inRangeData.dates.push(date);
					// @ts-ignore
					inRangeData.colors[date] = `date-range__date-in-range ${this.highlightColorClasses}`;

					if (i === 0) {
						// @ts-ignore
						inRangeData.colors[date] += ' date-range__range-start';
					}
					if (i === diffDays) {
						// @ts-ignore
						inRangeData.colors[date] += ' date-range__range-end';
					}
				}
			}
			// @ts-ignore
			this.dateRange = inRangeData;
		}

		private addDays(date: string, days: number): string {
	 		const result = new Date(date);
	 		result.setDate(result.getDate() + days);
	 		return format(result, 'YYYY-MM-DD');
		}
	 }
</script>

<style scoped>
	.date-range {
		display: flex;
	}

	.date-range__presets {
		margin-right: 1rem;
	}

	.date-range__pickers {
		display: flex;
	}

	.date-range__picker {
		padding: 0 1rem;
	}

	.date-range > > > .date-picker-table table {
		border-collapse: collapse;
	}

	.date-range > > > .date-picker-table__event.date-range__date-in-range {
		z-index: 0;
		/* override existing settings */
		width: 100%;
		height: 100%;
		left: 0;
		top: 0;
		bottom: 0;
		border-radius: 0;
	}

		.date-range > > > .date-picker-table__event.date-range__date-in-range.date-range__range-start {
			border-top-left-radius: 50%;
			border-bottom-left-radius: 50%;
			/* Cover only date button */
			left: 7px;
			width: 31px;
		}

		.date-range > > > .date-picker-table__event.date-range__date-in-range.date-range__range-end {
			border-top-right-radius: 50%;
			border-bottom-right-radius: 50%;
		}

	.date-range > > > .date-picker-table .btn {
		/* fixed zIndex is needed because .date-picker-table__event div is created after the .btn button */
		z-index: 1;
	}

	.m-width-300 {
		min-width: 300px;
	}
</style>
<style>
	.calendar-day-style {
		border-radius: 50%;
		height: 32px;
		width: 32px;
		display: block;
		position: relative;
	}

	.calendar-day-style-icon {
		position: absolute !important;
		opacity: 100 !important;
		border-radius: 50%;
		height: 6px;
		width: 6px;
		margin-top: -3px;
		left: 0;
	}
</style>