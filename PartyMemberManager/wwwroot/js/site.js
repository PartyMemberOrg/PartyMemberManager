///// <reference path="../lib/layui/layui.js" />
///// <reference path="../lib/jquery.serializejson-master/jquery.serializejson.min.js" />
///// <reference path="dialogs.js" />

// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
/**
 * 删除数据
 * @param {String} url - 删除操作url，通过ajax调用
 * @param {String} id - 删除数据的id
 * @param  {Function} callFun - 删除成功或失败后的回调函数
 */
function DeleteData(url, id, callFun) {
    showConfirm('确定要删除本条数据吗?', function () {
        //var verification_code = "";
        $.ajax({
            url: url,
            data: {
                "id": id//, "verification_code": verification_code
            },
            type: "post",
            dataType: "json",
            success: function (response) {
                if (response.code == 0) {
                    if (callFun != null) {
                        callFun();
                        showMessage("数据删除成功");
                    }
                    else {
                        showMessage("数据删除成功");
                        location.reload();
                    }
                    //showMessage("数据删除成功", function () {
                    //    if (callFun != null)
                    //        callFun();
                    //    else
                    //        //刷新页面
                    //        location.reload();
                    //});
                }
                else {
                    showError(response.message, function () {
                        if (callFun != null)
                            callFun();
                        else
                            //刷新页面
                            location.reload();
                    });
                }
            },
            error: function () {
                showError("删除数据时发生错误");
            }
        });
    })
}
/**
* 重置打印状态
* @param {String} url - 重置打印操作url，通过ajax调用
* @param {String} id - 重置打印数据的id
* @param  {Function} callFun - 重置打印成功或失败后的回调函数
*/
function ResetPrintData(url, id, callFun) {
    showConfirm('确定要重置本条数据的打印吗?', function () {
        //var verification_code = "";
        $.ajax({
            url: url,
            data: {
                "id": id//, "verification_code": verification_code
            },
            type: "post",
            dataType: "json",
            success: function (response) {
                if (response.code == 0) {
                    if (callFun != null) {
                        callFun();
                        showMessage("重置打印成功");
                    }
                    else {
                        showMessage("重置打印成功");
                        location.reload();
                    }
                    //showMessage("数据删除成功", function () {
                    //    if (callFun != null)
                    //        callFun();
                    //    else
                    //        //刷新页面
                    //        location.reload();
                    //});
                }
                else {
                    showError(response.message, function () {
                        if (callFun != null)
                            callFun();
                        else
                            //刷新页面
                            location.reload();
                    });
                }
            },
            error: function () {
                showError("重置打印数据时发生错误");
            }
        });
    })
}
/**
 * 显示编辑对话框
 * @param {String} url - 编辑页面的url
 * @param {String} postUrl - 数据保存Action的url，通过ajax提交保存
 * @param {String} title - 编辑窗体标题
 * @param {Integer} width - 编辑窗口宽度
 * @param {Integer} height - 编辑窗口高度
 * @param {Function} callBack - 编辑结束并保存后的回调函数
 */
function showEdit(url, postUrl, title, width = 600, height = 400, callBack) {
    top.layui.use('layer', function () {
        var layer = top.layui.layer;
        layer.ready(function () {
            var index = layer.load(2);
            layer.open({
                title: title
                , area: [width.toString() + 'px', height.toString() + 'px']
                , type: 2
                , content: [url, 'no']//第二个参数no，表示不显示iframe滚动条
                , btnAlign: 'c'
                , btn: ['保存']
                , yes: function (index, layero) {
                    var body = layer.getChildFrame('body', index);
                    var valid = $(body).find('form').valid();
                    if (!valid) {
                        showError("数据验证错误，请检查数据后再提交");
                        return;
                    }
                    var data = $(body).find('form').serializeJSON({ parseBooleans: true });
                    $.ajax({
                        url: postUrl,
                        data: data,
                        type: "post",
                        dataType: "json",
                        success: function (response) {
                            if (response.code == 0) {
                                clearError($(body).find('form'));
                                layer.close(index);
                                showMessage("数据保存成功");
                                //如果有回调函数，则执行回调函数,否则直接刷新页面
                                if (callBack != null)
                                    callBack(data);
                                else
                                    location.reload();
                            }
                            else {
                                displayErrorMessage($(body).find('form'), response);
                                layer.msg(response.message, {
                                    icon: 2,
                                    time: 2000
                                },
                                    function () {
                                        //保存时发生错误
                                    });
                            }
                        },
                        error: function () {
                            showError("保存数据时发生错误");
                        }
                    });
                }
            });
            layer.close(index);
        });
    });
}


