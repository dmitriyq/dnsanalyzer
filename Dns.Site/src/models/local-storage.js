import Utils from '@/utils/Utils';
var DnsLocalStorage = /** @class */ (function () {
    function DnsLocalStorage() {
    }
    DnsLocalStorage.getSavedDateRange = function () {
        var rangeJson = localStorage.getItem('DNS_DateRange');
        if (!!rangeJson) {
            return Utils.deserialize(rangeJson);
        }
        else {
            var defaultDate = new Date();
            defaultDate.setHours(0, 0, 0, 0);
            var range = {
                from: defaultDate,
                to: defaultDate,
            };
            DnsLocalStorage.setSavedDateRange(range);
            return range;
        }
    };
    DnsLocalStorage.setSavedDateRange = function (data) {
        var json = JSON.stringify(data);
        localStorage.setItem('DNS_DateRange', json);
    };
    DnsLocalStorage.getTableFilters = function () {
        var filterJson = localStorage.getItem('DNS_TableFilter');
        if (!!filterJson) {
            return Utils.deserialize(filterJson);
        }
        else {
            var defaultFilters = {
                showDymanic: false,
                showCompleted: false,
            };
            DnsLocalStorage.setTableFilters(defaultFilters);
            return defaultFilters;
        }
    };
    DnsLocalStorage.setTableFilters = function (data) {
        var json = JSON.stringify(data);
        localStorage.setItem('DNS_TableFilter', json);
    };
    return DnsLocalStorage;
}());
export default DnsLocalStorage;
//# sourceMappingURL=local-storage.js.map