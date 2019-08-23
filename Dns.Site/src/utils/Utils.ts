import User from '@/models/user';
import Vue from 'vue';
import ISelectModel from '@/models/select-model';

export default class Utils {
	public static getStatusIcon(status: number): string {
		switch (status) {
			case 1: return 'watch_later'; break; // Ожидание проверки
			case 2: return 'warning'; break; // Атака
			case 3: return 'report'; break; // Угроза
			case 4: return 'cloud_queue'; break; // Динамические IP
			case 5: return 'check_circle'; break; // Прекращена
		}
		return '';
	}
	public static getStatusColor(status: number): string {
		switch (status) {
			case 1: return 'grey--text darken-3'; break;
			case 2: return 'red--text darken-1'; break;
			case 3: return 'yellow--text accent-2'; break;
			case 4: return 'grey--text darken-3'; break;
			case 5: return 'green--text accent-4'; break;
		}
		return '';
	}

	public static getStatusIpIcon(status: number): string {
		switch (status) {
			case 1: return 'warning'; break; // пересечение
			case 3: return 'check_circle'; break; // Прекращено
			case 4: return 'watch_later'; break; // Ожидание закрытия
		}
		return '';
	}
	public static getStatusIpColor(status: number): string {
		switch (status) {
			case 1: return 'red--text darken-1'; break;
			case 3: return 'green--text accent-4'; break;
			case 4: return 'grey--text darken-3'; break;
		}
		return '';
	}

	public static getSelectStatus(currentStatus: number, statuses: Array<ISelectModel<number>>): ISelectModel<number> {
		const select = statuses.find((s) => s.value === currentStatus);
		if (select) {
			return select;
		} else {
			return { text: '', value: -1 };
		}
	}

	public static getUser(vue: Vue): User {
		return (vue.$root.$data as any).user as User;
	}
	public static setUser(vue: Vue, user: User) {
		(vue.$root.$data as any).user = user;
	}

	public static deserialize(data: string): any {
		return JSON.parse(data, this.reviveDateTime);
	}

	private static reviveDateTime(key: any, value: any): any {
		if (typeof value === 'string') {
			const ts = Date.parse(value);
			if (!isNaN(ts)) {
				return new Date(ts);
			}
		}

		return value;
	}
}