/**
 * 显示编辑删除对话框
 * @param {String} url - 编辑页面的url
 * @param {String} postUrl - 数据保存Action的url，通过ajax提交保存
 * @param {String} deleteUrl - 数据删除Action的url，通过ajax提交保存
 * @param {String} title - 编辑窗体标题
 * @param {Integer} width - 编辑窗口宽度
 * @param {Integer} height - 编辑窗口高度
 * @param {Function} callBack - 编辑结束并保存后的回调函数
 */
function showEditDelete(url, postUrl, deleteUrl, title, width = 600, height = 400, callBack) {
    top.layui.use('layer', function () {
        var layer = top.layui.layer;
        layer.ready(function () {
            var index = layer.load(2);
            layer.open({
                title: title
                , area: [width.toString() + 'px', height.toString() + 'px']
                , type: 2
                , content: [url, 'no']//第二个参数no，表示不显示iframe滚动条
                , btnAlign: 'c'
                , btn: ['保存', '删除']
                , yes: function (index, layero) {
                    var body = layer.getChildFrame('body', index);
                    var valid = $(body).find('form').valid();
                    if (!valid) {
                        showError("数据验证错误，请检查数据后再提交");
                        return;
                    }
                    var data = $(body).find('form').serializeJSON({ parseBooleans: true });
                    $.ajax({
                        url: postUrl,
                        data: data,
                        type: "post",
                        dataType: "json",
                        success: function (response) {
                            if (response.code == 0) {
                                clearError($(body).find('form'));
                                layer.close(index);
                                showMessage("数据保存成功");
                                //如果有回调函数，则执行回调函数,否则直接刷新页面
                                if (callBack != null)
                                    callBack(data);
                                else
                                    location.reload();
                            }
                            else {
                                displayErrorMessage($(body).find('form'), response);
                                layer.msg(response.message, {
                                    icon: 2,
                                    time: 2000
                                },
                                    function () {
                                        //保存时发生错误
                                    });
                            }
                        },
                        error: function () {
                            showError("保存数据时发生错误");
                        }
                    });
                }
                , btn2: function (index, layero) {
                    var body = layer.getChildFrame('body', index);
                    var data = $(body).find('form').serializeJSON({ parseBooleans: true });
                    var id = data.Id;
                    layer.close(index);
                    showConfirm('确定要删除本条数据吗?', function () {
                        //var verification_code = "";
                        $.ajax({
                            url: deleteUrl,
                            data: {
                                "id": id//, "verification_code": verification_code
                            },
                            type: "post",
                            dataType: "json",
                            success: function (response) {
                                if (response.code == 0) {
                                    showMessage("数据删除成功", function () {
                                        if (callBack != null)
                                            callBack();
                                        else
                                            //刷新页面
                                            location.reload();
                                    });
                                }
                                else {
                                    showError(response.message, function () {
                                        if (callBack != null)
                                            callBack();
                                        else
                                            //刷新页面
                                            location.reload();
                                    });
                                }
                            },
                            error: function () {
                                showError("删除数据时发生错误");
                            }
                        });
                    })
                }
            });
            layer.close(index);
        });
    });
}


/**
 * 数据顺序上移或下移
 * @param {any} url
 * @param {any} id
 * @param {any} isUp
 * @param {any} callFun
 */
function DataOrdinalUpDown(url, id, isUp, callFun) {
    $.ajax({
        url: url,
        data: {
            "id": id,//, "verification_code": verification_code
            "isUP": isUp
        },
        type: "post",
        dataType: "json",
        success: function (response) {
            if (response.code == 0) {
                if (callFun != null)
                    callFun();
                else
                    //刷新页面
                    location.reload();
            }
            else {
                showError(response.message, function () {
                    if (callFun != null)
                        callFun();
                    else
                        //刷新页面
                        location.reload();
                });
            }
        },
        error: function () {
            showError("移动数据时发生错误");
        }
    });
}

