import Vue from 'vue';
import Vuetify,
{
	VApp,
	VNavigationDrawer,
	VToolbar,
	VContent,
	VFooter,
	VProgressLinear,
	VList,
	VListItem,
	VListItemAction,
	VListItemContent,
	VListItemTitle,
	VIcon,
	VDivider,
	VSubheader,
	VToolbarTitle,
	VSpacer,
	VContainer,
	VLayout,
	VFlex,
	VAlert,
	VDataTable,
	VCheckbox,
	VCardTitle,
	VTextField,
	VSwitch,
	VExpansionPanel,
	VExpansionPanels,
	VExpansionPanelContent,
	VSlideYTransition,
	VBtn,
	VCard,
	VCardActions,
	VCardText,
	VMenu,
	VDatePicker,
	VForm,
	VSnackbar,
	VProgressCircular,
	VDialog,
	VSelect,
	VTextarea,
	VAppBar,
	VAppBarNavIcon,
} from 'vuetify/lib';
import { Touch, Ripple } from 'vuetify/lib/directives';
import colors from 'vuetify/lib/util/colors';
import ru from 'vuetify/src/locale/ru';

Vue.use(Vuetify, {
	components: {
		VApp,
		VNavigationDrawer,
		VToolbar,
		VContent,
		VFooter,
		VProgressLinear,
		VIcon,
		VList,
		VListItem,
		VListItemAction,
		VListItemContent,
		VListItemTitle,
		VDivider,
		VSubheader,
		VToolbarTitle,
		VSpacer,
		VContainer,
		VLayout,
		VFlex,
		VAlert,
		VDataTable,
		VCheckbox,
		VCardTitle,
		VTextField,
		VSwitch,
		VExpansionPanel,
		VExpansionPanels,
		VExpansionPanelContent,
		VSlideYTransition,
		VBtn,
		VCard,
		VCardActions,
		VCardText,
		VMenu,
		VDatePicker,
		VForm,
		VSnackbar,
		VProgressCircular,
		VDialog,
		VSelect,
		VTextarea,
		VAppBar,
		VAppBarNavIcon,
	},
	directives: {
		Touch,
		Ripple
	}
})

const opts = {
	lang: {
		locales: { ru },
		current: 'ru'
	},
	icons: {
		iconfont: 'fa4',
	},
	theme: {
		dark: true,
		themes: {
			light: {
				primary: colors.purple,
				secondary: colors.grey.darken1,
				accent: colors.shades.black,
				error: colors.red.accent3,
			},
			dark: {
				primary: colors.blue.lighten3,
			},
		},
	},
};

export default new Vuetify(opts);