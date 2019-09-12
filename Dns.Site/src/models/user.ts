export default class User {
	public name: string | null;
	public canChangePass: boolean;
	public logoutUrl: string | null;
	public changePassUrl: string | null;
	public isDnsAdmin: boolean;
}
