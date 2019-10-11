<template>
	<v-app>
		<Navigation :navBarState="navigationState"></Navigation>
		<v-app-bar class="primary white--text" fixed app :clipped-left="true">
			<v-app-bar-nav-icon class="white--text" @click.stop="navigationState = !navigationState"></v-app-bar-nav-icon>
			<v-toolbar-title>DNS анализатор</v-toolbar-title>
		</v-app-bar>
		<v-content>
			<router-view></router-view>
		</v-content>
		<v-footer :fixed="false" app style="justify-content:center">
		</v-footer>
	</v-app>
</template>
<script lang="ts">

	import Vue from 'vue';
	import { Component } from 'vue-property-decorator';
	import Navigation from '@/Navigation.vue';
	@Component({
		components: {
			Navigation,
		},
	})
	export default class AppComponent extends Vue {
		public navigationState: boolean = true;
	}
</script>