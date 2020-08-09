/// <reference path="../lib/layui/layui.js" />
/**
 * 显示错误提示对话框，显示4秒钟
 * @param {String} message - 错误提示信息
 * @param {Function} callBackFun - 回调函数，错误消息关闭后调用该回调函数
*/
function showError(message, callBackFun) {
    top.layui.use('layer', function () {
        var layer = top.layui.layer;
        layer.ready(function () {
            var index = layer.load(2);
            layer.msg(message, {
                icon: 2,
                time: 2000
            }, function () {
                if (callBackFun != null)
                    callBackFun();
            });
            layer.close(index);
        });
    });
}

/**
 * 显示信息提示框，显示2秒钟
 * @param {String} message - 提示信息
 * @param {Function} callBackFun - 回调函数，消息关闭后调用该回调函数
*/
function showMessage(message, callBackFun) {
    top.layui.use('layer', function () {
        var layer = top.layui.layer;
        layer.ready(function () {
            var index = layer.load(2);
            layer.msg(message, {}, function () {
                if (callBackFun != null)
                    callBackFun();
            });
            layer.close(index);
        });
    });
}

/**
 * 显示确认对话款
 * @param {String} message - 询问信息
 * @param {Function} callBackFun - 回调函数，用户点击完确定后调用该函数
 * @param {Function} cancelCallbackFun - 回调函数，用户点击完取消后调用该函数
 */
function showConfirm(message, callBackFun, cancelCallbackFun) {
    top.layui.use('layer', function () {
        var layer = top.layui.layer;
        layer.ready(function () {
            var index = layer.load(2);
            layer.confirm(message, function (index) {
                top.layer.close(index);
                if (callBackFun != null)
                    callBackFun();
            }, function (index) {
                top.layer.close(index);
                if (cancelCallbackFun != null)
                    cancelCallbackFun();
            });
            layer.close(index);
        });
    });
}
/**
 * 显示详细页面
 * @param {String} url - 详细信息页面的url
 * @param {String} title - 窗口标题
 * @param {Integer} width - 窗口宽度
 * @param {Integer} height - 窗口高度
 */
function showDetails(url, title, width = 600, height = 400) {
    top.layui.use('layer', function () {
        var layer = top.layui.layer;
        layer.ready(function () {
            var index = layer.load(2);
            layer.open({
                title: title,
                area: [width.toString() + 'px', height.toString() + 'px'],
                type: 2,
                content: [url, 'no']
            });
            layer.close(index);
        });
    });
}

/**
 * 显示单行输入框
 * @param {any} value 初始值
 * @param {any} title 输入框标题
 * @param {function(value)} callBakFun 回调函数
 */
function showPrompt(value, title, callBakFun) {
    top.layui.use('layer', function () {
        var layer = top.layui.layer;
        layer.ready(function () {
            layer.prompt({
                formType: 0,
                value: value,
                title: title,
                area: ['800px', '350px'] //自定义文本域宽高
            }, function (value, index, elem) {
                if (callBackFun != null)
                    callBakFun(value);
                layer.close(index);
            });
        });
    });
}

/**
 * 显示信息提示框，显示2秒钟
 * @param {String} message - 提示信息
 * @param {Function} callBackFun - 回调函数，消息关闭后调用该回调函数
*/
function showTip(message, callBackFun) {
    top.layui.use('layer', function () {
        var layer = top.layui.layer;
        layer.ready(function () {
            var index = layer.load(2);
            layer.msg(message, { btn: ['知道了'] }, function () {
                if (callBackFun != null)
                    callBackFun();
            });
            layer.close(index);
        });
    });
}

/**
 * 转入营养支持询问对话框
 * @param {Function} enterNutritionSupportCallbackFun - 回调函数，用户点击【进入营养支持】后调用该函数
 * @param {Function} waitConfirmCallBackFun - 回调函数，用户点击【待复筛确认】后调用该函数
 * @param {Function} cancelCallbackFun - 回调函数，用户点击关闭按钮后调用该函数
 */
function showNutritionSupportConfirm(enterNutritionSupportCallbackFun, waitConfirmCallBackFun, cancelCallbackFun) {
    var message = "需术前营养支持，是否进入营养支持？";
    top.layui.use('layer', function () {
        var layer = top.layui.layer;
        layer.ready(function () {
            var index = layer.load(2);
            layer.confirm(message, {
                btn: ['待复筛确认', '进入营养支持'] //按钮
                , yes: function (index) {
                    top.layer.close(index);
                    waitConfirmCallBackFun();
                }, btn2: function (index) {
                    top.layer.close(index);
                    enterNutritionSupportCallbackFun();
                }, cancel: function () {
                    top.layer.close(index);
                    cancelCallbackFun();
                }
            });
            layer.close(index);
        });
    });
}
