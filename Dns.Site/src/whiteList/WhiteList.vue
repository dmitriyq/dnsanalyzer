<template>
	<v-container fluid>
		<v-layout row wrap justify-start>
			<v-flex xs12 class="elevation-1">
				<v-toolbar dense extended prominent flat color="white">
					<v-toolbar-title class="hidden-xs-only">Белый список</v-toolbar-title>
					<v-spacer></v-spacer>
					<v-text-field v-model="tableSearch"
								  append-icon="search"
								  label="Быстрый поиск"
								  single-line
								  hide-details></v-text-field>
					<v-spacer></v-spacer>
					<v-dialog v-model="dialog" max-width="500px" persistent scrollable v-if="isDnsAdmin">
						<template v-slot:activator="{ on }">
							<v-btn class="hidden-xs-only" color="red" :disabled="selectedDomains.length <= 1"
								   dark v-on="on" @click="dialogType = 'DeleteBatch'">Удалить выбранные</v-btn>
							<v-btn class="hidden-xs-only" color="green"
								   v-text="selectedDomains.length > 0 ? 'Отменить выделение' : 'Выбрать все'"
								   dark @click="selectAll"></v-btn>
							<v-btn color="blue" dark v-on="on" @click="dialogType = 'Add'">Добавить домен</v-btn>
							<v-btn class="hidden-xs-only" color="blue" dark v-on="on" @click="dialogType = 'Import'">Импорт из файла</v-btn>
						</template>
						<v-card>
							<v-card-title>
								<span class="headline">{{ dialogType | titleName }}</span>
							</v-card-title>
							<v-card-text>
								<v-container>
									<v-layout>
										<v-flex xs12 v-if="dialogType !== 'Import'">
											<v-text-field ref="domainInput"
														  v-if="dialogType !== 'DeleteBatch'"
														  :disabled="dialogType === 'Delete'"
														  :rules="requiredRule"
														  v-model="editedItem.domain"
														  v-on:keyup.enter="save"
														  maxlength="255"
														  label="Доменное имя"></v-text-field>
											<v-textarea v-else
														v-model="selectedDomainNames"
														auto-grow
														disabled
														:rows="selectedDomains.length"
														label="Доменные имена">
											</v-textarea>
										</v-flex>
										<v-flex xs12 v-else>
											<p><span>Если файл .csv или .txt, то нужна кодировка Windows-1251. При импорте будут добавлены только новые домены</span></p>
											<div class="text-xs-center">
												<v-progress-circular indeterminate v-if="isUploading"
																	 color="primary"></v-progress-circular>
											</div>
											<p>{{ importMsg }}</p>
										</v-flex>
									</v-layout>
								</v-container>
							</v-card-text>
							<v-card-actions>
								<v-spacer></v-spacer>
								<v-btn color="primary" flat @click="cancel">Отмена</v-btn>
								<label class="button" @click="clearInputOnOpen" v-if="dialogType === 'Import'">
									<input type="file"
										   accept=".csv,.txt,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
										   style="display:none" id="uploadFile" @input="uploadFile">
									<v-btn color="blue darken-1" class="m-0" flat @click.native="showUpload">Указать файл</v-btn>
								</label>
								<v-btn v-else :color="(dialogType === 'Delete' || dialogType === 'DeleteBatch') ? 'error' : 'primary'"
									   flat @click="save"
									   v-text="(dialogType === 'Delete' || dialogType === 'DeleteBatch') ? 'Удалить' : 'Сохранить'"></v-btn>
							</v-card-actions>
						</v-card>
					</v-dialog>
				</v-toolbar>
				<v-data-table :headers="tableHeaders"
							  ref="whitelistTable"
							  :pagination.sync="pagination"
							  :rows-per-page-items="rowsPerPage"
							  :search="tableSearch"
							  v-model="selectedDomains"
							  item-key="id"
							  :items="domains">
					<template v-slot:items="props">
						<td class="text-xs-left pr-0 hidden-xs-only" v-if="isDnsAdmin">
							<v-checkbox v-model="props.selected" primary hide-details></v-checkbox>
						</td>
						<td class="text-xs-left">{{props.item.id}}</td>
						<td class="text-xs-center">{{props.item.domain}}</td>
						<td class="justify-center layout px-0" v-if="isDnsAdmin">
							<v-icon small class="mr-2" @click="editItem(props.item)">edit</v-icon>
							<v-icon small @click="deleteItem(props.item)">delete</v-icon>
						</td>
					</template>
				</v-data-table>
			</v-flex>
		</v-layout>
	</v-container>
</template>

