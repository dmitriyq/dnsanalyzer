<template>
	<v-container container--fluid>
		<v-layout row wrap justify-start>
			<v-flex xs12 class="mb-3">
				<p class="text-center title">Выгрузки в Excel</p>
			</v-flex>
			<v-flex lg3 md4 sm5 xs12 class="mr-2 mt-2">
				<v-card class="text-xs-center">
					<v-card-title primary-title class="pb-0">
						<span class="subheading font-weight-regular">Подозрительные домены</span>
					</v-card-title>
					<v-card-actions>
						<v-layout align-center justify-end>
							<v-btn text outlined color="info" @click="suspectDownload" :loading="buttonsState.suspect">Скачать</v-btn>
						</v-layout>
					</v-card-actions>
				</v-card>
			</v-flex>
			<v-flex lg3 md4 sm5 xs12 class="mr-2 mt-2">
				<v-card class="text-center">
					<v-card-title primary-title class="pb-0">
						<span class="subheading font-weight-regular">Белый список</span>
					</v-card-title>
					<v-card-actions>
						<v-layout align-center justify-end>
							<v-btn text outlined color="info" @click="whitelistDownload" :loading="buttonsState.white">Скачать</v-btn>
						</v-layout>
					</v-card-actions>
				</v-card>
			</v-flex>
			<v-flex xs12 md9 class="mr-2 mt-2">
				<v-card>
					<v-card-title primary-title class="pb-0">
						<v-layout row wrap justify-start>
							<v-flex xs12 sm6 md3>
								<p class="subheading font-weight-regular text-center w-100">Предустановленные значения</p>
							</v-flex>
							<v-flex xs12 md6>
								<p class="title font-weight-regular w-100 text-center">Статистика DNS</p>
							</v-flex>
						</v-layout>
					</v-card-title>
					<v-card-text>
						<v-layout row wrap justify-start>
							<v-flex xs12 sm6 md3>
								<v-btn text outlined color="info" @click="setWeek">Неделя</v-btn>
								<v-btn text outlined color="info" @click="setMonth">Месяц</v-btn>
								<v-btn text outlined color="info" @click="setYear">Год</v-btn>
								<v-btn text outlined color="info" @click="setFull">Полная</v-btn>
							</v-flex>
							<v-flex xs12 md4>
								<v-menu v-model="datePickers.fromOpen" :close-on-content-click="false" :nudge-right="40"
										transition="scale-transition" offset-y min-width="290px" max-width="290px">
									<template v-slot:activator="{ on }">
										<v-text-field v-model="diplayFromDate" label="Начало периода"
													  prepend-icon="event" readonly v-on="on"></v-text-field>
									</template>
									<v-date-picker v-model="datePickers.from" @input="datePickers.fromOpen = false" locale="ru-ru"
												   first-day-of-week="1"></v-date-picker>
								</v-menu>
							</v-flex>
							<v-flex xs12 md4>
								<v-menu v-model="datePickers.toOpen" :close-on-content-click="false" :nudge-right="40"
										transition="scale-transition" offset-y min-width="290px" max-width="290px">
									<template v-slot:activator="{ on }">
										<v-text-field v-model="displayToDate" label="Конец периода"
													  prepend-icon="event" readonly v-on="on"></v-text-field>
									</template>
									<v-date-picker v-model="datePickers.to" @input="datePickers.toOpen = false" locale="ru-ru"
												   first-day-of-week="1"></v-date-picker>
								</v-menu>
							</v-flex>
						</v-layout>
					</v-card-text>
					<v-card-actions>
						<v-layout align-center justify-end>
							<v-btn color="info" @click="dnsDownload" :loading="buttonsState.dns">Скачать</v-btn>
						</v-layout>
					</v-card-actions>
				</v-card>
			</v-flex>
		</v-layout>
	</v-container>
</template>

<script lang="ts">
	import Vue from 'vue';
	import { Component } from 'vue-property-decorator';
	import {
		format as fnsFormat, startOfWeek, lastDayOfWeek,
		startOfMonth, lastDayOfMonth, startOfYear, lastDayOfYear,
	} from 'date-fns';
	import Axios from 'axios';

	@Component({})
	export default class Export extends Vue {
		public buttonsState = {
			rkn: false,
			minjust: false,
			dns: false,
			suspect: false,
			white: false,
		};
		public datePickers = {
			from: this.formatDateISO(new Date()),
			fromOpen: false,
			to: this.formatDateISO(new Date()),
			toOpen: false,
		};
		get diplayFromDate(): string {
			return this.formatDateDisplay(this.parseISODate(this.datePickers.from));
		}
		get displayToDate(): string {
			return this.formatDateDisplay(this.parseISODate(this.datePickers.to));
		}

		private suspectDownload() {
			this.buttonsState.suspect = true;
			const url = '/api/export/exportSuspect';
			Axios.get(url)
				.then((resp) => {
					window.location.href = `/api/export/getFile?name=${resp.data.msg}`;
					this.buttonsState.suspect = false;
				});
		}
		private dnsDownload() {
			this.buttonsState.dns = true;
			const url = '/api/export/exportDns';
			const data = { from: this.parseISODate(this.datePickers.from), to: this.parseISODate(this.datePickers.to) };
			Axios.post(url, data)
				.then((resp) => {
					window.location.href = `/api/export/getFile?name=${resp.data.msg}`;
					this.buttonsState.dns = false;
				});
		}
		private whitelistDownload() {
			this.buttonsState.white = true;
			const url = '/api/export/exportWhitelist';
			Axios.get(url)
				.then((resp) => {
					window.location.href = `/api/export/getFile?name=${resp.data.msg}`;
					this.buttonsState.white = false;
				});
		}
		private setWeek() {
			const current = new Date();
			this.datePickers.from = this.formatDateISO(startOfWeek(current, { weekStartsOn: 1 }));
			this.datePickers.to = this.formatDateISO(lastDayOfWeek(current, { weekStartsOn: 1 }));
		}
		private setMonth() {
			const current = new Date();
			this.datePickers.from = this.formatDateISO(startOfMonth(current));
			this.datePickers.to = this.formatDateISO(lastDayOfMonth(current));
		}
		private setYear() {
			const current = new Date();
			this.datePickers.from = this.formatDateISO(startOfYear(current));
			this.datePickers.to = this.formatDateISO(lastDayOfYear(current));
		}
		private setFull() {
			const current = new Date();
			Axios.get(`/api/export/getFirstDate`)
				.then((resp) => {
					const first = resp.data as Date;
					this.datePickers.from = this.formatDateISO(first);
					this.datePickers.to = this.formatDateISO(current);
				});
		}
		private formatDateISO(date: Date): string {
			return fnsFormat(date, 'yyyy-MM-dd');
		}
		private formatDateDisplay(date: Date): string {
			return fnsFormat(date, 'dd.MM.yyyy');
		}
		private parseISODate(date: string): Date {
			const [year, month, day] = date.split('-');
			return new Date(+year, (+month) - 1, +day);
		}
    }
</script>
<style scoped>
	.w-100 {
		width: 100%;
	}
</style>