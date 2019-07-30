import { IIpInfo } from '@/models/dns-attack';

interface ISuspectDomain {
	domain: string;
	ips: IIpInfo[];
}

export { ISuspectDomain };
