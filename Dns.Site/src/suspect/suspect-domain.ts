import { IIpInfo } from '@/home/dns-attack';

interface ISuspectDomain {
	domain: string;
	ips: IIpInfo[];
}

export { ISuspectDomain };

