/*顶部工具栏方法*/
function InitToolbar() {
    var buttons = $("#toolbar-button-container").children();
    buttons.each(function (index, element) {
        $(element).on("click", function () {
            if (!$(this).find(".toolbar-button").hasClass("disable")) {
                $(this).addClass("toolbar-active");
                $(this).siblings().removeClass("toolbar-active");
            }
        });
    });
    //尽量使活动按钮居中
    //var activeButton = buttons.children(".toolbar-active");
    var activeButtonIndex = 0;
    for (var i = 0; i < buttons.length; i++) {
        if ($(buttons[i]).hasClass("toolbar-active"))
            activeButtonIndex = i;
    }
    if (activeButtonIndex < 0)
        activeButtonIndex = 0;
    var maxButtonCount = maxButtonsCount();
    var leftNear = activeButtonIndex;
    var rightNear = activeButtonIndex;
    if (buttons.length > maxButtonCount) {
        while ((rightNear - leftNear + 1) < maxButtonCount - 1) {
            if (leftNear > 0)
                leftNear--;
            if (rightNear < buttons.length - 1)
                rightNear++;
            if (leftNear == 0 && rightNear == buttons.length - 1)
                break;
        }
        //跳过最左边的基本信息按钮
        if (leftNear == 0 && rightNear < buttons.length - 1) {
            leftNear++;
            rightNear++;
        }
        if (rightNear - leftNear + 1 >= maxButtonCount)
            rightNear--;
        for (var i = 0; i < leftNear; i++) {
            if (i != 0)
                $(buttons.get(i)).hide();
        }
        for (var i = rightNear + 1; i < buttons.length; i++) {
            $(buttons.get(i)).hide();
        }
    }
}
function showPrevButton() {
    var buttons = $("#toolbar-button-container").children();
    var maxButtonCount = maxButtonsCount();
    for (var i = 0; i < buttons.length; i++) {
        if ($(buttons.get(i)).is(":visible")) {
            if (i > 0) {
                if (buttons.length > i + maxButtonCount - 1) {
                    //$(buttons.get(i + maxButtonCount - 1)).animate({ width: "0px" },300);//.hide(30);
                    $(buttons.get(i + maxButtonCount - 1)).hide();
                }
                //$(buttons.get(i - 1)).width(82);//.animate({ width: "82px" });//.show(30);
                $(buttons.get(i - 1)).show();
                break;
            }
        }
    }
}
function showNextButton() {
    var buttons = $("#toolbar-button-container").children();
    var maxButtonCount = maxButtonsCount();
    for (var i = 1; i < buttons.length; i++) {
        if (i < buttons.length - (maxButtonCount - 1)) {
            if ($(buttons.get(i)).is(":visible")) {
                //$(buttons.get(i)).animate({ width: "0px" },300);//.hide(30);
                $(buttons.get(i)).hide();
                if (buttons.length > i + maxButtonCount - 1) {
                    //$(buttons.get(i + maxButtonCount)).width(82);//.animate({ width: "82px" });//.show(30);
                    $(buttons.get(i + maxButtonCount - 1)).show();
                }
                break;
            }
        }
        else
            break;
    }
}

function maxButtonsCount() {
    var containerWidth = $("#toolbar-button-container").width();
    var maxButtonCount = Math.trunc(containerWidth / 85);
    return maxButtonCount;
}