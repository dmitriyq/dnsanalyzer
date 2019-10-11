<template>
	<v-expansion-panels accordion>
		<v-expansion-panel>
			<v-expansion-panel-header v-slot="{ open }">
				<v-row>
					<v-col cols="4">Фильтры</v-col>
					<v-col cols="8" class="text--secondary">
						<v-fade-transition leave-absolute>
							<span v-if="open"></span>
							<v-row v-else no-gutters class="hidden-xs-only">
								<v-col cols="6">Отображать динамические IP: {{ syncFilters.showDynamic | boolYesNo }}</v-col>
								<v-col cols="6">Отображать завершенные атаки: {{ syncFilters.showCompleted | boolYesNo }}</v-col>
							</v-row>
						</v-fade-transition>
					</v-col>
				</v-row>
			</v-expansion-panel-header>
			<v-expansion-panel-content>
				<v-row justify="space-around">
					<v-col>
						<v-switch label="Показывать динамические IP" dense hide-details
								  v-model="syncFilters.showDynamic">
						</v-switch>
					</v-col>
					<v-col>
						<v-switch label="Показывать законченные атаки" dense hide-details
								  v-model="syncFilters.showCompleted">
						</v-switch>
					</v-col>
				</v-row>
			</v-expansion-panel-content>
		</v-expansion-panel>
	</v-expansion-panels>
</template>

<script lang="ts">
	import Vue from 'vue';
	import { Component, PropSync } from 'vue-property-decorator';
	import { IAttackTableFilters } from '@/models/local-storage';
	@Component({
		filters: {
			boolYesNo(val: boolean) {
				return val ? 'Да' : 'Нет';
			},
		},
	})
	export default class AttackFilterPanel extends Vue {
		@PropSync('filters') public syncFilters: IAttackTableFilters;
	}
</script>