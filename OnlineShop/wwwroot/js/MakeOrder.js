function InOrder() {
    document.getElementById('InOrder').value = "";
    document.getElementById('linkorder').href = "MakeOrder?InOrder=";
    var inputElements = document.getElementsByClassName('cart-checkbox');
    for (var i = 0; inputElements[i]; ++i) {
        if (inputElements[i].checked) {
            document.getElementById('InOrder').value = document.getElementById('InOrder').value + "?" + inputElements[i].value;
        }
    }
    document.getElementById('linkorder').href = document.getElementById('linkorder').href + document.getElementById('InOrder').value;
}
