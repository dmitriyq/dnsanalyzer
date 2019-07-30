<template>
	<v-flex class="elevation-1 m-0 p-0 pb-3">
		<v-list subheader>
			<v-subheader class="blue lighten-4 subheading">
				Информация о IP
			</v-subheader>
		</v-list>
		<v-list-tile-content>
			<v-expansion-panel v-model="ipPanels" expand>
				<v-expansion-panel-content v-for="ip in attack.attacks" :key="ip.id" class="ip-panel" lazy>
					<template v-slot:header>
						<div class="subheading font-weight-medium">
							<StatusRow :title="ip.ip" :status="ip.status" statType="ip"></StatusRow>
						</div>
					</template>
					<v-list-tile-content class="pt-2 pr-3 pb-0 pl-4">
						<InfoRow title="Ограцнизация" :desc="ip.ipInfo && ip.ipInfo.company"></InfoRow>
						<InfoRow title="Страна" :desc="ip.ipInfo && ip.ipInfo.country"></InfoRow>
						<InfoRow title="Подсеть" :desc="ip.ipInfo && ip.ipInfo.subnet"></InfoRow>
						<InfoRow title="IP в реестре" :desc="ip.ipBlocked"></InfoRow>
						<InfoRow title="Подсеть в реестре" :desc="ip.subnetBlocked"></InfoRow>
						<v-list-tile-title style="height:auto;">
							<v-layout row wrap>
								<v-flex xs5 wrap class="body-2">IPv4Info:</v-flex>
								<v-flex xs7 wrap class="body-1">
									<v-btn :href="'http://ipv4info.ru/?act=check&ip='+ip.ip" target="_blank" style="float:right;">
										Просмотреть
									</v-btn>
								</v-flex>
							</v-layout>
						</v-list-tile-title>
					</v-list-tile-content>
					<v-divider></v-divider>
					<v-list-tile-content>
						<v-expansion-panel>
							<v-expansion-panel-content lazy>
								<template v-slot:header>
									<div class="subheading">История изменений</div>
								</template>
								<v-divider></v-divider>
								<v-list>
									<v-list-tile-content class="pt-2 pr-3 pb-0 pl-5">
										<InfoRow v-for="history in ip.histories" :key="history.id"
												 :title="history.create" :desc="changeStatusText(history.prevStatus, history.currentStatus)">
										</InfoRow>
									</v-list-tile-content>
								</v-list>
							</v-expansion-panel-content>
						</v-expansion-panel>
					</v-list-tile-content>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-list-tile-content>
		<v-divider></v-divider>
	</v-flex>
</template>

<script lang="ts">
	import Vue from 'vue';
	import InfoRow from '@/home/info/InfoRow.vue';
	import StatusRow from '@/home/info/StatusRow.vue';
	import { Component, Prop } from 'vue-property-decorator';
	import { IDnsAttackInfo, IAttackInfo } from '@/models/dns-attack';
	import Utils from '@/utils/Utils';
	import ISelectModel from '../../models/select-model';
	import Axios from 'axios';

	@Component({
		components: {
			InfoRow,
			StatusRow,
		},
	})
	export default class IpBlock extends Vue {
		@Prop() public attack: IDnsAttackInfo;

		public statuses: Array<ISelectModel<number>> = [];
		public ipPanels = this.attack.attacks.map((att) => false);

		get displayingStatuses(): Array<ISelectModel<number>> {
			return this.statuses.filter((st, i) => st.value !== 0);
		}

		public beforeMount() {
			Axios.get(`/api/attack/statuses`)
				.then((resp) => this.statuses = resp.data as Array<ISelectModel<number>>);
		}

		private statusCount(status: number): number {
			return this.attack.attacks.filter((att, indx) => att.status === status).length;
		}
		private changeStatusText(prevStatus: number, currentStatus: number): string {
			const prev = Utils.getSelectStatus(prevStatus, this.statuses).text;
			const curr = Utils.getSelectStatus(currentStatus, this.statuses).text;
			if (!prev) {
				return `${curr}`;
			} else {
				return `${prev} -> ${curr}`;
			}
		}
	}
</script>
<style>
	.ip-panel > .v-expansion-panel__header{
		padding-top: 0px;
		padding-bottom:0px;
		padding-left: 18px;
	}
</style>
<style scoped>

</style>