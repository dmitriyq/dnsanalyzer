<template>
	<v-container fluid grid-list-md>
		<v-layout align-start row wrap fluid>
			<v-flex xs12>
				<p class="text-xs-center font-weight-bold">Поиск в Реестре</p>
			</v-flex>
			<v-flex xs12 sm6 align-self-baseline>
				<v-text-field v-model="search"
							  label="Домен / IP"
							  clearable></v-text-field>
			</v-flex>
			<v-flex xs6 sm3>
				<v-btn left color="info" :disabled="canSearch" @click="searchReestr">Поиск</v-btn>
			</v-flex>
			<v-flex xs6 sm3>
				<v-checkbox label="Нечеткий поиск"
							v-model="containsSearch"></v-checkbox>
			</v-flex>
			<v-flex xs12>
				<v-data-table :headers="tableHeaders"
							  :items="reestr"
							  :loading="loading"
							  :total-items="pagination.totalItems"
							  :pagination.sync="pagination">
					<template slot="items" slot-scope="props">
						<tr :class="highLightRow(props.index) + ' rowHover'">
							<td class="text-xs-center">{{ props.item.created }}</td>
							<td class="text-xs-center">{{ props.item.domain }}</td>
							<td class="text-xs-left">{{ props.item.urlRkn }}</td>
							<td class="text-xs-center">{{ props.item.ip }}</td>
							<td class="text-xs-center">{{ props.item.subnet }}</td>
							<td class="text-xs-left">{{ props.item.urlMinjust }}</td>
						</tr>
					</template>
					<template slot="no-data">
						<v-alert :value="true" color="primary" outline>
							Ничего не найдено
						</v-alert>
					</template>
				</v-data-table>
				<div class="text-xs-center" v-if="totalPages > 1">
					<v-container fluid class="p-0">
						<v-layout justify-center>
							<v-flex xs12>
								<v-card>
									<v-card-text>
										<v-layout justify-center wrap row>
											<v-flex xs12>
												<v-pagination v-model="pagination.page"
															  color="green darken-1"
															  :length="totalPages"></v-pagination>
											</v-flex>
										</v-layout>
									</v-card-text>
								</v-card>
							</v-flex>
						</v-layout>
					</v-container>
				</div>
			</v-flex>
		</v-layout>
	</v-container>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import IDataTableHeaders from '@/models/data-table';
import IReestrRecord from '@/models/reestr-record';
import { IPagination } from '@/models/select-model';
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
	public pagination: IPagination = {
		totalItems: 0,
		rowsPerPage: 10,
		page: 1,
	};

	public get canSearch(): boolean {
		return this.search.trim().length <= 0;
	}
	public get totalPages(): number {
		return Math.ceil(this.pagination.totalItems / this.pagination.rowsPerPage);
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

	public highLightRow(id: number): string {
		// tslint:disable-next-line:no-bitwise
		if (id & 1) {
			return 'blue lighten-5';
		} else {
			return '';
		}
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