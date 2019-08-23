import Vue from 'vue';
import Router, { Route } from 'vue-router';

import Home from './home/Home.vue';
import DomainCheck from './domainCheck/DomainCheck.vue';
import Notification from './notification/Notification.vue';
import DnsAttackInfo from './home/DnsAttackInfo.vue';
import ReestrCheck from './reestrCheck/ReestrCheck.vue';
import Stats from './stats/Stats.vue';
import Suspect from './suspect/Suspect.vue';
import Export from './export/Export.vue';
import WhiteList from './whiteList/WhiteList.vue';

Vue.use(Router);

const routes = [
	{
		path: '/',
		component: Home,
		name: 'Home',
		children: [
			{
				path: '?id=:id',
				name: 'DnsAttackInfo',
				component: DnsAttackInfo,
				props: (route: Route) => ({ id: route.params.id }),
			},
		],
	},
	{
		path: '/domainCheck',
		component: DomainCheck,
	},
	{
		path: '/reestrCheck',
		component: ReestrCheck,
	},
	{
		path: '/notification',
		component: Notification,
	},
	{
		path: '/stat',
		component: Stats,
	},
	{
		path: '/suspect',
		component: Suspect,
	},
	{
		path: '/export',
		component: Export,
	},
	{
		path: '/whitelist',
		component: WhiteList,
	},
	{
		path: '/auth',
		name: 'auth',
		beforeEnter(to: Route) {
			location.href = to.query.url as string;
		},
	},

	{
		path: '*',
		redirect: '/',
	},
];

const router = new Router({
	mode: 'history',
	linkActiveClass: 'active',
	routes,
});

router.beforeEach(async (to, from, next) => {
	if (to.path.startsWith('/auth')) {
		return next();
	} else {
		const authResponse = await fetch('/api/account/CheckAuth',
			{
				method: 'get',
				headers: new Headers({
					'Vue-location': encodeURI(window.location.href),
				}),
			})
			.then((res) => res);
		if (authResponse.redirected) {
			return next({ path: '/auth', query: { url: authResponse.url } });
		} else {
			return next();
		}
	}
});

Vue.use(Router);

export default router;
