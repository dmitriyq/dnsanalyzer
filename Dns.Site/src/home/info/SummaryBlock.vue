<template>
	<v-flex class="elevation-1 m-0 p-0 mb-3">
		<v-list subheader>
			<v-subheader class="blue lighten-4 subheading">
				Общая информация
			</v-subheader>
		</v-list>
		<v-list-tile-content class="pt-2 pr-3 pb-0 pl-3">
			<StatusRow title="Текущий статус" :status="attack.status" statType="group"></StatusRow>
			<InfoRow title="Атакуемый домен" :desc="attack.whiteDomain"></InfoRow>
			<InfoRow title="Атакующий домен" :desc="attack.blackDomain"></InfoRow>
			<InfoRow title="Дата обнаружения" :desc="attack.begin"></InfoRow>
			<InfoRow title="Дата завершения" :desc="attack.close"></InfoRow>
		</v-list-tile-content>
		<v-divider></v-divider>
		<v-list-tile-content v-if="isDnsAdmin" class="w-100">
			<EditAttack :isEditStatus="true"
						:attackFull="attack"
						name="editStatus"></EditAttack>
		</v-list-tile-content>
		<v-list-tile-content>
			<v-expansion-panel v-model="panelBlocks.history" expand>
				<v-expansion-panel-content lazy>
					<template v-slot:header>
						<div class="subheading">История изменений</div>
					</template>
					<v-divider></v-divider>
					<v-list>
						<v-list-tile-content class="pt-2 pr-3 pb-0 pl-4">
							<InfoRow v-for="history in attack.histories" :key="history.id"
									 :title="history.create" :desc="changeStatusText(history.prevStatus, history.currentStatus)">
							</InfoRow>
						</v-list-tile-content>
					</v-list>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-list-tile-content>
		<v-divider></v-divider>
		<v-list-tile-content>
			<v-expansion-panel v-model="panelBlocks.notes" expand>
				<v-expansion-panel-content lazy>
					<template v-slot:header>
						<div class="subheading">Комментарии</div>
					</template>
					<v-divider></v-divider>
					<v-list>
						<v-list-tile-content class="pt-2 pr-3 pb-0 pl-4">
							<InfoRow v-for="note in attack.notes" :key="note.id"
									 :title="note.create" :desc="note.text">
							</InfoRow>
							<EditAttack :attackFull="attack"
										name="addComment"
										v-if="isDnsAdmin"
										:isEditStatus="false"></EditAttack>
						</v-list-tile-content>
					</v-list>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-list-tile-content>
	</v-flex>
</template>

<script lang="ts">
	import Vue from 'vue';
	import InfoRow from '@/home/info/InfoRow.vue';
	import StatusRow from '@/home/info/StatusRow.vue';
	import EditAttack from '@/home/EditAttack.vue';
	import { Component, Prop } from 'vue-property-decorator';
	import { IDnsAttackInfo } from '@/models/dns-attack';
	import Utils from '@/utils/Utils';
	import ISelectModel from '../../models/select-model';
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
		public panelBlocks = {
			history: [false],
			notes: [false],
		};

		get isDnsAdmin(): boolean {
			return Utils.getUser(this).isDnsAdmin;
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