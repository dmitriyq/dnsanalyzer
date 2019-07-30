<template>
	<v-flex class="elevation-1 m-0 p-0 pb-3">
		<v-list subheader>
			<v-subheader class="blue lighten-4 subheading">
				Информация о Доменах
			</v-subheader>
		</v-list>
		<v-list-tile-content>
			<v-expansion-panel v-model="panels" expand>
				<v-expansion-panel-content lazy v-if="whiteDomain!==null">
					<template v-slot:header>
						<div class="subheading font-weight-medium">{{whiteDomain.domain}}</div>
					</template>
					<v-list-tile-content class="pt-2 pr-3 pb-0 pl-4">
						<InfoRow title="Ограцнизация" :desc="whiteDomain.company"></InfoRow>
						<InfoRow title="Регистратор" :desc="whiteDomain.registrant"></InfoRow>
						<InfoRow title="Дата регистрации" :desc="whiteDomain.dateCreate"></InfoRow>
						<InfoRow title="Срок регистрации" :desc="whiteDomain.dateUntil"></InfoRow>
						<InfoRow title="NameServers" :desc="whiteDomain.nameServers"></InfoRow>
					</v-list-tile-content>
					<v-divider></v-divider>
				</v-expansion-panel-content>
				<v-expansion-panel-content lazy v-if="blackDomain!==null">
					<template v-slot:header>
						<div class="subheading font-weight-medium">{{blackDomain.domain}}</div>
					</template>
					<v-list-tile-content class="pt-2 pr-3 pb-0 pl-4">
						<InfoRow title="Ограцнизация" :desc="blackDomain.company"></InfoRow>
						<InfoRow title="Регистратор" :desc="blackDomain.registrant"></InfoRow>
						<InfoRow title="Дата регистрации" :desc="blackDomain.dateCreate"></InfoRow>
						<InfoRow title="Срок регистрации" :desc="blackDomain.dateUntil"></InfoRow>
						<InfoRow title="NameServers" :desc="blackDomain.nameServers"></InfoRow>
					</v-list-tile-content>
					<v-divider></v-divider>
				</v-expansion-panel-content>
			</v-expansion-panel>
		</v-list-tile-content>
		<v-divider></v-divider>
	</v-flex>
</template>

<script lang="ts">
	import Vue from 'vue';
	import InfoRow from '@/home/info/InfoRow.vue';
	import { Component, Prop } from 'vue-property-decorator';
	import { IDomainInfo } from '@/models/dns-attack';
	import ISelectModel from '../../models/select-model';

	@Component({
		components: {
			InfoRow,
		},
	})
	export default class DomainBlock extends Vue {
		@Prop() public whiteDomain: IDomainInfo;
		@Prop() public blackDomain: IDomainInfo;

		public panels: boolean[] = [false, false];
	}
</script>
<style scoped>

</style>