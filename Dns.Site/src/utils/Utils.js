var Utils = /** @class */ (function () {
    function Utils() {
    }
    Utils.getStatusIcon = function (status) {
        switch (status) {
            case 1:
                return 'watch_later';
                break; // Ожидание проверки
            case 2:
                return 'warning';
                break; // Атака
            case 3:
                return 'report';
                break; // Угроза
            case 4:
                return 'cloud_queue';
                break; // Динамические IP
            case 5:
                return 'check_circle';
                break; // Прекращена
        }
        return '';
    };
    Utils.getStatusColor = function (status) {
        switch (status) {
            case 1:
                return 'grey--text darken-3';
                break;
            case 2:
                return 'red--text darken-1';
                break;
            case 3:
                return 'yellow--text accent-2';
                break;
            case 4:
                return 'grey--text darken-3';
                break;
            case 5:
                return 'green--text accent-4';
                break;
        }
        return '';
    };
    Utils.getStatusIpIcon = function (status) {
        switch (status) {
            case 1:
                return 'warning';
                break; // пересечение
            case 3:
                return 'check_circle';
                break; // Прекращено
            case 4:
                return 'watch_later';
                break; // Ожидание закрытия
        }
        return '';
    };
    Utils.getStatusIpColor = function (status) {
        switch (status) {
            case 1:
                return 'red--text darken-1';
                break;
            case 3:
                return 'green--text accent-4';
                break;
            case 4:
                return 'grey--text darken-3';
                break;
        }
        return '';
    };
    Utils.getSelectStatus = function (currentStatus, statuses) {
        var select = statuses.find(function (s) { return s.value === currentStatus; });
        if (select) {
            return select;
        }
        else {
            return { text: '', value: -1 };
        }
    };
    Utils.getUser = function (vue) {
        return vue.$root.$data.user;
    };
    Utils.setUser = function (vue, user) {
        vue.$root.$data.user = user;
    };
    Utils.deserialize = function (data) {
        return JSON.parse(data, this.reviveDateTime);
    };
    Utils.reviveDateTime = function (key, value) {
        if (typeof value === 'string') {
            var ts = Date.parse(value);
            if (!isNaN(ts)) {
                return new Date(ts);
            }
        }
        return value;
    };
    return Utils;
}());
export default Utils;
//# sourceMappingURL=Utils.js.map