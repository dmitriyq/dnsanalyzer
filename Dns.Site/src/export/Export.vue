<template>
	<v-container fluid>
		<v-row>
			<v-col cols="12">
				<p class="text-center subheading">Выгрузки в Excel</p>
			</v-col>
		</v-row>
		<v-row>
			<v-col lg="3" md="6" sm="12">
				<v-card>
					<v-card-text class="text-center font-weight-bold">Подозрительные домены</v-card-text>
					<v-card-actions>
						<v-spacer />
						<v-btn text outlined color="info" @click="suspectDownload" :loading="buttonsState.suspect">Скачать</v-btn>
					</v-card-actions>
				</v-card>
			</v-col>
			<v-col lg="3" md="6" sm="12">
				<v-card>
					<v-card-text class="text-center font-weight-bold">Белый список</v-card-text>
					<v-card-actions>
						<v-spacer />
						<v-btn text outlined color="info" @click="whitelistDownload" :loading="buttonsState.white">Скачать</v-btn>
					</v-card-actions>
				</v-card>
			</v-col>
		</v-row>
		<v-row>
			<v-col lg="6" md="9" sm="12">
				<v-card>
					<v-card-text class="text-center font-weight-bold">
						<v-container>
							<v-row>
								<v-col sm="12" md="6" lg="3">
									<p>Предустановленные значения</p>
									<v-btn text outlined color="info" @click="setWeek">Неделя</v-btn>
									<v-btn text outlined color="info" @click="setMonth">Месяц</v-btn>
									<v-btn text outlined color="info" @click="setYear">Год</v-btn>
									<v-btn text outlined color="info" @click="setFull">Полная</v-btn>
								</v-col>
								<v-col sm="12" md="6" lg="3">
									<p>Статистика DNS</p>
									<v-menu v-model="datePickers.fromOpen" :close-on-content-click="false" :nudge-right="40"
											transition="scale-transition" offset-y min-width="290px" max-width="290px">
										<template v-slot:activator="{ on }">
											<v-text-field v-model="diplayFromDate" label="Начало периода"
														  prepend-icon="event" readonly v-on="on"></v-text-field>
										</template>
										<v-date-picker v-model="datePickers.from" @input="datePickers.fromOpen = false" locale="ru-ru"
													   first-day-of-week="1"></v-date-picker>
									</v-menu>
									<v-menu v-model="datePickers.toOpen" :close-on-content-click="false" :nudge-right="40"
											transition="scale-transition" offset-y min-width="290px" max-width="290px">
										<template v-slot:activator="{ on }">
											<v-text-field v-model="displayToDate" label="Конец периода"
														  prepend-icon="event" readonly v-on="on"></v-text-field>
										</template>
										<v-date-picker v-model="datePickers.to" @input="datePickers.toOpen = false" locale="ru-ru"
													   first-day-of-week="1"></v-date-picker>
									</v-menu>
								</v-col>
							</v-row>
						</v-container>
					</v-card-text>
					<v-card-actions>
						<v-spacer />
						<v-btn text outlined color="info" @click="dnsDownload" :loading="buttonsState.dns">Скачать</v-btn>
					</v-card-actions>
				</v-card>
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
	import Vue from 'vue';
	import { Component } from 'vue-property-decorator';
	import format from 'date-fns/format';
	import parseISO from 'date-fns/parseISO';
	import startOfWeek from 'date-fns/startOfWeek';
	import startOfYear from 'date-fns/startOfYear';
	import startOfMonth from 'date-fns/startOfMonth';
	import lastDayOfWeek from 'date-fns/lastDayOfWeek';
	import lastDayOfMonth from 'date-fns/lastDayOfMonth';
	import lastDayOfYear from 'date-fns/lastDayOfYear';

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
					const first = parseISO(resp.data);
					this.datePickers.from = this.formatDateISO(first);
					this.datePickers.to = this.formatDateISO(current);
				});
		}
		private formatDateISO(date: Date): string {
			return format(date, 'yyyy-MM-dd');
		}
		private formatDateDisplay(date: Date): string {
			return format(date, 'dd.MM.yyyy');
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