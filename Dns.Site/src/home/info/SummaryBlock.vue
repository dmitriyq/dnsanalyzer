<template>
	<v-row no-gutters>
		<v-col cols="12">
			<v-list dense>
				<v-subheader class="subtitle-1 primary white--text">
					Общая информация
				</v-subheader>
				<StatusRow title="Текущий статус" :status="attack.status" statType="group" />
				<InfoRow title="Атакуемый домен" :desc="attack.whiteDomain" />
				<InfoRow title="Атакующий домен" :desc="attack.blackDomain" />
				<InfoRow title="Дата обнаружения" :desc="attack.begin" />
				<InfoRow title="Дата завершения" :desc="attack.close" />
				<v-divider></v-divider>
				<v-list-item>
					<v-list-item-content v-if="isDnsAdmin">
						<v-row>
							<v-col cols="12" md="4">
								<EditAttack :isEditStatus="true"
											:attackFull="attack" />
							</v-col>
						</v-row>
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
										<InfoRow v-for="history in attack.histories" :key="history.id"
												 :title="history.create" :desc="changeStatusText(history.prevStatus, history.currentStatus)">
										</InfoRow>
									</v-list>
								</v-expansion-panel-content>
							</v-expansion-panel>
							<v-expansion-panel>
								<v-expansion-panel-header class="pa-1">
									<div class="subtitle-1">Комментарии</div>
								</v-expansion-panel-header>
								<v-expansion-panel-content>
									<v-list dense>
										<InfoRow v-for="note in attack.notes" :key="note.id"
												 :title="note.create" :desc="note.text" />
										<EditAttack :attackFull="attack"
													v-if="isDnsAdmin"
													:isEditStatus="false" />
									</v-list>
								</v-expansion-panel-content>
							</v-expansion-panel>
						</v-expansion-panels>
					</v-list-item-content>
				</v-list-item>
			</v-list>
		</v-col>
	</v-row>
</template>

<script lang="ts">
	import Vue from 'vue';
	import InfoRow from '@/home/info/InfoRow.vue';
	import StatusRow from '@/home/info/StatusRow.vue';
	import EditAttack from '@/home/EditAttack.vue';
	import { Component, Prop } from 'vue-property-decorator';
	import { IDnsAttackInfo } from '@/home/dns-attack';
	import Utils from '@/utils/Utils';
	import ISelectModel from '@/models/select-model';
	import Axios from 'axios';

	@Component({
		components: {
			InfoRow,
			StatusRow,
			EditAttack,
		},
	})
	export default class SummaryBlock extends Vue {
		@Prop() public attack: IDnsAttackInfo;

		public statuses: Array<ISelectModel<number>> = [];

		get isDnsAdmin(): boolean {
			return this.$store.getters.isAdmin as boolean;
		}

		public beforeMount() {
			Axios.get(`/api/attack/groupStatuses`)
				.then((resp) => this.statuses = resp.data as Array<ISelectModel<number>>);
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

<style scoped>
</style>