<template>
	<v-navigation-drawer :disable-resize-watcher="true"
						 :disable-route-watcher="true"
						 right
						 class="elevation-6"
						 width="500"
						 :mini-variant="false"
						 :clipped="true"
						 v-model="isShowingInfo"
						 v-touch="touchModel"
						 touchless
						 app>
		<router-view></router-view>
	</v-navigation-drawer>
</template>

<script lang="ts">
	import Vue from 'vue';
	import { Component, Prop, Watch } from 'vue-property-decorator';
	import ITouchModel from '@/models/v-touch-model';

	@Component({})
	export default class InfoContainer extends Vue {
		@Prop() public isShowingInfo: boolean;
		public touchModel: ITouchModel;
		private changeMe: number = 0;

		public created() {
			this.touchModel = {
				down: () => { this.changeMe++; },
				left: () => { this.changeMe++; },
				right: () => { this.$emit('hideinfo'); },
				up: () => { this.changeMe++; },
			};
		}
	}
</script>

<style scoped>
</style>