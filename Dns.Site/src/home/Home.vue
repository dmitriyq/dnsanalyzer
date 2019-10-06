<template>
	<v-container fluid grid-list-md text-xs-center fill-height class="pb-0">
		<v-layout row wrap fluid>
			<v-flex xs12>
				<v-expansion-panel>
					<v-expansion-panel-content ripple>
						<div slot="header">Фильтр записей</div>
						<v-layout align-start justify-start row wrap class="mx-3">
							<v-flex>
								<v-switch label="Показывать динамические IP"
										  color="primary"
										  v-model="tableFilters.showDymanic"
										  @change="saveFilter">
								</v-switch>
							</v-flex>
							<v-flex>
								<v-switch label="Показывать законченные атаки"
										  color="primary"
										  v-model="tableFilters.showCompleted"
										  @change="saveFilter">
								</v-switch>
							</v-flex>
						</v-layout>
					</v-expansion-panel-content>
				</v-expansion-panel>
				<v-card-title class="py-0">
					<v-flex v-if="selected.length > 0 && isDnsAdmin" xs5 sm1>
						<EditAttack :isEditStatus="true"
									:attacks="selected"
									name="editStatus"></EditAttack>
					</v-flex>
					<v-spacer></v-spacer>
					<v-flex xs12 sm6>
						<v-text-field v-model="search"
									  append-icon="search"
									  label="Поиск"
									  single-line
									  hide-details></v-text-field>
					</v-flex>
				</v-card-title>
				<AttackTable @selectedupdate="showMultiActionButton"
							 @showattackinfo="showInfo"
							 :isShowingInfo="isShowInfo"
							 :attacks="dnsAttacks"
							 :filterExpr="search"
							 :tableUpdating="tableUpdating"
							 :tableFilters="tableFilters">
				</AttackTable>
			</v-flex>
			<v-flex>
				<InfoContainer :isShowingInfo="isShowInfo" @hideinfo="onHideInfo">
				</InfoContainer>
			</v-flex>
		</v-layout>
	</v-container>
</template>

<script lang="ts">
	import Vue from 'vue';
	import { Component, Prop, Watch } from 'vue-property-decorator';
	import EditAttack from '@/home/EditAttack.vue';
	import InfoContainer from '@/home/InfoContainer.vue';
	import AttackTable from '@/home/AttackTable.vue';
	import DnsAttack from '@/models/dns-attack';
	import DateRangeOptions from '@/models/date-range';
	import { format as fnsFormat } from 'date-fns';
	import Utils from '@/utils/Utils';
	import * as signalR from '@microsoft/signalr';
	import { EventBus } from '@/utils/event-bus';
	import DnsLocalStorage from '@/models/local-storage';

	@Component({
		components: {
			EditAttack,
			InfoContainer,
			AttackTable,
		},
		filters: {
			statusIcon(status: number) {
				return Utils.getStatusIcon(status);
			},
			formatDate(value: Date) {
				return fnsFormat(value, 'DD.MM.YYYY HH:mm');
			},
		},
	})
	export default class HomeComponent extends Vue {

		public dnsAttacks: DnsAttack[] = [];
		public selected: DnsAttack[] = [];
		public isShowInfo: boolean = false;
		public tableUpdating: boolean = true;
		public selectedId: number | null;
		public attackHub: signalR.HubConnection;
		public search: string = '';

		public tableFilters = {
			showDymanic: false,
			showCompleted: false,
		};

		public dateChange(from: Date | null, to: Date | null) {
			this.attackHub.send('AttackList', from, to)
				.then(() => this.tableUpdating = true);
		}
		public monthChanged(month: Date) {
			if (this.hubIsConnected()) {
				this.attackHub.send('getCalendarEvents', month, false);
			} else {
				window.setTimeout(() => this.monthChanged(month), 200);
			}
		}

		public showMultiActionButton(atts: DnsAttack[]) {
			this.selected = atts;
		}

		public created() {
			this.tableFilters = DnsLocalStorage.getTableFilters();
			this.initHub();
		}
		public beforeMount() {
			const urlQuery = this.$router.currentRoute.query;
			const id = urlQuery.id as string;
			if (urlQuery && id) {
				this.showInfo(parseInt(id, 10));
			}
		}

		get isDnsAdmin(): boolean {
			return Utils.getUser(this).isDnsAdmin;
		}

		public showInfo(id: number, isReload: boolean = false) {
			if (id === this.selectedId) {
				this.isShowInfo = !this.isShowInfo;
			} else {
				this.isShowInfo = true;
			}
			if (!this.isShowInfo) {
				this.selectedId = null;
				this.$router.push('/');
			} else {
				this.selectedId = id;
				EventBus.$emit('updateSelectedId', id);
				this.$router.push({ name: 'DnsAttackInfo', params: { id: this.selectedId.toString() } });
			}
		}

		private hubIsConnected(): boolean {
			const state = (this.attackHub as any).connection.connectionState;
			return state === 1 ? true : false;
		}

		private initHub() {
			this.attackHub = new signalR.HubConnectionBuilder()
				.withUrl('/attackHub').build();
			this.attackHub.start()
				.then(() => {
					this.attackHub.send('AttackList', null, null)
						.then(() => this.tableUpdating = true);
				})// tslint:disable-next-line:no-console
				.catch((err: any) => console.error(err));

			this.attackHub.on('Attacks', (attackList: DnsAttack[]) => {
				this.dnsAttacks = attackList;
				this.tableUpdating = false;
			});
			this.attackHub.on('UpdateAttack', (updatedAttack: DnsAttack) => {
				const oldInfo = this.dnsAttacks.findIndex((val) => val.id === updatedAttack.id);
				if (oldInfo !== -1) {
					this.dnsAttacks.splice(oldInfo, 1, updatedAttack);
					if (this.selectedId === updatedAttack.id && this.isShowInfo) {
						EventBus.$emit('update-info');
					}
				} else {
					this.dnsAttacks.unshift(updatedAttack);
				}
				this.dnsAttacks = this.dnsAttacks.sort((one, two) => {
					if (one.status < two.status) {
						return -1;
					}
					if (one.status > two.status) {
						return 1;
					}
					return 0;
				});
				this.monthChanged(new Date((this.$refs.calendComp as any).selectedMonth));
			});
			this.attackHub.onclose(() => {
				this.initHub();
			});
		}
		private onHideInfo() {
			this.isShowInfo = false;
			this.selectedId = null;
			this.$router.push('/');
		}

		private saveFilter(): void {
			DnsLocalStorage.setTableFilters(this.tableFilters);
		}
	}
</script>

<style scoped>
	.bordered {
		box-sizing: border-box;
		border: solid #2196f3 1px;
	}

	.rowHover:hover {
		background: #E3F2FD !important;
	}
</style>