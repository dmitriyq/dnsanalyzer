<template>
	<v-layout row wrap justify-start>
		<v-flex xs12 class="mb-5">
			<v-sheet height="450">
				<p class="text-xs-center subheading">Статистика за {{monthLabels.current}}г.</p>
				<v-calendar type="month"
							ref="calendar"
							v-model="calendarInitalValue"
							class="disable-calendar-link"
							locale="ru"
							:weekdays="[1, 2, 3, 4, 5, 6, 0]">
					<template v-slot:day="{ date }">
						<v-container fluid class="px-0 py-0">
							<v-layout align-center justify-space-between row fill-height>
								<v-flex xs12 class="text-xs-center">
									<v-data-iterator hide-actions :items="getEvent(date)">
										<template v-slot:item="props">
											<v-card>
												<v-card-title class="px-0 py-0 pt-1"><span class="text-xs-center">Атака</span></v-card-title>
												<v-divider></v-divider>
												<v-list dense class="list-item-height-20">
													<v-list-tile>
														<v-list-tile-content>Новые:</v-list-tile-content>
														<v-list-tile-content class="align-end">{{ props.item.groupNew }}</v-list-tile-content>
													</v-list-tile>
													<v-list-tile>
														<v-list-tile-content>Угроз:</v-list-tile-content>
														<v-list-tile-content class="align-end">{{ props.item.groupThreat }}</v-list-tile-content>
													</v-list-tile>
													<v-list-tile>
														<v-list-tile-content>Атак:</v-list-tile-content>
														<v-list-tile-content class="align-end">{{ props.item.groupAttack }}</v-list-tile-content>
													</v-list-tile>
													<v-list-tile>
														<v-list-tile-content>Прекращено:</v-list-tile-content>
														<v-list-tile-content class="align-end">{{ props.item.groupComplete }}</v-list-tile-content>
													</v-list-tile>
												</v-list>
											</v-card>
										</template>
									</v-data-iterator>
								</v-flex>
								<v-flex xs12>
									<v-data-iterator hide-actions :items="getEvent(date)">
										<template v-slot:item="props">
											<v-card>
												<v-card-title class="px-0 py-0 pt-1"><span class="text-xs-center">IP</span></v-card-title>
												<v-divider></v-divider>
												<v-list dense class="list-item-height-20">
													<v-list-tile>
														<v-list-tile-content>Новые:</v-list-tile-content>
														<v-list-tile-content class="align-end">{{ props.item.attackNew }}</v-list-tile-content>
													</v-list-tile>
													<v-list-tile>
														<v-list-tile-content>Пересечений:</v-list-tile-content>
														<v-list-tile-content class="align-end">{{ props.item.attackIntersection }}</v-list-tile-content>
													</v-list-tile>
													<v-list-tile>
														<v-list-tile-content>Прекращено:</v-list-tile-content>
														<v-list-tile-content class="align-end">{{ props.item.attackComplete }}</v-list-tile-content>
													</v-list-tile>
												</v-list>
											</v-card>
										</template>
									</v-data-iterator>
								</v-flex>
							</v-layout>
						</v-container>
					</template>
				</v-calendar>
			</v-sheet>
		</v-flex>
		<v-flex xs6 class="text-sm-left text-xs-center">
			<v-btn @click="prevMonthClick">
				<v-icon dark left>keyboard_arrow_left</v-icon>
				{{monthLabels.prev}}
			</v-btn>
		</v-flex>
		<v-flex xs6 class="text-sm-right text-xs-center">
			<v-btn @click="nextMonthClick" :disabled="nextMonthBtnDisabled">
				{{monthLabels.next}}
				<v-icon right dark>keyboard_arrow_right</v-icon>
			</v-btn>
		</v-flex>
	</v-layout>
</template>

<script lang="ts">
	import Vue from 'vue';
	import { Component, Watch } from 'vue-property-decorator';
	import { ICalendarEvent, ICalendarDate } from '@/models/calendar-model';
	import { format, startOfMonth, isAfter } from 'date-fns';
	import Axios from 'axios';
	import ruLocale from 'date-fns/locale/ru';

	@Component({})
	export default class StatCalendar extends Vue {
		public monthLabels = { current: '', prev: '', next: '' };
		public calendarInitalValue = '';
		public nextMonthBtnDisabled: boolean = false;
		public events: ICalendarEvent[] = [];

		public mounted() {
			const currentDate = new Date();
			this.calendarInitalValue = format(currentDate, 'YYYY-MM-DD');
			this.changeMonth(currentDate);
		}

		@Watch('calendarInitalValue')
		private onCalendarValueChange(val: string, oldVal: string) {
			if (oldVal) {
				this.changeMonth(new Date(val));
			}
			this.nextMonthBtnDisabled = this.isFutureMonth(val);
		}
		private changeMonth(date: Date): void {
			const year = date.getFullYear();
			const prevMonth = date.getMonth() === 0 ? 11 : (date.getMonth() - 1);
			const nextMonth = date.getMonth() === 11 ? 0 : (date.getMonth() + 1);
			this.monthLabels.current = this.formatMonthYearDate(date);
			this.monthLabels.prev = this.formatMonthYearDate(new Date(prevMonth === 11 ? (year - 1) : year, prevMonth));
			this.monthLabels.next = this.formatMonthYearDate(new Date(nextMonth === 0 ? (year + 1) : year, nextMonth));

			Axios.get(`/api/attack/calendar?year=${date.getFullYear()}&month=${date.getMonth() + 1}`)
				.then((resp) => {
					const events = (resp.data as ICalendarEvent[]);
					events.forEach((val) => val.date = format(new Date(val.date), 'YYYY-MM-DD'));
					this.events = events;
				});
		}
		private formatMonthYearDate(date: Date): string {
			return format(date, 'MMMM YYYY', { locale: ruLocale });
		}
		private prevMonthClick() {
			(this.$refs.calendar as any).prev();
		}
		private nextMonthClick() {
			(this.$refs.calendar as any).next();
		}
		private isFutureMonth(date: string): boolean {
			const current = startOfMonth(new Date());
			const val = new Date(date);
			return isAfter(val, current);
		}
		private getEvent(date: string): ICalendarEvent[] {
			const arr = new Array<ICalendarEvent>();
			const item = this.events.find((val) => val.date === date);
			if (item) {
				arr.push(item);
			}
			return arr;
		}
    }
</script>
<style>
	.disable-calendar-link .v-calendar-weekly__day-label{
		cursor:default;
	}
	.disable-calendar-link .v-calendar-weekly__day-label:hover{
		text-decoration:none;
	}
	.list-item-height-20 div[role=listitem] .v-list__tile{
		height:20px;
		padding:0;
	}
</style>