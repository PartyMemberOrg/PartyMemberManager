
/**
 * 搜索患者
 * @param {any} url 查询Action的url地址
 * @param {any} callBackFun 回调函数，传入选中患者的id
 */
function searchPatientArchive(url, callBackFun) {
    var hospitalizationNoFilter = $("#HospitalizationNoFilter").val();
    var nameFilter = $("#NameFilter").val();
    var phoneFilter = $("#PhoneFilter").val();
    var patientStatusFilter = $("#PatientStatusFilter").val();
    var sessionKey = $("#SessionKey").val();
    var where = { sessionKey: sessionKey, hospitalizationNoFilter: hospitalizationNoFilter, nameFilter: nameFilter, phoneFilter: phoneFilter, patientStatusFilter: patientStatusFilter };
    $.ajax({
        url: url,
        data: where,
        dataType: "json",
        type: "POST",
        error: function () { },
        success: function (data) {
            if (data.count == 0) {
                showMessage("未找到满足条件的患者信息");
            }
            else
                if (data.count == 1) {
                    callBackFun(data.data[0].id);
                }
                else {
                    showTable(url, where, callBackFun);
                }
        },
        complete: function () { }
    });
}


/**
 * 弹出表格选择患者
 * @param {any} url 查询患者Action的url
 * @param {any} where 过滤条件
 * @param {any} callBackFun 回调函数，传入选中患者的id
 */
function showTable(url, where, callBackFun) {

    top.layui.use('table', function () {
        var table = top.layui.table;
        top.layui.use('layer', function () {
            var layer = top.layui.layer;
            layer.open({
                type: 1,
                area: ["650px", '430px'],
                title: "根据查询条件找到如下患者：",
                maxmin: false,
                content: '<div><table id="templateTable"></table></div>',
                success: function (index, layero) {

                    table.render({
                        elem: '#templateTable'
                        , height: 300
                        , width: '100%'
                        , url: url //数据接口
                        , where: where
                        //, data: data
                        , page: false //开启分页
                        , cols: [[ //表头
                            { type: 'radio', title: '选择' }
                            , { field: 'name', title: '患者姓名', width: '74', minWidth: 50 }
                            , { field: 'age', title: '年龄', width: '50', minWidth: 50 }
                            , {
                                field: 'admissionTime', title: '入院时间', width: '120', templet: function (d) {
                                    return formatDate(d.admissionTime, 'yyyy-MM-dd HH:mm');
                                }
                            }
                            , { field: 'hospitalizationNo', title: '住院号', width: '70' }
                            , { field: 'inpatientAreaName', title: '病区', width: '70' }
                            , { field: 'bedNo', title: '床号', width: '60' }
                            , { field: 'doctorName', title: '医生', width: '60' }
                            , { field: 'patientStatusString', title: '当前状态', width: '70' }
                            // , { field: 'id', title: '操作', width: '80' }

                        ]]
                    });

                },

                btn: ['确定', '关闭'],
                yes: function (index, layero) {
                    var checkStatus = table.checkStatus('templateTable'); //
                    var datas = checkStatus.data;//选中的数据
                    if (datas.length > 0) {
                        var selectData = datas[0];
                        var tpid = selectData.id;
                        var tpname = selectData.name;
                        if (callBackFun != null)
                            callBackFun(tpid);
                        //$('#tpId').val(tpid);
                        //$('#tpName').val(tpname);
                        //alert(tpid);
                        try {
                            layer.close(index);//关闭对话框.
                        } catch (e) {
                            setTimeout(function () { layer.close(index) }, 100);//延时0.1秒，对应360 7.1版本bug
                        }
                    } else {
                        layer.msg('请选择一位患者!');
                    }

                }
            });
        });
    });

}


//返回查询结果中的患者Id列表
function getPatientArchiveListString() {
    return $("#PatientArchiveListString").val().split(',');
}
//是否有查询结果
function hasPatientArchiveList() {
    return $("#PatientArchiveListString").val().length > 0;
}
//移到前一个
function prev() {
    var id = getId();
    if (!hasPatientArchiveList()) {
        showMessage("无查询结果，请先查询");
        return;
    }
    var patientArchiveList = getPatientArchiveListString();
    if (patientArchiveList.length == 0) {
        showMessage("没有满足条件的记录，请先查询");
        return;
    }
    var pos = patientArchiveList.indexOf(id);
    if (pos > 0)
        pos--;
    else {
        if (pos == 0) {
            showMessage("已经是第一条记录了");
            return;
        }
    }
    load(patientArchiveList[pos]);
}

//移到后一个
function next() {
    var id = getId();
    if (!hasPatientArchiveList()) {
        showMessage("无查询结果，请先查询");
        return;
    }
    var patientArchiveList = getPatientArchiveListString();
    if (patientArchiveList.length == 0) {
        showMessage("没有满足条件的记录，请先查询");
        return;
    }
    var pos = patientArchiveList.indexOf(id);
    if (pos < patientArchiveList.length - 1)
        pos++;
    else {
        showMessage("已经是最后一条记录了");
        return;
    }
    load(patientArchiveList[pos]);
}