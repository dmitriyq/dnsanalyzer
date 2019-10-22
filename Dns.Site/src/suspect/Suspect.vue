<template>
	<v-container fluid>
		<v-row align="start">
			<v-col cols="12">
				<p class="text-center subtitle-1">Домены из черного списка у которых больше 10 IP-адресов</p>
			</v-col>
		</v-row>
		<v-row justify="center">
			<v-col cols="12" md="6">
				<v-expansion-panels multiple>
					<v-expansion-panel v-for="domain in domains" :key="domain.domain">
						<v-expansion-panel-header>
							<v-container fluid class="pa-0">
								<v-row>
									<v-col cols="10" class="pa-0">{{domain.domain}}</v-col>
									<v-col cols="2" class="pa-0">{{domain.ips.length}} шт.</v-col>
								</v-row>
							</v-container>
						</v-expansion-panel-header>
						<v-expansion-panel-content>
							<v-data-table fixed-header
										  hide-default-footer
										  :items="domain.ips"
										  :items-per-page="-1"
										  item-key="id"
										  hide-action
										  :headers="tableHeaders"
										  >

							</v-data-table>
						</v-expansion-panel-content>
					</v-expansion-panel>
				</v-expansion-panels>
			</v-col>
		</v-row>
		<v-layout row wrap justify-start>
			<v-flex xs12 class="mb-3">
				
			</v-flex>
			<v-flex xs12>
				<v-expansion-panels>
					<v-expansion-panel v-model="panelDomains" expand>
						<v-expansion-panel-content v-for="domain in domains" :key="domain.domain">
							<template v-slot:header>
								<v-flex xs6>{{domain.domain}}</v-flex>
								<v-flex xs6>{{domain.ips.length}} IP</v-flex>
							</template>
							<v-data-table :items="domain.ips" :headers="tableHeaders" hide-default-footer>
							</v-data-table>
						</v-expansion-panel-content>
					</v-expansion-panel>
				</v-expansion-panels>
			</v-flex>
		</v-layout>
	</v-container>
</template>

<script lang="ts">
	import Vue from 'vue';
	import { Component } from 'vue-property-decorator';
	import Axios from 'axios';
	import { ISuspectDomain } from '@/suspect/suspect-domain';
	import IDataTableHeaders from '@/models/data-table';

	@Component({})
	export default class Suspect extends Vue {
		public panelDomains: boolean[] = [];
		public tableHeaders: IDataTableHeaders[] = [
			{ text: 'IP', value: 'ip', align: 'left', width: '150px' },
			{ text: 'Подсеть', value: 'subnet', align: 'left', width: '150px'},
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
