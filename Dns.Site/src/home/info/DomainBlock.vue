<template>
	<v-row no-gutters>
		<v-col cols="12" v-if="whiteDomain !== null && blackDomain !== null">
			<v-list dense>
				<v-subheader class="subtitle-1 primary white--text">
					Информация о Доменах
				</v-subheader>
				<v-list-item>
					<v-list-item-content>
						<v-expansion-panels multiple>
							<v-expansion-panel :readonly="whiteDomain === null">
								<v-expansion-panel-header :hide-action="whiteDomain === null" class="pa-1">
									<span v-if="whiteDomain !== null && whiteDomain.domain !== null">{{whiteDomain.domain}}</span>
								</v-expansion-panel-header>
								<v-expansion-panel-content  v-if="whiteDomain !== null">
									<v-list dense>
										<InfoRow title="Ограцнизация" :desc="whiteDomain.company"></InfoRow>
										<InfoRow title="Регистратор" :desc="whiteDomain.registrant"></InfoRow>
										<InfoRow title="Дата регистрации" :desc="whiteDomain.dateCreate"></InfoRow>
										<InfoRow title="Срок регистрации" :desc="whiteDomain.dateUntil"></InfoRow>
										<InfoRow title="NameServers" :desc="whiteDomain.nameServers"></InfoRow>
									</v-list>
								</v-expansion-panel-content>
							</v-expansion-panel>
							<v-expansion-panel :readonly="blackDomain === null">
								<v-expansion-panel-header :hide-action="blackDomain === null" class="pa-1">
									<span v-if="blackDomain !== null && blackDomain.domain !== null">{{blackDomain.domain}}</span>
								</v-expansion-panel-header>
								<v-expansion-panel-content v-if="blackDomain !== null">
									<v-list dense>
										<InfoRow title="Ограцнизация" :desc="blackDomain.company"></InfoRow>
										<InfoRow title="Регистратор" :desc="blackDomain.registrant"></InfoRow>
										<InfoRow title="Дата регистрации" :desc="blackDomain.dateCreate"></InfoRow>
										<InfoRow title="Срок регистрации" :desc="blackDomain.dateUntil"></InfoRow>
										<InfoRow title="NameServers" :desc="blackDomain.nameServers"></InfoRow>
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
	import { Component, Prop } from 'vue-property-decorator';
	import { IDomainInfo } from '@/home/dns-attack';
	import ISelectModel from '../../models/select-model';

	@Component({
		components: {
			InfoRow,
		},
	})
	export default class DomainBlock extends Vue {
		@Prop() public whiteDomain: IDomainInfo;
		@Prop() public blackDomain: IDomainInfo;
	}
</script>
<style scoped>
</style>