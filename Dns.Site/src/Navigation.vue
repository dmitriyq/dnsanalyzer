<template>
	<v-navigation-drawer fixed
						 :clipped="true"
						 v-model="navState"
						 app>
		<v-list dense>
			<v-list-item router
						 :to="item.to"
						 :key="i"
						 v-for="(item, i) in items"
						 exact>
				<v-list-item-action>
					<v-icon v-html="item.icon"></v-icon>
				</v-list-item-action>
				<v-list-item-content>
					<v-list-item-title v-text="item.title"></v-list-item-title>
				</v-list-item-content>
			</v-list-item>
			<v-divider></v-divider>
			<v-subheader inset v-if="isLogged">Учетная запись</v-subheader>
			<v-list-item>
				<v-list-item-content>
					<v-list-item-title>
						<v-switch dense hide-details class="mr-3" v-model="isDarkMode" label="Темная тема" @change="changeTheme"></v-switch>
					</v-list-item-title>
				</v-list-item-content>
			</v-list-item>
			<v-list-item>
				<v-list-item-action>
					<v-icon>account_box</v-icon>
				</v-list-item-action>
				<v-list-item-content>
					<v-list-item-title v-text="user.name"></v-list-item-title>
				</v-list-item-content>
			</v-list-item>
			<v-list-item :href="changePassUrl()" v-if="user.canChangePass">
				<v-list-item-action>
					<v-icon>vpn_key</v-icon>
				</v-list-item-action>
				<v-list-item-content>
					<v-list-item-title>Сменить пароль</v-list-item-title>
				</v-list-item-content>
			</v-list-item>
			<v-list-item :href="logoutUrl()">
				<v-list-item-action>
					<v-icon>exit_to_app</v-icon>
				</v-list-item-action>
				<v-list-item-content>
					<v-list-item-title>Выход</v-list-item-title>
				</v-list-item-content>
			</v-list-item>
		</v-list>
	</v-navigation-drawer>
</template>
<script lang="ts">

	import Vue from 'vue';
	import { Component, Prop, PropSync } from 'vue-property-decorator';
	import Axios from 'axios';
	import IUser from '@/models/user';
	@Component
	export default class Navigation extends Vue {
		@PropSync('navBarState', { type: Boolean, required: true }) public navState: boolean;
		public isDarkMode: boolean = false;
		public items = [
			{ icon: 'home', title: 'Главная', to: '/' },
			{ icon: 'filter_9_plus', title: 'Подозрительные домены', to: '/suspect' },
			{ icon: 'grade', title: 'Белый список', to: '/whitelist' },
			// { icon: 'playlist_add_check', title: 'Проверка домена', to: '/domainCheck' },
			{ icon: 'search', title: 'Поиск в Реестре', to: '/reestrCheck' },
			{ icon: 'bar_chart', title: 'Статистика', to: '/stat' },
			{ icon: 'local_hospital', title: 'Статус системы', to: '/health'},
			{ icon: 'save', title: 'Выгрузки', to: '/export' },
			{ icon: 'email', title: 'Уведомления', to: '/notification' },
		];

		get user(): IUser {
			return this.$store.state.user as IUser;
		}

        public isLogged(): boolean {
            return (this.user && this.user.name && (this.user.name.length > 0)) || false;
        }

        public changePassUrl(): string {
            return this.user.changePassUrl + encodeURI(window.location.href);
        }
        public logoutUrl(): string {
            return this.user.logoutUrl + encodeURI(window.location.href);
        }

		public changeTheme(): void {
			this.$vuetify.theme.dark = this.isDarkMode;
			this.$store.dispatch('updateDarkMode', this.isDarkMode);
		}

		public created() {

			this.isDarkMode = this.$store.state.darkMode as boolean;

			this.changeTheme();

			if (!this.isLogged()) {
				if (process.env.NODE_ENV === 'development') {
					this.$store.dispatch('updateUser', {
						canChangePass: !!false,
						name: 'Тестович',
						changePassUrl: '',
						logoutUrl: '',
						isDnsAdmin: true,
					} as IUser);
				} else {
					Axios.get('api/account/GetUserInfo')
						.then((res) => res.data)
						.then((data) => {
							this.$store.dispatch('updateUser', {
								canChangePass: !!data.canChangePass,
								name: data.name,
								changePassUrl: data.resetPasswordUrl,
								logoutUrl: data.logoutUrl,
								isDnsAdmin: data.isDnsAdmin,
							} as IUser);
						});
				}

            }
		}
	}
</script>