/**
 * 使用ajax保存当前页面的数据
 * @param {any} postUrl 数据保存action
 * @param {any} callBack 保存完成后的回调函数
 */
function save(postUrl, callBack) {
    var valid = $(document).find('form').valid();
    if (!valid) {
        showError("数据验证错误，请检查数据后再提交");
        return;
    }
    var data = $(document).find('form').serializeJSON();
    $.ajax({
        url: postUrl,
        data: data,
        type: "post",
        dataType: "json",
        success: function (response) {
            if (response.code == 0) {
                clearError($(document).find('form'));
                showMessage("数据保存成功");
                //如果有回调函数，则执行回调函数,否则直接刷新页面
                if (callBack != null)
                    callBack(data);
                else
                    location.reload();
            }
            else {
                displayErrorMessage($(document).find('form'), response);
                showError(response.message, function () {
                    //保存时发生错误
                });
            }
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            showError("保存数据时发生错误");
        }
    });
}

/**
 * 显示错误信息
 * @param {any} form
 * @param {any} response
 */
function displayErrorMessage(form, response) {
    //如果保存时发生错误，便利遍历错误信息
    form.find("[data-valmsg-for]").html("");
    form.find("[data-valmsg-for]").removeClass("field-validation-error");
    form.find("[data-valmsg-for]").removeClass("field-validation-valid");
    //$(".invalid").removeClass("invalid");
    if (response.errors.length > 0) {
        for (var i = 0; i < response.errors.length; i++) {
            var error = response.errors[i];
            var errorValidateMessge = form.find("[data-valmsg-for='" + error.key + "']");
            errorValidateMessge.html(ConvertErrorMessage(error.message));
            errorValidateMessge.removeClass("field-validation-valid");
            errorValidateMessge.addClass("field-validation-error");
            form.find("#" + error.key).addClass("input-validation-error");
        }
    }
}
/**
 * 清除错误
 * @param {any} form
 */
function clearError(form) {
    form.find("[data-valmsg-for]").html("");
    form.find("[data-valmsg-for]").removeClass("field-validation-error");
    form.find("[data-valmsg-for]").removeClass("field-validation-valid");
    //$(".invalid").removeClass("invalid");
}

/**
 * 错误消息转换
 * @param {String} message
 */
function ConvertErrorMessage(message) {
    if (message == "The value '' is invalid.")
        return "字段不允许空";
    else
        return message;
}

/**
 * 格式化日期
 * @param {any} date
 * @param {any} format
 */
function formatDate(date, format) {
    if (date == null)
        return "";
    var formatedDate = null;
    top.layui.use('util', function () {
        var util = top.layui.util;
        formatedDate = util.toDateString(date, format);
    });
    if (formatedDate == null) {
        layui.use('util', function () {
            var util = layui.util;
            formatedDate = util.toDateString(date, format);
        });
    }
    return formatedDate;
}


/**
 * 显示内容对话框
 * @param {String} url - 编辑页面的url
 * @param {String} title - 编辑窗体标题
 * @param {Integer} width - 编辑窗口宽度
 * @param {Integer} height - 编辑窗口高度
 * @param {Function} callBack - 编辑结束并保存后的回调函数
 */
function showContent(url, title, width = 600, height = 400) {
    top.layui.use('layer', function () {
        var layer = top.layui.layer;
        layer.ready(function () {
            var index = layer.load(2);
            layer.open({
                title: title
                , area: [width.toString() + 'px', height.toString() + 'px']
                , type: 2
                , content: [url, 'no']//第二个参数no，表示不显示iframe滚动条
                , btnAlign: 'c'
                , btn: ['确定']
                , yes: function (index, layero) {
                    layer.close(index);
                }
            });
            layer.close(index);
        });
    });
}
/**
 * 显示打印对话框
 * @param {String} url - 编辑页面的url
 * @param {String} title - 编辑窗体标题
 * @param {Integer} width - 编辑窗口宽度
 * @param {Integer} height - 编辑窗口高度
 * @param {Function} updatePrintStatus - 更新打印状态回调函数
 * @param {Function} callBack - 关闭回调函数
 */
