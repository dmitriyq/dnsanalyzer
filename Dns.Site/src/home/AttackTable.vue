<template>
	<v-data-table class="elevation-1"
				  fixed-header
				  :loading="tableUpdating"
				  loading-text="Получение данных"
				  locale="ru-RU"
				  must-sort
				  no-data-text="Нет данных"
				  no-results-text="Не найдено данных, подходящих под условие запроса"
				  show-group-by
				  :show-select="isAdmin && !this.$vuetify.breakpoint.xs"
				  :headers="tableHeaders"
				  :items="dnsAttacks"
				  item-key="id"
				  :items-per-page="25"
				  v-model="checkBoxedAttacks"
				  :search="filterExpr"
				  :footer-props="tableFooterOpts">
		<v-progress-linear slot="progress" color="blue" indeterminate></v-progress-linear>
		<template v-slot:item.status="{ item }">
			<v-icon x-large
					:class="getColorStatus(item.status)">
				{{ item.status | statusIcon }}
			</v-icon>
		</template>
		<template v-slot:item.summary="{ item }">
			<span v-for="summ in item.summary">
				{{summ.count}}
				<v-icon :class="getColorIpStatus(summ.status) + 'text-center'">
					{{ summ.status | statusIpIcon }}
				</v-icon>
			</span>
		</template>
		<template v-slot:item.info="{ item }">
			<v-btn icon :to="{ name: 'DnsAttackInfo', params: {id: item.id}}">
				<v-icon x-large>more_horiz</v-icon>
			</v-btn>
		</template>
	</v-data-table>
</template>

<script lang="ts">
	import Vue from 'vue';
	import { Component, Prop, PropSync, Watch } from 'vue-property-decorator';
	import DnsAttack from '@/home/dns-attack';
	import Utils from '@/utils/Utils';
	import IDataTableHeaders from '@/models/data-table';
	import IAttackTableFilters from '@/home/attack-table-filters';

	@Component({
		filters: {
			statusIcon(status: number) {
				return Utils.getStatusIcon(status);
			},
			statusIpIcon(status: number) {
				return Utils.getStatusIpIcon(status);
			},
		},
		mounted() {
			window.document.querySelectorAll('.v-data-table-header__icon').forEach((el, i) => {
				el.nextElementSibling!.innerHTML = 'сгруппировать';
			});

		},
	})
	export default class AttackTable extends Vue {
		@PropSync('attacks') public dnsAttacks: DnsAttack[];
		@PropSync('selectedAttacks') public checkBoxedAttacks: DnsAttack[];
		@Prop() public tableUpdating: boolean;
		@Prop() public tableFilters: IAttackTableFilters;
		@Prop() public filterExpr: string;

		public tableFooterOpts = {
			'showFirstLastPage': true,
			'show-current-page': true,
			'items-per-page-all-text': 'Все',
			'items-per-page-options': [10, 25, 100, -1],
			'items-per-page-text': 'Кол-во на странице',
			'page-text': 'записей',
		};

		public selectedId: number = 0;
		public tableHeaders: IDataTableHeaders[] = [
			{
				text: 'Статус',
				value: 'status',
				align: 'center',
				width: '100px',
				class: 'pl-0 pr-2 no-grouppable',
				filter: (value: number, search: string, item: DnsAttack) => {
					if (!this.tableFilters.showCompleted && value === 5) {
						return false;
					} else if (!this.tableFilters.showDynamic && value === 4) {
						return false;
					} else {
						return true;
					}
				},
			},
			{ text: 'Атакуемый домен', value: 'whiteDomain' },
			{ text: 'Атакующий домен', value: 'blackDomain' },
			{ text: 'IP', value: 'summary', width: '150px', class: 'no-grouppable', align: 'center', filterable: false },
			{ text: 'Подробнее', value: 'info', width: '120px', align: 'center', class: 'no-grouppable', filterable: false },
		];

		public getColorStatus(status: number): string { return Utils.getStatusColor(status); }
		public getColorIpStatus(status: number): string { return Utils.getStatusIpColor(status); }

    get isAdmin(): boolean {
			return this.$store.getters.isAdmin;
		}

		@Watch('tableFilters', { deep: true })
		private onTableFilterChanged(val: IAttackTableFilters, oldVal: IAttackTableFilters) {
			if (!val.showCompleted) {
				this.checkBoxedAttacks = this.checkBoxedAttacks.filter((v) => v.status !== 5);
			} else if (!val.showDynamic) {
				this.checkBoxedAttacks = this.checkBoxedAttacks.filter((v) => v.status !== 4);
			}
		}


		@Watch('checkBoxedAttacks')
		private onCheckBoxedAttacks() {
			this.$emit('selectedupdate', this.checkBoxedAttacks);
		}
	}
</script>
<style>
	th[role="columnheader"][scope="col"].no-grouppable span~i~span{
		display: none;
	}
</style>
