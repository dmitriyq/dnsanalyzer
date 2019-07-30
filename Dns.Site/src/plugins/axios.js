import Axios from 'axios';

Axios.interceptors.response.use((response) => {
	return response;
}, function (error) {
	if (error.response.status === 302) {
		location.href = location.origin + '/';
		return;
	}
	return Promise.reject(error);
});