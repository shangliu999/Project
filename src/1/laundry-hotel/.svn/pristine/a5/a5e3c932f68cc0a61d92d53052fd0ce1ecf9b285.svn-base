(function ($) {
    "use strict";

    $.fn.treegridData = function (options, param) {
        //如果是调用方法
        if (typeof options == 'string') {
            return $.fn.treegridData.methods[options](this, param);
        }

        //如果是初始化组件
        options = $.extend({}, $.fn.treegridData.defaults, options || {});
        var target = $(this);
        debugger;
        //得到根节点
        target.getRootNodes = function (data) {
            var result = [];
            $.each(data, function (index, item) {
                if (!item[options.parentColumn]) {
                    result.push(item);
                }
            });
            return result;
        };
        var j = 0;
        //递归获取子节点并且设置子节点
        target.getChildNodes = function (data, parentNode, parentIndex, tbody) {
            var tempj = 0;
            $.each(data, function (i, item) {
                if (item[options.parentColumn] == parentNode[options.id]) {
                    var tr = $('<tr></tr>');
                    j++;
                    var nowParentIndex = j;
                    tr.addClass('treegrid-' + nowParentIndex);
                    tr.addClass('treegrid-parent-' + parentIndex);
                    tr.attr("data-id", "tr" + nowParentIndex);
                    tr.attr('data-parentid', "tr" + parentIndex);
                    $.each(options.columns, function (index, column) {
                        if (options.checkbox && index == 0) {
                            var td = $('<td style="width:30px;"></td>');
                            var cb = $('<input type="checkbox" data-type="1" style="margin:4px;" value=' + item[column.field] + ' />').on("click", function () {
                                //CheckBox 点击事件
                                var check = $(this).is(':checked');
                                var trId = $(this).parent().parent().attr("data-id");
                                $(this).parent().parent().find("input[type='checkbox']").attr("checked", check);
                                $(this).parent().parent().find("input[type='checkbox']").prop("checked", check);
                                target.find("tr[data-parentid='" + trId + "']").find("input[type='checkbox']").attr("checked", check);
                                target.find("tr[data-parentid='" + trId + "']").find("input[type='checkbox']").prop("checked", check);
                            })
                            if (item["IsCheck"]) {
                                cb.attr("checked", "true");
                                td.append(cb);
                            } else {
                                td.append(cb);
                            }
                            tr.append(td);
                        } else {
                            var td = $('<td></td>');
                            if ((typeof item[column.field]) == "object") {
                                $.each(item[column.field], function (i, row) {
                                    var ocb = $('<input type="checkbox" data-type="2" data-right=' + row["RightID"] + ' style="margin:4px;" value=' + row['BtnID'] + ' />').html(row["BtnName"]);
                                    if (row["IsCheck"]) {
                                        ocb.attr("checked", "true");
                                    }
                                    td.append(ocb).append(row["BtnName"]);
                                });
                                tr.append(td);
                            }
                            else {
                                td.text(item[column.field]);
                                tr.append(td);
                            }
                        }
                    });
                    tbody.append(tr);
                    //target.getChildNodes(data, item, nowParentIndex, tbody);
                    target.getChildNodes(data, item, nowParentIndex, tbody);
                }
            });
        };
        target.addClass('table');
        if (options.striped) {
            target.addClass('table-striped');
        }
        if (options.bordered) {
            target.addClass('table-bordered');
        }
        if (options.url) {
            $.ajax({
                type: options.type,
                url: options.url,
                data: options.ajaxParams,
                dataType: "JSON",
                success: function (data, textStatus, jqXHR) {
                    debugger;
                    //构造表头
                    var thr = $('<tr></tr>');

                    $.each(options.columns, function (i, item) {
                        if (options.checkbox && i == 0) {
                            var th = $('<th style="padding:10px;"></th>');
                            var allcb = $('<input type="checkbox" />').on("click", function () {
                                var check = $(this).is(':checked'); 
                                $(this).parent().parent().parent().parent().find("input[type='checkbox']").attr("checked", check);
                                $(this).parent().parent().parent().parent().find("input[type='checkbox']").prop("checked", check);
                            });
                            th.append(allcb);
                            thr.append(th);
                        } else {
                            var th = $('<th style="padding:10px;"></th>');
                            th.text(item.title);
                            thr.append(th);
                        }
                    });
                    var thead = $('<thead></thead>');
                    thead.append(thr);
                    target.append(thead);

                    //构造表体
                    var tbody = $('<tbody></tbody>');
                    var rootNode = target.getRootNodes(data);
                    $.each(rootNode, function (i, item) {
                        var tr = $('<tr></tr>');
                        j++;
                        tr.addClass('treegrid-' + (j));
                        tr.attr("data-id", "tr" + (j));
                        $.each(options.columns, function (index, column) {
                            if (options.checkbox && index == 0) {
                                var td = $('<td style="width:30px;"></td>');
                                var cb = $('<input type="checkbox" style="margin:4px;" data-type="1" value=' + item[column.field] + ' />').on("click", function () {
                                    //CheckBox 点击事件
                                    var check = $(this).is(':checked');
                                    var trId = $(this).parent().parent().attr("data-id");
                                    $(this).parent().parent().find("input[type='checkbox']").attr("checked", check);
                                    $(this).parent().parent().find("input[type='checkbox']").prop("checked", check);
                                    target.find("tr[data-parentid='" + trId + "']").find("input[type='checkbox']").attr("checked", check);
                                    target.find("tr[data-parentid='" + trId + "']").find("input[type='checkbox']").prop("checked", check);
                                    var twoTrId = target.find("tr[data-parentid='" + trId + "']").find("input[type='checkbox']").parent().parent().attr("data-id");
                                    target.find("tr[data-parentid='" + twoTrId + "']").find("input[type='checkbox']").attr("checked", check);
                                    target.find("tr[data-parentid='" + twoTrId + "']").find("input[type='checkbox']").prop("checked", check);
                                })
                                if (item["IsCheck"]) {
                                    cb.attr("checked", "true");
                                    td.append(cb);
                                } else {
                                    td.append(cb);
                                }
                                tr.append(td);
                            } else {
                                var td = $('<td></td>');
                                if ((typeof item[column.field]) == "object") {
                                    $.each(item[column.field], function (i, row) {
                                        var ocb = $('<input type="checkbox" data-type="2" data-right=' + row["RightID"] + ' style="margin:4px;" value=' + row['BtnID'] + ' />');
                                        if (row["IsCheck"]) {
                                            ocb.attr("checked", "true");
                                        }
                                        td.append(ocb).append(row["BtnName"]);
                                    });
                                    tr.append(td);
                                }
                                else {
                                    td.text(item[column.field]);
                                    tr.append(td);
                                }
                            }
                        });

                        tbody.append(tr);
                        target.getChildNodes(data, item, (j), tbody);
                    });
                    target.append(tbody);
                    target.treegrid({
                        expanderExpandedClass: options.expanderExpandedClass,
                        expanderCollapsedClass: options.expanderCollapsedClass,
                        treeColumn: options.expandColumn
                    });
                    if (!options.expandAll) {
                        target.treegrid('collapseAll');
                    }
                }
            });
        }
        else {
            //也可以通过defaults里面的data属性通过传递一个数据集合进来对组件进行初始化....有兴趣可以自己实现，思路和上述类似
        }
        return target;
    };

    $.fn.treegridData.methods = {
        getAllNodes: function (target, data) {
            return target.treegrid('getAllNodes');
        },
        getChildNodes: function (target) {
            return target.treegrid('getChildNodes');
        }
        //组件的其他方法也可以进行类似封装........
    };

    $.fn.treegridData.defaults = {
        id: 'Id',
        parentColumn: 'ParentId',
        data: [],    //构造table的数据集合
        type: "GET", //请求数据的ajax类型
        url: null,   //请求数据的ajax的url
        ajaxParams: {}, //请求数据的ajax的data属性
        expandColumn: 0,//在哪一列上面显示展开按钮
        expandAll: true,  //是否全部展开
        striped: false,   //是否各行渐变色
        bordered: false,  //是否显示边框
        columns: [],
        checkbox: false,
        expanderExpandedClass: 'glyphicon glyphicon-chevron-down',//展开的按钮的图标
        expanderCollapsedClass: 'glyphicon glyphicon-chevron-right'//缩起的按钮的图标

    };
})(jQuery);