<template>
	<v-app>
		<v-navigation-drawer fixed
							 :mini-variant="false"
							 :clipped="true"
							 v-model="drawer"
							 app>
			<v-list>
				<v-list-tile router
							 :to="item.to"
							 :key="i"
							 v-for="(item, i) in items"
							 exact>
					<v-list-tile-action>
						<v-icon v-html="item.icon"></v-icon>
					</v-list-tile-action>
					<v-list-tile-content>
						<v-list-tile-title v-text="item.title"></v-list-tile-title>
					</v-list-tile-content>
				</v-list-tile>
				<v-divider></v-divider>
				<v-subheader inset v-if="isLogged">Учетная запись</v-subheader>
				<v-list-tile>
					<v-list-tile-action>
						<v-icon>account_box</v-icon>
					</v-list-tile-action>
					<v-list-tile-content>
						<v-list-tile-title v-text="user.name"></v-list-tile-title>
					</v-list-tile-content>
				</v-list-tile>
				<v-list-tile :href="changePassUrl()" v-if="user.canChangePass">
					<v-list-tile-action>
						<v-icon>vpn_key</v-icon>
					</v-list-tile-action>
					<v-list-tile-content>
						<v-list-tile-title>Сменить пароль</v-list-tile-title>
					</v-list-tile-content>
				</v-list-tile>
				<v-list-tile :href="logoutUrl()">
					<v-list-tile-action>
						<v-icon>exit_to_app</v-icon>
					</v-list-tile-action>
					<v-list-tile-content>
						<v-list-tile-title>Выход</v-list-tile-title>
					</v-list-tile-content>
				</v-list-tile>
			</v-list>
		</v-navigation-drawer>
		<v-toolbar class="blue white--text"
				   fixed
				   app
				   :clipped-left="true">
			<v-toolbar-side-icon class="white--text" @click.stop="drawer = !drawer"></v-toolbar-side-icon>
			<v-toolbar-title>DNS анализатор</v-toolbar-title>
			<v-spacer></v-spacer>
		</v-toolbar>
		<v-content>
			<v-container fill-height fluid style="padding:0;">
				<v-slide-y-transition mode="out-in">
					<v-layout column align-center fill-height>
						<router-view></router-view>
					</v-layout>
				</v-slide-y-transition>
			</v-container>
		</v-content>
		<v-footer :fixed="false" app style="justify-content:center">
			<div style="text-align:center;margin:10px">{{ footerText }}</div>
		</v-footer>
	</v-app>
</template>
<script lang="ts">

	import Vue from 'vue';
	import { Component } from 'vue-property-decorator';
	import Axios from 'axios';
	import User from './models/user';
	import Utils from '@/utils/Utils';
	@Component
	export default class AppComponent extends Vue {
		public drawer = true;
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
		public footerText = `© ${new Date().getFullYear()}`;

		get user(): User {
			return Utils.getUser(this);
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

		public created() {
            if (!this.isLogged()) {
                Axios.get('api/account/GetUserInfo')
                    .then((res) => res.data)
					.then((data) => {
						Utils.setUser(this,
							{ canChangePass: !!data.canChangePass,
                            name: data.name,
                            changePassUrl: data.resetPasswordUrl,
                            logoutUrl: data.logoutUrl,
                            isDnsAdmin: data.isDnsAdmin,
                        });
                    });
            }
		}
	}
</script>
<style scoped>
	.v-btn {
		color: white;
	}
</style>