<template>
	<v-container fluid>
		<v-row>
			<v-col cols="12">
				<AttackFilterPanel :filters.sync="tableFilters"/>
			</v-col>
		</v-row>
		<v-row justify="space-between">
			<v-col cols="4">
				<EditAttack v-if="selected.length > 0 && isDnsAdmin"
							:isEditStatus="true"
							:attacks="selected"
							name="editStatus"></EditAttack>
			</v-col>
			<v-col cols="6" class="pr-5" style="width:50%">
				<v-text-field v-model="search"
							  append-icon="search"
							  label="Поиск"
							  single-line
							  hide-details></v-text-field>
			</v-col>
		</v-row>
		<v-row>
			<v-col cols="12">
				<AttackTable @showattackinfo="showInfo"
							 :isAdmin="isDnsAdmin"
							 :attacks.sync="dnsAttacks"
							 :selectedAttacks.sync="selected"
							 :filterExpr="search"
							 :tableUpdating="tableUpdating"
							 :tableFilters="tableFilters">
				</AttackTable>
			</v-col>
		</v-row>
		<v-layout row wrap container--fluid>
			<v-flex xs12>
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
	import AttackFilterPanel from '@/home/AttackFilterPanel.vue';
	import EditAttack from '@/home/EditAttack.vue';
	import InfoContainer from '@/home/InfoContainer.vue';
	import AttackTable from '@/home/AttackTable.vue';
	import DnsAttack from '@/models/dns-attack';
	import { format as fnsFormat } from 'date-fns';
	import Utils from '@/utils/Utils';
	import * as signalR from '@microsoft/signalr';
	import { EventBus } from '@/utils/event-bus';
	import { IAttackTableFilters, DnsLocalStorage } from '@/models/local-storage';

	@Component({
		components: {
			AttackFilterPanel,
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

		public showDynamic: false;
		public showCompleted: false;
		public tableFilters = {
			showDynamic: false,
			showCompleted: false,
		};

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
			return true;
			//return Utils.getUser(this).isDnsAdmin;
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

		@Watch('tableFilters', { deep: true })
		private onTableFilterChanged(val: IAttackTableFilters, oldVal: IAttackTableFilters) {
			DnsLocalStorage.setTableFilters(val);
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