import 'promise-polyfill/src/polyfill';
import 'whatwg-fetch';

import '@babel/polyfill';
import 'font-awesome/css/font-awesome.css';
import './app.scss';
import 'es6-shim';
import './plugins/axios';

import Vue from 'vue';
import 'vuetify/dist/vuetify.min.css';
import vuetify from '@/plugins/vuetify';
import App from './App.vue';

import router from './router';
import Store from '@/models/store';

const store: Store = {
	user: {
		canChangePass: false,
		name: null,
		changePassUrl: null,
		logoutUrl: null,
		isDnsAdmin: false,
	},
};

const app = new Vue({
	el: '#app',
	render: (h) => h(App),
	router,
	vuetify,
	data: store,
});
