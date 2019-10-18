import Utils from '@/utils/Utils';
import IAttackTableFilters from '../home/attack-table-filters';

export default class DnsLocalStorage {
	public static getSavedDateRange(): ({ from: Date, to: Date }) {
		const rangeJson = localStorage.getItem('DNS_DateRange');
		if (!!rangeJson) {
			return Utils.deserialize(rangeJson) as ({ from: Date, to: Date });
		} else {
			const defaultDate = new Date();
			defaultDate.setHours(0, 0, 0, 0);
			const range = {
				from: defaultDate,
				to: defaultDate,
			};
			DnsLocalStorage.setSavedDateRange(range);
			return range;
		}
	}

	public static setSavedDateRange(data: ({ from: Date, to: Date })): void {
		const json = JSON.stringify(data);
		localStorage.setItem('DNS_DateRange', json);
	}

	public static getTableFilters(): IAttackTableFilters {
		const filterJson = localStorage.getItem('DNS_TableFilter');
		if (!!filterJson) {
			return Utils.deserialize(filterJson) as IAttackTableFilters;
		} else {
			const defaultFilters = {
				showDynamic: false,
				showCompleted: false,
			} as IAttackTableFilters;
			DnsLocalStorage.setTableFilters(defaultFilters);
			return defaultFilters;
		}
	}

	public static setTableFilters(data: IAttackTableFilters): void {
		const json = JSON.stringify(data);
		localStorage.setItem('DNS_TableFilter', json);
	}
}
