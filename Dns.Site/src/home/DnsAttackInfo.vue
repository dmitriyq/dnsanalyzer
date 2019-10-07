<template>
	<v-container container--fluid style="padding:0;">
		<v-layout row wrap>
			<v-flex>
				<v-progress-circular :size="70"
									 :width="7"
									 v-if="isLoading"
									 color="primary"
									 indeterminate>
				</v-progress-circular>
				<v-flex v-if="!isLoading">
					<v-flex xs12 class="text-center headline p-0 info--text">
						Информация об атаке
					</v-flex>
					<SummaryBlock :attack="attack"></SummaryBlock>
					<DomainBlock :whiteDomain="attack.whiteDomainInfo" :blackDomain="attack.blackDomainInfo"></DomainBlock>
					<IpBlock :attack="attack"></IpBlock>
				</v-flex>
			</v-flex>
		</v-layout>
	</v-container>
</template>

<script lang="ts">
	import Vue from 'vue';
	import SummaryBlock from '@/home/info/SummaryBlock.vue';
	import IpBlock from '@/home/info/IpBlock.vue';
	import DomainBlock from '@/home/info/DomainBlock.vue';
	import { Component, Prop, Watch } from 'vue-property-decorator';
	import { IDnsAttackInfo } from '@/models/dns-attack';
	import { EventBus } from '@/utils/event-bus';
	import ISelectModel from '@/models/select-model';
	import Axios from 'axios';
	import Utils from '@/utils/Utils';

	@Component({
		components: {
			SummaryBlock,
			IpBlock,
			DomainBlock,
		},
	})
	export default class DnsAttackInfo extends Vue {
		@Prop() public id: number;

		public dnsAttackInfo: IDnsAttackInfo = {
			attacks: [],
			begin: new Date(),
			blackDomain: '',
			blackDomainInfo: {
				company: null, dateCreate: null, dateUntil: null,
				domain: '', nameServers: [], registrant: null,
			},
			close: null,
			histories: [],
			id: 0,
			notes: [],
			status: 0,
			whiteDomain: '',
			whiteDomainInfo: {
				company: null, dateCreate: null, dateUntil: null,
				domain: '', nameServers: [], registrant: null,
			},
		};
		public isLoading: boolean = true;
		public statuses: Array<ISelectModel<number>> = [];
		public groupStatuses: Array<ISelectModel<number>> = [];
		public currentStatus: ISelectModel<number> = { text: '', value: -1 };

		public getSelectStatus(status: number): ISelectModel<number> {
			return Utils.getSelectStatus(this.attack.status, this.statuses);
		}
		public getSelectGroupStatus(status: number): ISelectModel<number> {
			return Utils.getSelectStatus(status, this.groupStatuses);
		}

		public created() {
			EventBus.$on('update-info', () => {
				if (this.id !== 0) {
					this.isLoading = true;
					this.loadInfo(this.id);
				}
			});
		}

		public beforeMount() {
			this.loadInfo(this.id);
		}

		public beforeDestroy() {
			(this.$parent.$parent.$parent as any).isShowInfo = false;
		}

		get attack(): IDnsAttackInfo {
			return this.dnsAttackInfo;
		}

		private loadInfo(id: number) {
			Axios.get(`/api/attack/info?id=${id}`)
				.then((resp) => {
					this.dnsAttackInfo = resp.data as IDnsAttackInfo;
					this.isLoading = false;
				});
			EventBus.$emit('updateSelectedId', id);
		}

		@Watch('id')
		private onIdChanged(val: number, oldVal: number) {
			if (val !== oldVal) {
				this.isLoading = true;
				this.loadInfo(val);
			}
		}
	}
</script>

<style scoped>
</style>