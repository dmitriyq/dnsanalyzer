<template>
	<v-row no-gutters>
		<v-col cols="12">
			<v-list dense>
				<v-subheader class="subtitle-1 primary white--text">
					Информация о IP
				</v-subheader>
			</v-list>
			<v-list-item>
				<v-list-item-content>
					<v-expansion-panels multiple>
						<v-expansion-panel v-for="ip in attack.attacks" :key="ip.id" class="ip-panel">
							<v-expansion-panel-header class="pa-1">
								<div class="subheading font-weight-medium">
									<StatusRow :title="ip.ip" :status="ip.status" statType="ip"></StatusRow>
								</div>
							</v-expansion-panel-header>
							<v-expansion-panel-content>
                <v-list dense>
                  <InfoRow title="Ограцнизация" :desc="ip.ipInfo && ip.ipInfo.company"></InfoRow>
                  <InfoRow title="Страна" :desc="ip.ipInfo && ip.ipInfo.country"></InfoRow>
                  <InfoRow title="Подсеть" :desc="ip.ipInfo && ip.ipInfo.subnet"></InfoRow>
                  <InfoRow title="IP в реестре" :desc="ip.ipBlocked"></InfoRow>
                  <InfoRow title="Подсеть в реестре" :desc="ip.subnetBlocked"></InfoRow>
                  <v-list-item>
                    <v-list-item-content class="pa-0">
                      <v-container fluid class="pa-0">
                        <v-row no-gutters>
                          <v-col col="5" class="body-2">
                            IPv4Info:
                          </v-col>
                          <v-col col="7" class="body-1 text-wrap-word">
                            <v-btn color="info" :href="'http://ipv4info.ru/?act=check&ip='+ip.ip" target="_blank" style="float:right;">
                              Просмотреть
                            </v-btn>
                          </v-col>
                        </v-row>
                      </v-container>
                    </v-list-item-content>
                  </v-list-item>
                  <v-list-item>
                    <v-list-item-content>
                      <v-expansion-panels multiple>
                        <v-expansion-panel>
                          <v-expansion-panel-header class="pa-1">
                            <div class="subtitle-1">История изменений</div>
                          </v-expansion-panel-header>
                          <v-expansion-panel-content>
                            <v-list dense>
                              <InfoRow v-for="history in ip.histories" :key="history.id"
                                       :title="history.create" :desc="changeStatusText(history.prevStatus, history.currentStatus)">
                              </InfoRow>
                            </v-list>
                          </v-expansion-panel-content>
                        </v-expansion-panel>
                      </v-expansion-panels>
                    </v-list-item-content>
                  </v-list-item>
                </v-list>
							</v-expansion-panel-content>
						</v-expansion-panel>
					</v-expansion-panels>
				</v-list-item-content>
			</v-list-item>
		</v-col>
	</v-row>
</template>

<script lang="ts">
	import Vue from 'vue';
	import InfoRow from '@/home/info/InfoRow.vue';
	import StatusRow from '@/home/info/StatusRow.vue';
	import { Component, Prop } from 'vue-property-decorator';
	import { IDnsAttackInfo, IAttackInfo } from '@/home/dns-attack';
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
	.ip-panel > .v-expansion-panel__header {
		padding-top: 0px;
		padding-bottom: 0px;
		padding-left: 18px;
	}
</style>
<style scoped>
</style>
