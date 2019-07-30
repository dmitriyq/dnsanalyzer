var _this = this;
import * as tslib_1 from "tslib";
import Vue from 'vue';
import Router from 'vue-router';
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
var routes = [
    {
        path: '/',
        component: Home,
        name: 'Home',
        children: [
            {
                path: '?id=:id',
                name: 'DnsAttackInfo',
                component: DnsAttackInfo,
                props: function (route) { return ({ id: route.params.id }); },
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
        beforeEnter: function (to) {
            location.href = to.query.url;
        },
    },
    {
        path: '*',
        redirect: '/',
    },
];
var router = new Router({
    mode: 'history',
    linkActiveClass: 'active',
    routes: routes,
});
router.beforeEach(function (to, from, next) { return tslib_1.__awaiter(_this, void 0, void 0, function () {
    var authResponse;
    return tslib_1.__generator(this, function (_a) {
        switch (_a.label) {
            case 0:
                if (!to.path.startsWith('/auth')) return [3 /*break*/, 1];
                return [2 /*return*/, next()];
            case 1: return [4 /*yield*/, fetch('/api/account/CheckAuth', {
                    method: 'get',
                    headers: new Headers({
                        'Vue-location': encodeURI(window.location.href),
                    }),
                })
                    .then(function (res) { return res; })];
            case 2:
                authResponse = _a.sent();
                if (authResponse.redirected) {
                    return [2 /*return*/, next({ path: '/auth', query: { url: authResponse.url } })];
                }
                else {
                    return [2 /*return*/, next()];
                }
                _a.label = 3;
            case 3: return [2 /*return*/];
        }
    });
}); });
Vue.use(Router);
export default router;
//# sourceMappingURL=router.js.map