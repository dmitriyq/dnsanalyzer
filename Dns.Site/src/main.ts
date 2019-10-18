import './plugins/axios';

import Vue from 'vue';
import vuetify from '@/plugins/vuetify';
import App from './App.vue';

import router from './router';
import store from './store';

const app = new Vue({
	el: '#app',
	render: (h) => h(App),
	store,
	router,
	vuetify,
	data: store,
});
