export default interface IReestrRecord {
	idRkn: number | null;
	created: Date | null;
	domain: string | null;
	urlRkn: string | null;
	ip: string | null;
	subnet: string | boolean | null;

	idMinjust: number | null;
	urlMinjust: string | null;
}
