function checkSize() {
    var yes = false;
    var input = document.getElementById("Image");
    if (input.files && input.files.length == 1) {
        var ext = input.value.match(/\.([^\.]+)$/)[1];
        switch (ext) {
            case 'jpg': yes = true;
            case 'png': yes = true;
                break;
            default:
                alert('Неверный формат!', 'Ошибка');
                input.value = '';
        }
        if (!yes) {

            if (input.files[0].size > 5000000) {
                alert("Слишком большой размер файла!", 'Ошибка');
                input.value = '';
                return false;
            }
        }

    }
    return true;
}