export default interface IDataTableHeaders {
	text: string;
	value: string;
	align?: 'left' | 'center' | 'right';
	sortable?: boolean;
	class?: string[] | string;
	width?: string | number;
	filter?: (value: any, search: string, item: any) => boolean;
	filterable?: boolean;
	sort?: (a: any, b: any) => number;

	selected?: boolean;
}
