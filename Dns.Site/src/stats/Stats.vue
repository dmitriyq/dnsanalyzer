<template>
	<v-container fluid>
		<!--<StatCalendar></StatCalendar>-->
		<StatTables></StatTables>
	</v-container>
</template>

<script lang="ts">
	import Vue from 'vue';
	import { Component } from 'vue-property-decorator';
	import StatTables from '@/stats/StatTables.vue';
	import StatCalendar from '@/stats/StatCalendar.vue';

	@Component({
		components: {
			StatTables,
			StatCalendar,
		},
	})
	export default class Stats extends Vue {
    }
</script>
