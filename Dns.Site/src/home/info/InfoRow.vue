<template>
	<v-list-tile-title class="height-auto">
		<v-layout row wrap>
			<v-flex xs5 wrap class="body-2">
				{{ title | format }}:
			</v-flex>
			<v-flex xs7 wrap class="body-1 text-wrap-word">
				{{ desc | format }}
			</v-flex>
		</v-layout>
	</v-list-tile-title>
</template>

<script lang="ts">

import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import { format as fnsFormat } from 'date-fns';
import isValid from 'date-fns/is_valid';

@Component({
	filters: {
		format(value: any) {
			if (value === null) {
				return '-';
			}
			else if (typeof value === 'string') {
				if (isValid(new Date(value))) {
					return fnsFormat(value, 'DD.MM.YYYY HH:mm');
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
				return fnsFormat(value, 'DD.MM.YYYY HH:mm');
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
	.text-wrap-word{
		white-space: normal;
		word-break:break-word;
	}
	.height-auto{
		height:auto;
	}
</style>