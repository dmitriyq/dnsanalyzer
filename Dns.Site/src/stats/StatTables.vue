<template>
	<v-container fluid>
		<v-row>
			<v-col xs="12" sm="6">
				<p style="font-size:16px;text-align:center">Статистика по атакам</p>
				<v-data-table class="elevation-1"
							  hide-default-footer
							  :headers="theaders"
							  :items="groups">
				</v-data-table>
			</v-col>
			<v-col xs="12" sm="6">
				<p style="font-size:16px;text-align:center">Статистика по IP адресам</p>
				<v-data-table class="elevation-1"
							  hide-default-footer
							  :headers="theaders"
							  :items="attacks">
				</v-data-table>
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
	import Vue from 'vue';
	import { Component } from 'vue-property-decorator';
	import Axios from 'axios';
	import IDataTableHeaders from '@/models/data-table';

	@Component({})
	export default class StatTables extends Vue {
		public theaders: IDataTableHeaders[] = [
			{ text: 'Статус', value: 'name', align: 'center', sortable: false },
			{ text: 'Число записей', value: 'count', align: 'center', sortable: false },
		];
		public groups: Array<({ name: string, count: number})> = [];
		public attacks: Array<({ name: string, count: number})> = [];

		public created() {
			Axios.get('api/attack/stats')
				.then((r) => {
					const resp = r.data as any;
					this.groups = resp.groups;
					this.attacks = resp.attacks;
				});
		}
    }
</script>