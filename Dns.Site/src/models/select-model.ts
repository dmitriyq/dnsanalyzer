export default interface ISelectModel<T> {
    text: string;
    value: T;
}

interface IPagination {
	page: number;
	rowsPerPage: number;
	totalItems: number;
}

export { IPagination };

