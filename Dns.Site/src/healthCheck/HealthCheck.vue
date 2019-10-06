<template>
	<v-container fluid>
		<v-layout row wrap justify-start>
			<v-flex xs12 class="mb-3">
				<p class="text-xs-center subheading">Статус компонентов системы</p>
			</v-flex>
			<v-flex xs12>
				<v-data-table :headers="tableHeaders"
							  :items="statuses"
							  hide-action
							  :pagination.sync="pagination">
					<template slot="items" slot-scope="props">
						<td class="text-xs-center">{{ props.item.service }}</td>
						<td class="text-xs-center">{{ props.item.creationDate }}</td>
						<td class="text-xs-left">{{ props.item.currentAction }}</td>
					</template>
					<template slot="no-data">
						<v-alert :value="true" color="primary" outline>
							Ничего не найдено
						</v-alert>
					</template>
				</v-data-table>
			</v-flex>
		</v-layout>
	</v-container>
</template>

<script lang="ts">
	import Vue from 'vue';
	import { Component } from 'vue-property-decorator';
	import IDataTableHeaders from '@/models/data-table';
	import * as signalR from '@aspnet/signalr';
	import { HealthStatus } from '@/models/health-status';

	@Component({})
	export default class HealthCheck extends Vue {
		public healthHub: signalR.HubConnection;
		public statuses: HealthStatus[] = [];
		public panelDomains: boolean[] = [];
		public pagination = { rowsPerPage: -1 };
		public tableHeaders: IDataTableHeaders[] = [
			{ text: 'Компонент', value: 'service', align: 'left', width: '200px' },
			{ text: 'Время', value: 'creationDate', align: 'left', width: '150px' },
			{ text: 'Статус', value: 'currentAction', align: 'left' },
		];

		public created() {
			this.initHub();
		}

		private initHub() {
			this.healthHub = new signalR.HubConnectionBuilder()
				.withUrl('/healthHub').build();
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