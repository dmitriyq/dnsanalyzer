<template>
	<v-data-table class="elevation-1"
				  :hide-actions="true"
				  :headers="tableHeader"
				  :items="dnsAttacks"
				  must-sort
				  disable-initial-sort
				  :pagination.sync="tableSort"
				  v-model="checkBoxedAttacks"
				  :search="filterExpr"
				  :loading="tableUpdating">
		<v-progress-linear slot="progress" color="blue" indeterminate></v-progress-linear>
		<template slot="items" slot-scope="props">
			<tr :class="highLightRow(props.item.id) + ' rowHover'"
				v-if="showRow(props.item.status)">
				<td v-if="isDnsAdmin"
					class="text-xs-left pr-0 hidden-xs-only"
					@click="props.selected = !props.selected">
					<v-checkbox :input-value="!!props.selected"
								primary
								hide-details></v-checkbox>
				</td>
				<td class="text-xs-center px-0">
					<v-icon x-large
							:class="getColorStatus(props.item.status)">
						{{ props.item.status | statusIcon }}
					</v-icon>
				</td>
				<td class="text-xs-left">{{ props.item.whiteDomain }}</td>
				<td class="text-xs-left hidden-xs-only">{{ props.item.blackDomain }}</td>
				<td class="hidden-xs-only">
					<span v-for="summ in props.item.summary">
						{{summ.count}}
						<v-icon :class="getColorIpStatus(summ.status) + 'text-xs-center'">
							{{ summ.status | statusIpIcon }}
						</v-icon>
					</span>
				</td>
				<td @click="showInfo(props.item.id);">
					<a>
						<v-icon x-large>more_horiz</v-icon>
					</a>
				</td>
			</tr>
		</template>
		<template slot="no-data">
			<v-alert :value="true" color="error" icon="warning" outline>
				Нет данных
			</v-alert>
		</template>
	</v-data-table>
</template>

<script lang="ts">
	import Vue from 'vue';
	import { Component, Prop, Watch } from 'vue-property-decorator';
	import DnsAttack from '@/models/dns-attack';
	import Utils from '@/utils/Utils';
	import IDataTableHeaders from '@/models/data-table';
	import { EventBus } from '@/utils/event-bus';

	@Component({
		filters: {
			statusIcon(status: number) {
				return Utils.getStatusIcon(status);
			},
			statusIpIcon(status: number) {
				return Utils.getStatusIpIcon(status);
			},
		},
	})
	export default class AttackTable extends Vue {
		@Prop() public attacks: DnsAttack[];
		@Prop() public selectedAttacks: DnsAttack[];
		@Prop() public tableUpdating: boolean;
		@Prop() public tableFilters: ({
			showDymanic: boolean,
			showCompleted: boolean,
		});
		@Prop() public isShowingInfo: boolean;
		@Prop() public filterExpr: string;

		public checkBoxedAttacks: DnsAttack[] = [];

		public selectedId: number = 0;
		public isShowInfo: boolean = false;
		public search: string = '';
		public tableHeaders: IDataTableHeaders[] = [
			{ text: '', value: '', sortable: false, align: 'left', width: '50px', class: 'hidden-xs-only' },
			{ text: 'Статус', value: 'status', align: 'center', width: '100px', class: 'pl-0 pr-2' },
			{ text: 'Атакуемый домен', value: 'whiteDomain' },
			{ text: 'Атакующий домен', value: 'blackDomain', class: 'hidden-xs-only' },
			{ text: 'IP', value: 'summary', width: '150px', class: 'hidden-xs-only', align: 'center' },
			{ text: 'Подробнее', value: '', width: '100px', align: 'center' },
		];
		public tableSort: any = {rowsPerPage: -1 };

		public highLightRow(id: number): string {
			let style = '';
			if (this.isShowingInfo && id === this.selectedId) {
				style = 'blue lighten-3';
			}
			return style;
		}

		public showInfo(id: number) {
			this.$emit('showattackinfo', id);
		}
		public showRow(status: number): boolean {

			if (status === 4 && !this.tableFilters.showDymanic) {
				return false;
			} else if (status === 5 && !this.tableFilters.showCompleted) {
				return false;
			} else {
				return true;
			}
		}

		public getColorStatus(status: number): string { return Utils.getStatusColor(status); }
		public getColorIpStatus(status: number): string { return Utils.getStatusIpColor(status); }

		public created() {
			EventBus.$on('clearselectedattacks', () => {
				this.checkBoxedAttacks = this.checkBoxedAttacks.slice(0, 0);
			});
			EventBus.$on('updateSelectedId', (id: string) => this.selectedId = parseInt(id, 10));
		}

		get dnsAttacks(): DnsAttack[] {
			return this.attacks;
		}

		get isDnsAdmin(): boolean {
			return Utils.getUser(this).isDnsAdmin;
		}

		get tableHeader(): IDataTableHeaders[] {
			if (!this.isDnsAdmin) {
				return this.tableHeaders
					.filter((val, indx) => indx !== 0);
			} else {
				return this.tableHeaders;
			}
		}

		@Watch('checkBoxedAttacks')
		private onCheckBoxedAttacks() {
			this.$emit('selectedupdate', this.checkBoxedAttacks);
		}
	}
</script>

<style scoped>
	.bordered {
		box-sizing: border-box;
		border: solid #2196f3 1px;
	}

	.rowHover:hover {
		background: #90caf9 !important;
	}
</style>
<style>
	.tile > div {
		height: auto !important;
	}
</style>