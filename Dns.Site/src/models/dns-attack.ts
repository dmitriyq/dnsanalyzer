export default interface IDnsAttackGroup {
	id: number;
	whiteDomain: string;
	blackDomain: string;
	status: number;
	summary: IAttackSummary[];
}

interface IAttackSummary {
	status: number;
	count: number;
}

interface IDnsNote {
	id: number;
	attackId: number;
	create: Date;
	text: string | null;
}

interface IIpInfo {
	ip: string;
	subnet: string | null;
	company: string | null;
	country: string | null;
}

interface IDomainInfo {
	domain: string;
	company: string | null;
	registrant: string | null;
	dateCreate: Date | null;
	dateUntil: Date | null;

	nameServers: string[];
}

interface IHistory {
	id: number;
	create: Date;
	prevStatus: number;
	currentStatus: number;
}

interface IAttackInfo {
	id: number;
	ip: string;
	ipBlocked: boolean;
	subnetBlocked: string | null;
	status: number;
	ipInfo: IIpInfo;
	histories: IHistory[];
}

interface IDnsAttackInfo {
	id: number;
	status: number;
	whiteDomain: string;
	blackDomain: string;
	begin: Date;
	close: Date | null;

	attacks: IAttackInfo[];
	notes: IDnsNote[];
	histories: IHistory[];
	blackDomainInfo: IDomainInfo;
	whiteDomainInfo: IDomainInfo;
}
export { IDnsNote, IIpInfo, IDomainInfo, IDnsAttackInfo, IAttackInfo };