function showPrint(url, title, width = 600, height = 400, updatePrintStatusCallBack, callBack) {
    top.layui.use('layer', function () {
        var layer = top.layui.layer;
        var printed = 0;
        layer.ready(function () {
            var index = layer.load(2);
            layer.open({
                title: title
                , area: [width.toString() + 'px', height.toString() + 'px']
                , type: 2
                , scrollbar: false
                , content: [url, 'no']//第二个参数no，表示不显示iframe滚动条
                , btnAlign: 'c'
                , btn: ['打印', '关闭']
                , yes: function (index, layero) {
                    var body = layer.getChildFrame('body', index);
                    printed = 1;
                    $(body).find('#print')[0].contentWindow.print();
                    if (updatePrintStatusCallBack != null)
                        updatePrintStatusCallBack();
                    //layer.close(index);
                }
                , btn2: function (index, layero) {
                    if (printed == 1) {
                        if (callBack != null)
                            callBack();
                    }
                    layer.close(index);
                }
                , cancel: function (index, layero) {
                    if (printed == 1) {
                        if (callBack != null)
                            callBack();
                    }
                    layer.close(index);
                }
            });
            layer.close(index);
        });
    });
}

/**
 * 显示编辑对话框
 * @param {String} url - 编辑页面的url
 * @param {String} postUrl - 数据保存Action的url，通过ajax提交保存
 * @param {String} title - 编辑窗体标题
 * @param {Integer} width - 编辑窗口宽度
 * @param {Integer} height - 编辑窗口高度
 * @param {Function} callBack - 编辑结束并保存后的回调函数
 */
function showImport(url, postUrl, title, width = 600, height = 400, callBack) {
    top.layui.use('layer', function () {
        var layer = top.layui.layer;
        layer.ready(function () {
            var index = layer.load(2);
            layer.open({
                title: title
                , area: [width.toString() + 'px', height.toString() + 'px']
                , type: 2
                , content: [url, 'no']//第二个参数no，表示不显示iframe滚动条
                , btnAlign: 'c'
                , btn: ['导入']
                , yes: function (index, layero) {
                    var body = layer.getChildFrame('body', index);
                    var valid = $(body).find('form').valid();
                    if (!valid) {
                        showError("数据验证错误，请检查数据后再提交");
                        return;
                    }
                    var data = new FormData($(body).find('form')[0]);
                    $.ajax({
                        url: postUrl,
                        data: data,
                        type: "post",
                        contentType: false,
                        processData: false,
                        success: function (response) {
                            if (response.code == 0) {
                                clearError($(body).find('form'));
                                layer.close(index);
                                showMessage("数据导入成功");
                                //如果有回调函数，则执行回调函数,否则直接刷新页面
                                if (callBack != null)
                                    callBack(data);
                                else
                                    location.reload();
                            }
                            else if (response.code == -2) {
                                //部分导入成功
                                window.open(response.errorDataFile, "_blank")
                                clearError($(body).find('form'));
                                layer.close(index);
                                showMessage("成功导入" + response.successCount + "条，失败" + response.failCount + "条");
                                //如果有回调函数，则执行回调函数,否则直接刷新页面
                                if (callBack != null)
                                    callBack(data);
                                else
                                    location.reload();
                            }
                            else {
                                displayErrorMessage($(body).find('form'), response);
                                layer.msg(response.message, {
                                    icon: 2,
                                    time: 2000
                                },
                                    function () {
                                        //保存时发生错误
                                    });
                            }
                        },
                        error: function (response) {
                            showError("导入数据时发生错误");
                        }
                    });
                }
            });
            layer.close(index);
        });
    });
}
/**
 * 显示编辑对话框
 * @param {String} url - 编辑页面的url
 * @param {String} postUrl - 数据保存Action的url，通过ajax提交保存
 * @param {String} title - 编辑窗体标题
 * @param {Integer} width - 编辑窗口宽度
 * @param {Integer} height - 编辑窗口高度
 * @param {Function} callBack - 编辑结束并保存后的回调函数
 */
