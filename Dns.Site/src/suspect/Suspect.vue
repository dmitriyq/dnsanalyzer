<template>
	<v-container fluid>
		<v-layout row wrap justify-start>
			<v-flex xs12 class="mb-3">
				<p class="text-xs-center subheading">Домены из черного списка у которых больше 10 IP-адресов</p>
			</v-flex>
			<v-flex xs12>
				<v-expansion-panel v-model="panelDomains" expand>
					<v-expansion-panel-content v-for="domain in domains" :key="domain.domain">
						<template v-slot:header>
							<v-flex xs6>{{domain.domain}}</v-flex>
							<v-flex xs6>{{domain.ips.length}} IP</v-flex>
						</template>
						<v-data-table :items="domain.ips" :headers="tableHeaders" hide-action :pagination.sync="pagination">
							<template v-slot:items="props">
								<td>{{ props.item.ip }}</td>
								<td>{{ props.item.subnet }}</td>
								<td>{{ props.item.country }}</td>
								<td>{{ props.item.company }}</td>
							</template>
						</v-data-table>
					</v-expansion-panel-content>
				</v-expansion-panel>
			</v-flex>
		</v-layout>
	</v-container>
</template>

<script lang="ts">
	import Vue from 'vue';
	import { Component } from 'vue-property-decorator';
	import Axios from 'axios';
	import { ISuspectDomain } from '@/models/suspect-domain';
	import IDataTableHeaders from '@/models/data-table';

	@Component({})
	export default class Suspect extends Vue {
		public panelDomains: boolean[] = [];
		public pagination = { rowsPerPage: -1 };
		public tableHeaders: IDataTableHeaders[] = [
			{ text: 'IP', value: 'ip', align: 'left', width: '150px' },
			{ text: 'Подсеть', value: 'subnet', align: 'left', width: '150px' },
			{ text: 'Страна', value: 'country', align: 'left', width: '150px'},
			{ text: 'Организация', value: 'company', align: 'left' },
		];

		public domains: ISuspectDomain[] = [];

		public created() {
			Axios.get(`/api/attack/suspects`)
				.then((resp) => {
					const data = (resp.data.data as ISuspectDomain[]);
					const date = resp.data.date as Date;

					this.panelDomains = data.map((val) => false);
					this.domains = data;
				});
		}
    }
</script>
