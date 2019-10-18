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
							 :attacks.sync="dnsAttacks"
							 :selectedAttacks.sync="selected"
							 :filterExpr="search"
							 :tableUpdating="tableUpdating"
							 :tableFilters="tableFilters">
				</AttackTable>
			</v-col>
		</v-row>
		<v-row>
			<v-col cols="12">
				<InfoContainer @hideinfo="onHideInfo"/>
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
	import Vue from 'vue';
	import { Component, Prop, Watch } from 'vue-property-decorator';
	import AttackFilterPanel from '@/home/AttackFilterPanel.vue';
	import EditAttack from '@/home/EditAttack.vue';
	import InfoContainer from '@/home/InfoContainer.vue';
	import AttackTable from '@/home/AttackTable.vue';
	import DnsAttack from '@/home/dns-attack';
	import format from 'date-fns/format';
	import Utils from '@/utils/Utils';
	import * as signalR from '@microsoft/signalr';
	import { EventBus } from '@/utils/event-bus';
	import DnsLocalStorage from '@/models/local-storage';
	import IAttackTableFilters from '@/home/attack-table-filters';

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
				return format(value, 'DD.MM.YYYY HH:mm');
			},
		},
	})
	export default class HomeComponent extends Vue {

		public dnsAttacks: DnsAttack[] = [];
		public selected: DnsAttack[] = [];
		public tableUpdating: boolean = true;
		public selectedId: number | null;
		public attackHub: signalR.HubConnection;
		public search: string = '';

		public tableFilters: IAttackTableFilters = {
			showDynamic: false,
			showCompleted: false,
		};

		public created() {
			this.tableFilters = DnsLocalStorage.getTableFilters();
			this.initHub();
			if (this.isSideModalOpened && this.$route.query.id === undefined) {
				this.isSideModalOpened = false;
			}
		}
		public beforeMount() {
			const urlQuery = this.$router.currentRoute.query;
			const id = urlQuery.id as string;
			if (urlQuery && id) {
				this.showInfo(parseInt(id, 10));
			}
		}

		get isDnsAdmin(): boolean {
			return this.$store.getters.isAdmin as boolean;
		}

		get isSideModalOpened(): boolean {
			return this.$store.state.sideDialogOpened;
		}
		set isSideModalOpened(val: boolean) {
			this.$store.dispatch('updateSideDialogState', val);
		}

		public showInfo(id: number) {

			if (id === this.selectedId) {
				this.isSideModalOpened = !this.isSideModalOpened;
			} else {
				this.isSideModalOpened = true;
			}
			if (!this.isSideModalOpened) {
				this.selectedId = null;
				this.$router.push('/');
			} else {
				this.selectedId = id;
				this.$router.push({ name: 'DnsAttackInfo', params: { id: this.selectedId.toString() } });
			}
		}

		@Watch('tableFilters', { deep: true })
		private onTableFilterChanged(val: IAttackTableFilters, oldVal: IAttackTableFilters) {
			DnsLocalStorage.setTableFilters(val);
		}

		private initHub() {
			this.attackHub = new signalR.HubConnectionBuilder().withAutomaticReconnect({
				nextRetryDelayInMilliseconds: ((c) => 1000),
			}).withUrl('/attackHub').build();
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
					if (this.selectedId === updatedAttack.id && this.isSideModalOpened) {
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
			this.$store.dispatch('updateSideDialogState', false);
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