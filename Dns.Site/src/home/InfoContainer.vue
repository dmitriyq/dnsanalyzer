<template>
	<v-navigation-drawer :disable-resize-watcher="true"
						 :disable-route-watcher="true"
						 right width="500"
						 v-model="isShowingInfo"
						 v-touch="touchModel"
						 touchless app>
		<router-view></router-view>
	</v-navigation-drawer>
</template>

<script lang="ts">
	import Vue from 'vue';
	import { Component, Prop, Watch } from 'vue-property-decorator';
	import ITouchModel from '@/models/v-touch-model';

	@Component({})
	export default class InfoContainer extends Vue {
		public touchModel: ITouchModel;

		public created() {
			this.touchModel = {
				right: () => { this.$emit('hideinfo'); },
			};
		}

		public get isShowingInfo(): boolean {
			return this.$store.state.sideDialogOpened as boolean;
		}
		public set isShowingInfo(val: boolean) {
			this.$store.dispatch('updateSideDialogState', val);
		}
	}
</script>