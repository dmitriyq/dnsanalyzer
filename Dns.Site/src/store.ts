import Vue from 'vue';
import Vuex from 'vuex';
import IUser from '@/models/user';

Vue.use(Vuex);

export default new Vuex.Store({
	state: {
		user: {
			canChangePass: false,
			changePassUrl: null,
			isDnsAdmin: false,
			logoutUrl: null,
			name: null,
		} as IUser,
		sideDialogOpened: false,
		darkMode: false,
	},

	mutations: {
		setUser(state, user: IUser): void {
			state.user = user;
		},
		setDarkMode(state, val: boolean): void {
			state.darkMode = val;
		},
		setSideDialogState(state, val: boolean): void {
			state.sideDialogOpened = val;
		},
	},

	actions: {
		updateUser(context, user: IUser): void {
			context.commit('setUser', user);
		},
		updateDarkMode(context, mode: boolean): void {
			localStorage.setItem('isDarkMode', String(mode));
			context.commit('setDarkMode', mode);
		},
		updateSideDialogState(context, mode: boolean): void {
			context.commit('setSideDialogState', mode);
		},
	},

	getters: {
		isAdmin(state): boolean {
			return state.user.isDnsAdmin;
		},
	},
});
