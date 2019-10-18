<template>
	<v-form v-model="formValid" ref="notifyForm">
		<v-container fluid>
			<v-row>
				<v-col cols="12">
					<p class="text-center subheading">Настройки уведомлений</p>
				</v-col>
			</v-row>
			<v-row v-for="item in notifies" :key="item.id">
				<v-col sm="12" md="6">
					<v-row>
						<v-col cols="8">
							<v-text-field v-model="item.value"
										  placeholder="E-mail или телефон"
										  :rules="noifyRules"
										  solo>
							</v-text-field>
						</v-col>
						<v-col cols="4">
							<v-btn text outlined color="error" icon left @click="delRow(item.id)">
								<v-icon>delete</v-icon>
							</v-btn>
						</v-col>
					</v-row>
				</v-col>
			</v-row>
			<v-row>
				<v-col sm="12" md="6">
					<v-row>
						<v-col cols="8">
							<v-btn text outlined color="primary" icon @click="addRow">
								<v-icon>add</v-icon>
							</v-btn>
						</v-col>
						<v-col cols="4">
							<v-btn text outlined color="primary" @click="saveNotify">
								Сохранить
							</v-btn>
						</v-col>
					</v-row>
				</v-col>
			</v-row>
		</v-container>
		<v-snackbar v-model="snackbar"
					top
					multi-line
					color="success"
					:timeout="2000">
			<p class="title text-center">Настройки уведомлений сохранены.</p>
		</v-snackbar>
	</v-form>
</template>

<script lang="ts">
    import Vue from 'vue';
    import { Component } from 'vue-property-decorator';
    import Axios from 'axios';

    @Component
    export default class Notification extends Vue {
        public formValid: boolean = false;
        public notifies: Array<{ value: string, id: number }> = [];

        public noifyRules: Array<(s: string) => string | boolean> = [
			(v) => {
                if (!!!v) {
                    return true;
                } else {
					const trimmedVal = v.trim();
					const isValidPhone = /^((\+7)|(8))\d{10}$/gm.test(trimmedVal);
					const isValidEmail = /^[^\s@]+@[^\s@]+\.[^\s@]+$/gm.test(trimmedVal);
					if (isValidEmail || isValidPhone) {
						return true;
					} else {
						return 'Телефон в формате (+7|8)xxxxxxxxxx или email формата example@example.example';
					}
                }
            },
        ];
        public snackbar: boolean = false;

        public addRow() {
            const maxId = Math.max(...this.notifies.map((n) => n.id));
            this.notifies.push({ value: '', id: maxId + 1 });
        }

        public delRow(id: number) {
            const rowId = this.notifies.findIndex((val) => val.id === id);
            if (rowId !== -1) {
                this.notifies.splice(rowId, 1);
            }
            if (this.notifies.length < 1) {
                this.notifies.push({ value: '', id: 0 });
            }
        }

        public saveNotify() {
			if ((this.$refs.notifyForm as any).validate()) {
				const data = this.notifies.map((val) => val.value);
				Axios.post('/api/account/usernotifications', data);
				this.snackbar = true;
            }
        }

        public created() {
            Axios.get('/api/account/usernotifications')
                .then((res) => {
                    this.notifies = (res.data as string[]).map((val, indx) => {
                        return { value: val, id: indx };
                    });
                    if (this.notifies.length < 1) {
                        this.notifies.push({ value: '', id: 0 });
                    }
                });
        }
    }
</script>