<script lang="ts">
	import Vue from 'vue';
	import { Component } from 'vue-property-decorator';
	import Axios from 'axios';
	import IDataTableHeaders from '@/models/data-table';
	import Utils from '@/utils/Utils';
	import IWhiteDomain from './white-domain';

	@Component({
		filters: {
			titleName(value: 'Add' | 'Import' | 'Delete' | 'DeleteBatch' | 'Edit') {
				let text = '';
				switch (value) {
					case 'Add': text = 'Добавление домена'; break;
					case 'Delete': text = 'Удаление домена'; break;
					case 'DeleteBatch': text = 'Удаление доменов'; break;
					case 'Edit': text = 'Редактирование домена'; break;
					case 'Import': text = 'Импорт из файла'; break;
				}
				return text;
			},
		},
	})
	export default class WhiteList extends Vue {
		public dialog: boolean = false;
		public dialogType: 'Add' | 'Import' | 'Delete' | 'DeleteBatch' | 'Edit' = 'Add';
		public tableHeaders: IDataTableHeaders[] = [
			{ text: '', value: '', sortable: false, align: 'left', width: '20px', class: 'hidden-xs-only' },
			{ text: 'ID', value: 'id', align: 'left', width: '50px' },
			{ text: 'Домен', value: 'domain', align: 'center', width: '250px' },
			{ text: 'Действия', value: 'id', align: 'center', width: '100px', sortable: false },
		];
		public pagination = {
			rowsPerPage: 10,
			sortBy: 'id',
		};
		public rowsPerPage = [10, 25, 50, 100, { text: '$vuetify.dataIterator.rowsPerPageAll', value: -1 }];
		public tableSearch: string = '';
		public importMsg: string = '';
		public isUploading: boolean = false;
		public selectedDomains: IWhiteDomain[] = [];
		public get selectedDomainNames(): string {
			return this.selectedDomains.map((v) => v.domain).join('\n');
		}
		public get isDnsAdmin(): boolean {
			return Utils.getUser(this).isDnsAdmin;
		}
		public domains: IWhiteDomain[] = [];
		public defaultItem: IWhiteDomain = {
			id: 0,
			dateAdded: new Date(),
			domain: '',
		};
		public editedItem: IWhiteDomain = {
			id: 0,
			dateAdded: new Date(),
			domain: '',
		};
		public requiredRule: Array<(s: string) => boolean | string> = [
			(s) => !!s || 'Обязательное поле',
			(s) => (s.includes('.') && s.split('.').length >= 2) || 'Не корректное доменное имя',
		];
		public created() {
			if (!this.isDnsAdmin) {
				this.tableHeaders.splice(0, 1);
				this.tableHeaders.splice(2, 1);
			}
			Axios.get(`/api/whitelist/`)
				.then((resp) => {
					const data = (resp.data as IWhiteDomain[]);
					this.domains = data;
				});
		}
		private selectAll(): void {
			if (this.selectedDomains.length > 0) {
				this.selectedDomains = [];
			} else {
				const items = ((this.$refs.whitelistTable as any).filteredItems) as IWhiteDomain[];
				this.selectedDomains = items;
			}
		}
		private editItem(item: IWhiteDomain): void {
			this.dialog = true;
			this.dialogType = 'Edit';
			this.editedItem = Object.assign({}, item);
		}
		private deleteItem(item: IWhiteDomain): void {
			this.dialog = true;
			this.dialogType = 'Delete';
			this.editedItem = Object.assign({}, item);
		}
		private uploadFile(parms: any): void {
			const fd = new FormData();
			fd.append('file', parms.target.files[0]);
			this.isUploading = true;
			Axios.post('api/whitelist/upload', fd)
				.then((resp) => {
					const domains = resp.data.success as string[];
					const invalid = resp.data.error as string[];
					Axios.post('/api/whitelist/batch', domains)
						.then((batch) => {
							const mdls = batch.data as IWhiteDomain[];
							this.importMsg = `Кол-во добавленных доменов: ${mdls.length}\n Не добавлены домены: ${invalid.join(',')}`;
							this.domains.push(...mdls);
							this.isUploading = false;
						});
				});
		}
		private cancel(): void {
			this.dialog = false;
			this.editedItem = Object.assign({}, this.defaultItem);
			this.importMsg = '';
		}
		private save(): void {
			switch (this.dialogType) {
				case 'Add':
				case 'Edit':
					const isValid = (this.$refs.domainInput as any).valid as boolean;
					if (!isValid) {
						return;
					} else {
						if (this.dialogType === 'Add') {
							Axios.post('/api/whitelist/', { domain: this.editedItem.domain })
								.then((resp) => {
									const addedDomain = resp.data as IWhiteDomain;
									this.domains.push(addedDomain);
									this.cancel();
								}).catch((err) => (this.$refs.domainInput as any).errorMessages.push(err.response.data.msg));
						} else {
							Axios.put(`/api/whitelist/${this.editedItem.id}`, this.editedItem)
								.then((resp) => {
									const domain = this.domains.find((v) => v.id === this.editedItem.id);
									if (domain) {
										domain.domain = this.editedItem.domain;
										this.cancel();
									}
								}).catch((err) => (this.$refs.domainInput as any).errorMessages.push(err.response.data.msg));
						}
					}
					break;
				case 'Delete':
					Axios.delete(`/api/whitelist/${this.editedItem.id}`)
						.then((resp) => {
							const ind = this.domains.findIndex((v) => v.id === this.editedItem.id);
							if (ind !== -1) {
								this.domains.splice(ind, 1);
								this.cancel();
							}
						}).catch((err) => (this.$refs.domainInput as any).errorMessages.push(err.response.data.msg));
					break;
				case 'DeleteBatch':
					const ids = this.selectedDomains.map((v) => v.id);
					Axios.delete('/api/whitelist', { data: ids })
						.then((resp) => {
							ids.forEach((id) => {
								const index = this.domains.findIndex((v) => v.id === id);
								if (index !== -1) {
									this.domains.splice(index, 1);
								}
							});
							this.cancel();
						}).catch((err) => (this.$refs.domainInput as any).errorMessages.push(err.response.data.msg));
					break;
			}
		}
		private showUpload(): void {
			const fileUploaderElem = document.getElementById('uploadFile');
			if (fileUploaderElem) {
				fileUploaderElem.click();
			}
		}
		private clearInputOnOpen(): void {
			const fileUploaderElem = document.getElementById('uploadFile');
			if (fileUploaderElem) {
				(fileUploaderElem as HTMLInputElement).value = '';
			}
		}
    }
</script>