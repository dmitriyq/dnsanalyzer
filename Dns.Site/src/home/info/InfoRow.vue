<template>
	<v-list-item class="height-28">
		<v-list-item-content class="pa-0">
			<v-container fluid class="pa-0">
				<v-row no-gutters>
					<v-col col="5" class="body-2">
						{{ title | format }}:
					</v-col>
					<v-col col="7" class="body-1 text-wrap-word">
						{{ desc | format }}
					</v-col>
				</v-row>
			</v-container>
		</v-list-item-content>
	</v-list-item>
</template>

<script lang="ts">

import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import { format as fnsFormat, parseISO } from 'date-fns';
import isValid from 'date-fns/isValid';

@Component({
	filters: {
		format(value: any) {
			if (value === null) {
				return '-';
			}
			else if (typeof value === 'string') {
				if (isValid(new Date(value))) {
					const date = parseISO(value);
					return fnsFormat(date, 'dd.MM.yyyy HH:mm');
				} else {
					return value as string;
				}
			} else if (typeof value === 'boolean') {
				const valBool = value as boolean;
				if (valBool) {
					return 'Да';
				} else {
					return 'Нет';
				}
			} else if (value instanceof Date) {
				return fnsFormat(value, 'dd.MM.yyyy HH:mm');
			} else if (typeof value === 'number') {
				return value.toString();
			} else if (Array.isArray(value)) {
				return (value as any[]).join(', ');
			}
		},
	},
})
export default class InfoRow extends Vue {
	@Prop() public title: string;
	@Prop() public desc: any;
}
</script>

<style scoped>
	.text-wrap-word {
		white-space: normal;
		word-break: break-word;
	}

	.height-28 {
		min-height: 28px;
	}
</style>