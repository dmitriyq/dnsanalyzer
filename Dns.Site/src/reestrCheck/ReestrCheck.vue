<template>
	<v-container fluid>
		<v-row>
			<v-col cols="12">
				<p class="text-center font-weight-bold">Поиск в Реестре</p>
			</v-col>
		</v-row>
		<v-row>
			<v-col xs="12" sm="6">
				<v-text-field v-model="search"
							  label="Домен / IP"
							  clearable></v-text-field>
			</v-col>
			<v-col xs="6" sm="3">
				<v-btn left color="info" :disabled="canSearch" @click="searchReestr">Поиск</v-btn>
			</v-col>
			<v-col xs="6" sm="3">
				<v-checkbox label="Нечеткий поиск"
							v-model="containsSearch"></v-checkbox>
			</v-col>
		</v-row>
		<v-row>
			<v-col cols="12">
				<v-data-table class="elevation-1"
							  fixed-header
							  :loading="loading"
							  loading-text="Получение данных"
							  locale="ru-RU"
							  no-data-text="Нет данных"
							  no-results-text="Не найдено данных, подходящих под условие запроса"
							  :headers="tableHeaders"
							  :items="reestr"
							  item-key="idRkn"
							  :items-per-page="25"
							  :footer-props="tableFooterOpts">
					<v-progress-linear slot="progress" color="blue" indeterminate/>
				</v-data-table>
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import IDataTableHeaders from '@/models/data-table';
import IReestrRecord from '@/reestrCheck/reestr-record';
import Axios from 'axios';

@Component
export default class ReestrCheck extends Vue {
	public search: string = '';
	public containsSearch: boolean = false;
	public reestr: IReestrRecord[] = [];
	public loading: boolean = false;
	public tableHeaders: IDataTableHeaders[] = [
		{ text: 'Дата включения', value: 'created', align: 'center' },
		{ text: 'Домен', value: 'domain', align: 'center' },
		{ text: 'URL ', value: 'urlRkn', align: 'center' },
		{ text: 'IP', value: 'ip', align: 'center' },
		{ text: 'Подсеть', value: 'subnet', align: 'center' },
		{ text: 'Минюст URL', value: 'urlMinjust', width: '60px', align: 'center' },
	];

	public tableFooterOpts = {
		'showFirstLastPage': true,
		'show-current-page': true,
		'items-per-page-all-text': 'Все',
		'items-per-page-options': [10, 25, 100, -1],
		'items-per-page-text': 'Кол-во на странице',
		'page-text': 'записей',
	};

	public get canSearch(): boolean {
		return this.search.trim().length <= 0;
	}

	public searchReestr(): void {
		this.loading = true;
		const data = { search: this.search.trim(), isContainsSearch: this.containsSearch };
		Axios.post('/api/reestr/query', data)
			.then((resp) => {
					this.loading = false;
					this.reestr = resp.data as IReestrRecord[];
				});
	}

	public created() {
		window.addEventListener('keyup', this.enterListener);
	}
	public destroyed() {
		window.removeEventListener('keyup', this.enterListener);
	}
	private enterListener(event: KeyboardEvent) {
		if (event.keyCode === 13 && !this.canSearch) {
			this.searchReestr();
		}
	}
}
</script>

<style scoped>
</style>