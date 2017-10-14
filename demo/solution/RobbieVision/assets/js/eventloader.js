
var GET = (function () {
    var queryStringParameters = {};
    var regEx = /[?&]([^=&]+)(=?)([^&]*)/g;
    while (section = regEx.exec(location.search)) {
        queryStringParameters[decodeURIComponent(section[1])] = (section[2] === "=" ? decodeURIComponent(section[3]) : true);
    }
    return queryStringParameters;
})();

var sessionId = GET['sessionId'];
var jsonPath = "/DataStorage/".concat(sessionId, ".json");
var imagePath = "/DataStorage/".concat(sessionId, ".jpg");

function loadEvents(file, callback) {
    var rawFile = new XMLHttpRequest();
    rawFile.overrideMimeType("application/json");
    rawFile.open("GET", file, true);
    rawFile.onreadystatechange = function() {
        if (rawFile.readyState === 4 && rawFile.status == "200") {
            callback(rawFile.responseText);
        }
    }
    rawFile.send(null);
}

setInterval(function() {
        loadEvents(jsonPath,
            function(response) {

                var eventLog = "";

                var data = JSON.parse(response);
                for (var index = data.length - 1; index >= 0; index--) {
                    var event = data[index];
                    eventLog += event.TimeStamp + " :: [" + event.Sense + "] " + event.Message + "<br />";
                }

                document.getElementById("events").innerHTML = eventLog;
                document.getElementById("capture").src = imagePath.concat("?", new Date().getTime());
            });
    },
    500);