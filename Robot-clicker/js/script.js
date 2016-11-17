var search = [
  "Найти сертефикат соответсвия"
];
var HashSet = function () {
    var set = {};
    this.add = function (key) {
        set[key] = true;
    };
    this.remove = function (key) {
        delete set[key];
    };
    this.contains = function (key) {
        return set.hasOwnProperty(key);
    };
};
var set = new HashSet();

function findOrders(string) {
    var matches = document.evaluate("//span[contains(., '" + string + "')]", document.documentElement, null,
  XPathResult.ORDERED_NODE_SNAPSHOT_TYPE, null);
    var orders = [];
    for (var i = 0; i < matches.snapshotLength; i++) {
        var el = matches.snapshotItem(i);
        while ((el = el.parentElement) && !el.classList.contains("workorder"));
        orders.push(el);
    }
    return orders;
}

function wait(ms) {
    var start = new Date().getTime();
    var end = start;
    while (end < start + ms) {
        end = new Date().getTime();
    }
}

for (var j = 0; j < search.length; j++) {
    var orders = findOrders(search[j]);
    for (var i = 0; i < orders.length; i++) {
        var order = orders[i];
        if (set.contains(order.id)) {
            continue;
        }
        set.add(order.id);
        order.getElementsByClassName("wo-accept")[0].dispatchEvent(new MouseEvent("click", {
            "view": window,
            "bubbles": true,
            "cancelable": false
        }));

        order.getElementsByClassName("cf-ok")[0].dispatchEvent(new MouseEvent("click", {
            "view": window,
            "bubbles": true,
            "cancelable": false
        }));

        order.getElementsByTagName("textarea")[0].value = "Привет! У меня есть вопрос!";
    }
}
