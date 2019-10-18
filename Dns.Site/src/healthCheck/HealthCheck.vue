<template>
	<v-container fluid>
		<v-row>
			<v-col cols="12">
				<p class="text-center subheading">Статус компонентов системы</p>
			</v-col>
		</v-row>
		<v-row>
			<v-col cols="12">
				<v-data-table :headers="tableHeaders"
							  :items="statuses"
							  no-data-text="Нет данных"
							  no-results-text="Не найдено данных, подходящих под условие запроса"
							  hide-default-footer
							  :items-per-page="-1">
				</v-data-table>
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
	import Vue from 'vue';
	import { Component } from 'vue-property-decorator';
	import IDataTableHeaders from '@/models/data-table';
	import * as signalR from '@microsoft/signalr';
	import { HealthStatus } from '@/healthCheck/health-status';

	@Component({})
	export default class HealthCheck extends Vue {
		public healthHub: signalR.HubConnection;
		public statuses: HealthStatus[] = [];
		public panelDomains: boolean[] = [];
		public pagination = { rowsPerPage: -1 };
		public tableHeaders: IDataTableHeaders[] = [
			{ text: 'Компонент', value: 'service', align: 'center', width: '250px' },
			{ text: 'Время', value: 'creationDate', align: 'center', width: '150px' },
			{ text: 'Статус', value: 'currentAction', align: 'center' },
		];

		public created() {
			this.initHub();
		}

		private initHub() {
			this.healthHub = new signalR.HubConnectionBuilder()
				.withAutomaticReconnect({
					nextRetryDelayInMilliseconds: ((c) => 1000),
				}).withUrl('/healthHub').build();
			this.healthHub.start()
				// tslint:disable-next-line:no-console
				.catch((err: any) => console.error(err));

			this.healthHub.on('Update', (event: HealthStatus) => {
				const oldInfo = this.statuses.findIndex((val) => val.service === event.service);
				if (oldInfo !== -1) {
					this.statuses.splice(oldInfo, 1, event);
				} else {
					this.statuses.unshift(event);
				}
			});
			this.healthHub.onclose(() => {
				this.initHub();
			});
		}
	}
</script>