function showUpFile(url, postUrl, title, width = 600, height = 400, callBack) {
    top.layui.use('layer', function () {
        var layer = top.layui.layer;
        layer.ready(function () {
            var index = layer.load(2);
            layer.open({
                title: title
                , area: [width.toString() + 'px', height.toString() + 'px']
                , type: 2
                , content: [url, 'no']//第二个参数no，表示不显示iframe滚动条
                , btnAlign: 'c'
                , btn: ['保存']
                , yes: function (index, layero) {
                    var body = layer.getChildFrame('body', index);
                    var valid = $(body).find('form').valid();
                    if (!valid) {
                        showError("数据验证错误，请检查数据后再提交");
                        return;
                    }
                    var data = new FormData($(body).find('form')[0]);
                    $.ajax({
                        url: postUrl,
                        data: data,
                        type: "post",
                        contentType: false,
                        processData: false,
                        success: function (response) {
                            if (response.code == 0) {
                                clearError($(body).find('form'));
                                layer.close(index);
                                showMessage("数据上传成功");
                                //如果有回调函数，则执行回调函数,否则直接刷新页面
                                if (callBack != null)
                                    callBack(data);
                                else
                                    location.reload();
                            }
                            else if (response.code == -2) {
                                //部分导入成功
                                window.open(response.errorDataFile, "_blank")
                                clearError($(body).find('form'));
                                layer.close(index);
                                showMessage("成功上传" + response.successCount + "条，失败" + response.failCount + "条");
                                //如果有回调函数，则执行回调函数,否则直接刷新页面
                                if (callBack != null)
                                    callBack(data);
                                else
                                    location.reload();
                            }
                            else {
                                displayErrorMessage($(body).find('form'), response);
                                layer.msg(response.message, {
                                    icon: 2,
                                    time: 2000
                                },
                                    function () {
                                        //保存时发生错误
                                    });
                            }
                        },
                        error: function (response) {
                            showError("上传数据时发生错误");
                        }
                    });
                }
            });
            layer.close(index);
        });
    });
}

/**
 * 显示编辑对话框，并返回数据
 * @param {String} url - 编辑页面的url
 * @param {String} title - 编辑窗体标题
 * @param {Integer} width - 编辑窗口宽度
 * @param {Integer} height - 编辑窗口高度
 * @param {Function} callBack - 编辑结束并保存后的回调函数
 */
function showEditReturnData(url, title, width = 600, height = 400, callBack) {
    top.layui.use('layer', function () {
        var layer = top.layui.layer;
        layer.ready(function () {
            var index = layer.load(2);
            layer.open({
                title: title
                , area: [width.toString() + 'px', height.toString() + 'px']
                , type: 2
                , content: [url]//第二个参数no，表示不显示iframe滚动条
                , btnAlign: 'c'
                , btn: ['保存']
                , yes: function (index, layero) {
                    var body = layer.getChildFrame('body', index);
                    var valid = $(body).find('form').valid();
                    if (!valid) {
                        showError("数据验证错误，请检查数据后再提交");
                        return;
                    }
                    var data = $(body).find('form').serializeJSON({ parseBooleans: true });
                    layer.close(index);
                    callBack(data);
                }
            });
            layer.close(index);
        });
    });
}


/**
 * 验证Guid形式
 * @param {any} testID
 */
function TestGuid(testID) {
    var reg = new RegExp(/^[0-9a-z]{8}-[0-9a-z]{4}-[0-9a-z]{4}-[0-9a-z]{4}-[0-9a-z]{12}$/);
    if (reg.test(testID)) {
        return true;
    }
    return false;
}


/**
 * 显示日期对话框
 * @param {String} url - 编辑页面的url
 * @param {Function} callBack - 编辑结束并保存后的回调函数
 */
function showPrintDate(url,callBack) {
    top.layui.use('layer', function () {
        var layer = top.layui.layer;
        layer.ready(function () {
            var index = layer.load(2);
            layer.open({
                title: '选择打印日期'
                , area: ['400px', '450px']
                , type: 2
                , content: [url, 'no']//第二个参数no，表示不显示iframe滚动条
                , btnAlign: 'c'
                , btn: ['确定']
                , yes: function (index, layero) {
                    var body = layer.getChildFrame('body', index);
                    var date = $(body).find('#date').val();
                    layer.close(index);
                    callBack(date);
                }
            });
            layer.close(index);
        });
    });
}