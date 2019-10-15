<template>
	<v-list-item>
		<v-list-item-content>
			<v-list-item-title>
				<v-container fluid class="pa-0 pt-2">
					<v-row no-gutters>
						<v-col col="5" class="body-2">
							{{ title }}:
						</v-col>
						<v-col col="7" class="body-1">
							{{ currentStatus.text }}
							<v-icon :class="statusColor" v-text="statusIcon"/>
						</v-col>
					</v-row>
				</v-container>
			</v-list-item-title>
		</v-list-item-content>
	</v-list-item>
</template>

<script lang="ts">

import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import Utils from '@/utils/Utils';
import ISelectModel from '@/models/select-model';
import Axios from 'axios';

@Component
export default class StatusRow extends Vue {
	@Prop() public title: string;
	@Prop() public status: number;
	@Prop() public statType: 'group' | 'ip';

	public statuses: Array<ISelectModel<number>> = [];

	public beforeMount() {
		this.loadStatuses();
	}

	get currentStatus(): ISelectModel<number> {
		return Utils.getSelectStatus(this.status, this.statuses);
	}

	get statusIcon(): string {
		if (this.statType === 'group') {
			return Utils.getStatusIcon(this.status);
		} else if (this.statType === 'ip') {
			return Utils.getStatusIpIcon(this.status);
		} else {
			return '';
		}
	}

	get statusColor(): string {
		if (this.statType === 'group') {
			return Utils.getStatusColor(this.status);
		} else if (this.statType === 'ip') {
			return Utils.getStatusIpColor(this.status);
		} else {
			return '';
		}
	}

	private loadStatuses(): void {
		if (this.statType === 'group') {
			Axios.get(`/api/attack/groupStatuses`)
				.then((resp) => this.statuses = resp.data as Array<ISelectModel<number>>);
		} else if (this.statType === 'ip') {
			Axios.get(`/api/attack/statuses`)
				.then((resp) => this.statuses = resp.data as Array<ISelectModel<number>>);
		}
	}
}
</script>
