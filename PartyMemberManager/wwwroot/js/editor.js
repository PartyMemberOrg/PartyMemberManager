/**
 * 初始化日期、日期时间、日期范围控件
 */
function InitDateInput() {
    $("input[data-type=date]").each(function () {
        var elementid = $(this).attr("id");
        layui.use('laydate', function () {
            var laydate = layui.laydate;
            //执行一个laydate实例
            laydate.render({
                elem: "#" + elementid //指定元素
                , format: "yyyy-MM-dd"
                , type: 'date'
                , done: function (value, date) {
                    $("#" + elementid).val(value).change();
                }
            });
        });
    });
    $("input[data-type=datetime]").each(function () {
        var elementid = $(this).attr("id");
        layui.use('laydate', function () {
            var laydate = layui.laydate;
            //执行一个laydate实例
            laydate.render({
                elem: "#" + elementid //指定元素
                , format: "yyyy-MM-dd HH:mm"
                , type: 'datetime'
                , done: function (value, date) {
                    $("#" + elementid).val(value).change();
                }
            });
        });
    });
    $("input[data-type=daterange]").each(function () {
        var elementid = $(this).attr("id");
        layui.use('laydate', function () {
            var laydate = layui.laydate;
            //执行一个laydate实例
            laydate.render({
                elem: "#" + elementid //指定元素
                , format: "yyyy-MM-dd"
                , type: 'date'
                , range:true
                , done: function (value, date) {
                    $("#" + elementid).val(value).change();
                }
            });
        });
    });
}

/**
 * 通过Id单独初始化个别日期组件
 * @param {String} id 控件id，需要加上#
 * @param {String} defaultValue
 */
function InitDateInputById(id, defaultValue) {
    var theId = id;
    var theDefaultValue = defaultValue;
    layui.use('laydate', function () {
        var laydate = layui.laydate;
        laydate.render({
            elem: id
            , format: "yyyy-MM-dd"
            , type: 'date'
            , value: defaultValue
            , isInitValue: false
        });
    });
}

/**
 * 通过Id单独初始化个别日期时间组件
 * 说明：我记得这个是目前isInitValue有一个bug，就是只要时分秒有任意一个00的就会被判断时间有问题，然后会走默认的时间调整，你可以试一下把开始的值设置成01:01:01看看是不是就不会再render的时候就弄到input上去了
 * 所以：需要把时间的00分设置为01分
 * @param {String} id 控件id，需要加上#
 * @param {String} defaultValue
 */
function InitDateTimeInputById(id, defaultValue) {
    var theId = id;
    var theDefaultValue = defaultValue;
    layui.use('laydate', function () {
        var laydate = layui.laydate;
        laydate.render({
            elem: id
            , format: "yyyy-MM-dd HH:mm"
            , type: 'datetime'
            , value: defaultValue
            , isInitValue: false
        });
    });
}