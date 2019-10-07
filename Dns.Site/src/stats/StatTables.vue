<template>
	<v-layout row wrap justify-start>
		<v-flex sm6 xs12>
			<p style="font-size:16px;text-align:center">Статистика по атакам</p>
			<v-data-table class="elevation-1"
						  hide-default-footer
						  :headers="theaders"
						  :items="groups">
				<template slot="items" slot-scope="props">
					<td class="text-center">{{ props.item.name }}</td>
					<td class="text-center">{{ props.item.count }}</td>
				</template>
			</v-data-table>
		</v-flex>
		<v-flex sm6 xs12>
			<p style="font-size:16px;text-align:center">Статистика по IP адресам</p>
			<v-data-table class="elevation-1"
						  hide-default-footer
						  :headers="theaders"
						  :items="attacks">
				<template slot="items" slot-scope="props">
					<td class="text-center">{{ props.item.name }}</td>
					<td class="text-center">{{ props.item.count }}</td>
				</template>
			</v-data-table>
		</v-flex>
	</v-layout>
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