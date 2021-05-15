
function hex2a(hexx) {
    var hex = hexx.toString();
    var str = '';
    var isHex = false;
    var dontAdd = false;
    for (var i = 0; (i < hex.length && hex.substr(i, 2) !== '00'); i += 1) {
        if (isHex) {
            str += String.fromCharCode(parseInt(hex.substr(i, 2), 16));
            isHex = false;
            i += 1;
            dontAdd = true;
        }
        if (hex[i] == "%") {
            isHex = true;
        }
        else {
            if (!dontAdd) str += hex[i];
            else dontAdd = false;
        }
    }

    return str;
}


function isNumeric(str) {
    if (typeof str != "string") return false
    return !isNaN(str) && !isNaN(parseFloat(str))
}


function getParameterData(data) {
    var dataString = data.toString();
    var tempData = dataString.split("&__")[0];
    tempData = tempData.split("&");

    for (var i = 0; i < tempData.length; i++) {
        tempData[i] = tempData[i].split("=")[1];
    }

    var isIndex = false;
    for (var i = 0; i < tempData.length; i++) {
        if (isNumeric(tempData[i])) {
            isIndex = true;

            if (typeof tempData[i + 1] == 'undefined') {
                tempData.splice(i + 1, 0, "false");
            }
        }
        if (isIndex) {
            if (isNumeric(tempData[i + 1]) && tempData[i] != "true") {
                console.log(tempData[i + 1]);
                tempData.splice(i + 1, 0, "false");
                isIndex = false;
            }
        }
    }
    return tempData;
}