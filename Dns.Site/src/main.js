import 'promise-polyfill/src/polyfill';
import 'whatwg-fetch';
import '@babel/polyfill';
import 'font-awesome/css/font-awesome.css';
import './app.scss';
import 'es6-shim';
import './plugins/axios';
import 'vuetify/src/stylus/app.styl';
import Vue from 'vue';
import './plugins/vuetify';
import App from './App.vue';
import router from './router';
var store = {
    user: {
        canChangePass: false,
        name: null,
        changePassUrl: null,
        logoutUrl: null,
        isDnsAdmin: false,
    },
};
var app = new Vue({
    el: '#app',
    render: function (h) { return h(App); },
    router: router,
    data: store,
});
//# sourceMappingURL=main.js.map