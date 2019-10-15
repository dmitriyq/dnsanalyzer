export default interface IUser {
	name: string | null;
	canChangePass: boolean;
	logoutUrl: string | null;
	changePassUrl: string | null;
	isDnsAdmin: boolean;
}
