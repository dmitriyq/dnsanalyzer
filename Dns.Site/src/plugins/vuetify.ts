import Vue from 'vue';
import Vuetify from 'vuetify/lib';
import colors from 'vuetify/lib/util/colors';
import ru from 'vuetify/src/locale/ru';

Vue.use(Vuetify);

const vuetify = new Vuetify({
	icons: {
		iconfont: 'md',
	},
	locale: {
		current: 'ru',
		locales: { ru },
	},
	theme: {
		dark: false,
		themes: {
			light: {
				primary: colors.lightBlue.base,
				secondary: colors.deepPurple.accent1,
				accent: colors.deepOrange.base,
				error: colors.red.base,
				warning: colors.amber.base,
				info: colors.cyan.base,
				success: colors.green.darken1,
			},
			dark: {
				primary: colors.blue.darken2,
				secondary: colors.deepPurple.base,
				accent: colors.amber.darken2,
				error: colors.red.base,
				warning: colors.amber.base,
				info: colors.cyan.base,
				success: colors.green.darken1,
			},
		},
	},
});
export default vuetify;
