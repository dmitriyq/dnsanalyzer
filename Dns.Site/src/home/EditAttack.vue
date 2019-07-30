<template>
	<div style="width:100%;">
		<v-dialog v-model="dialog" lazy persistent max-width="700px" style="float:right;">
			<v-btn slot="activator" color="info">{{ buttonText }}</v-btn>
			<v-form v-model="formValid" ref="editStatusForm">
				<v-card>
					<v-card-title>
						<span class="headline">{{ dialogTitle }}</span>
					</v-card-title>
					<v-card-text>
						<v-container grid-list-md>
							<v-layout wrap align-center justify-space-between row fill-height>
								<v-flex xs12 v-if="isMultiply" v-for="att in attacks" :key="att.id">
									<p class="mb-0">
										<span class="subheading font-weight-bold">
											Текущий статус {{ att.whiteDomain }} - {{ att.blackDomain }}:
										</span>
										<span class="subheading" v-text="getSelectStatus(att.status).text"></span>
									</p>
								</v-flex>
								<v-flex xs12 sm4 v-if="!isMultiply">
									<span class="subheading font-weight-bold">Текущий статус:</span>
								</v-flex>
								<v-flex xs12 sm8 v-if="!isMultiply">
									<span class="subheading" v-text="getSelectStatus(attackObj.status).text"></span>
								</v-flex>
								<v-flex xs12 sm4 v-if="isStatusDlg">
									<span class="subheading font-weight-bold">Изменить на:</span>
								</v-flex>
								<v-flex xs12 sm8 v-if="isStatusDlg">
									<v-select :items="statusList"
											  v-model="selectedStatus"
											  required
											  :rules="statusRules"
											  label="Выберите статус"></v-select>
								</v-flex>
								<v-flex xs12 sm4>
									<span class="subheading font-weight-bold">Комментарий:</span>
								</v-flex>
								<v-flex xs12 sm8>
									<v-textarea solo
												label="Введите комментарий"
												auto-grow
												box
												:rules="commentRules"
												v-model="comment">
									</v-textarea>
								</v-flex>
							</v-layout>
						</v-container>
					</v-card-text>
					<v-card-actions>
						<v-spacer></v-spacer>
						<v-btn color="blue darken-1" flat @click="dialog = false">Закрыть</v-btn>
						<v-btn color="blue darken-1" flat @click="onSubmit">Сохранить</v-btn>
					</v-card-actions>
				</v-card>
			</v-form>
		</v-dialog>
	</div>
</template>

<script lang="ts">
	import Vue from 'vue';
	import { Component, Prop, Watch } from 'vue-property-decorator';
	import IDnsAttack, { IDnsAttackInfo } from '@/models/dns-attack';
	import SelectModel from '@/models/select-model';
	import Utils from '@/utils/Utils';
	import { EventBus } from '@/utils/event-bus';
	import ISelectModel from '@/models/select-model';
	import Axios from 'axios';

	@Component({})
	export default class EditAttack extends Vue {
		@Prop() public attackFull?: IDnsAttackInfo;
		@Prop() public attacks?: IDnsAttack[];
		@Prop() public isEditStatus?: boolean;

		public dialog: boolean = false;
		public statuses: Array<ISelectModel<number>> = [];
		public attackObj: IDnsAttackInfo;

		public selectedStatus: number | null = null;
		public comment: string | null = '';
		public formValid: boolean = false;

		public isMultiply: boolean = false;

		get isStatusDlg(): boolean {
			return !!this.isEditStatus;
		}
		get statusList(): Array<SelectModel<number>> {
			if (this.isMultiply) {
				return this.statuses;
			} else {
				return this.statuses
					.filter((v) => v.value !== this.attackObj.status)
					.filter((v) => v.value !== 0);
			}
		}
		get dialogTitle(): string {
			if (this.isMultiply) {
				return 'Изменение статуса атак';
			} else {
				return this.isStatusDlg ? 'Изменение статуса атаки' : 'Добавление комментария';
			}
		}
		get buttonText(): string {
			return this.isStatusDlg ? 'Изменить' : 'Добавить';
		}

		public statusRules: Array<(s: string) => boolean | string> = [
			(s) => !!s || 'Необходимо указать статус',
		];
		public commentRules: Array<(s: string) => boolean | string> = [
			(c) => {
				if (this.isStatusDlg) { return true; }
				else {
					return !!c ? true : 'Необходимо указать коменнтарий';
				}
			},
		];

		public getSelectStatus(status: number): ISelectModel<number> {
			return Utils.getSelectStatus(status, this.statuses);
		}
		public created() {
			if (this.attackFull) {
				this.attackObj = this.attackFull;
				this.isMultiply = false;
			} else if (this.attacks) {
				this.isMultiply = true;
			}
		}

		public beforeMount() {
			Axios.get(`/api/attack/groupStatuses`)
				.then((resp) => this.statuses = resp.data as Array<ISelectModel<number>>);
		}

		private onSubmit() {
			if ((this.$refs.editStatusForm as any).validate()) {
				if (this.isMultiply && this.attacks) {
					Axios.post('/api/attack/editmany', {
						ids: this.attacks.map((a) => a.id),
						status: this.selectedStatus,
						comment: this.comment,
					});
					EventBus.$emit('clearselectedattacks');
				}
				else {
					if (this.isStatusDlg) {
						Axios.post('/api/attack/edit', {
							id: this.attackObj.id,
							status: this.selectedStatus,
							comment: this.comment,
						});
					} else {
						Axios.post('/api/attack/note', {
							id: this.attackObj.id,
							comment: this.comment,
						});
					}
				}
				(this.$refs.editStatusForm as any).reset();
				this.dialog = false;
				this.$router.push('/');
			}
		}
	}
</script>

<style scoped>

</style>