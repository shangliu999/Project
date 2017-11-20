var url1 = localStorage.getItem("NoticeURL");
var url2 = localStorage.getItem("InfactoryURL");
var url3 = localStorage.getItem("StockURL");
var regionMode = localStorage.getItem("RegionMode");
var warehouseID = localStorage.getItem("WarehouseID");

var urlarray = [];
urlarray.push(url1);
urlarray.push(url2);
urlarray.push(url3);

//记录跳转屏幕总次数，标记并显示一次无内容屏幕，切屏速度
var pageindex, kanban, speed;
pageindex = kanban = 0;
//切屏速度30秒
speed = 1000 * 30; 

window.onload = function () {
    showScreen();

    //显示bottom
    showMarquee();
    setInterval(function () {
        showMarquee();
    }, 1000 * 60 * 5);
};

function showScreen() {
    var index, pagesize;
    index = pageindex % 2;
    //页面大小
    pagesize = index == 0 ? 1 : 6;

    //视图类型
    var view = regionMode + index + "";

    console.log("index:" + index);

    //请求数据路径
    var url = urlarray[index];

    $.ajax({
        type: "post",
        data: {},
        url: url + "?regionid=" + warehouseID + "&t=" + new Date().getTime(),
        cache: false,
        success: function (result) {
            var arr = [];
            if (result.length > 0) {
                if (result[0] == "")
                { } else {
                    if (index == 0) {
                        for (var i = 0; i < result.length; i++) {
                            if (result[i].Position == warehouseID) {
                                if (result[i].IsFull == true) {
                                    arr.push(result[i]);
                                }
                            }
                        }
                    } else {
                        arr.push(result);
                    }
                }
            }

            $(".child").children().remove();
            createheader(view);

            //切屏线程和分页线程
            var rid = 0, pagination = 0;
            kanban = 0;

            var callback = function () {
                var item = arr.splice(0, 1);

                if (item.length == 0) {
                    pageindex++;

                    //控制下一次循环
                    if (index == 0) {
                        clearInterval(rid);
                        showScreen();
                    } else {
                        kanban++;
                        if (kanban > 0) {
                            //倒退一次
                            pageindex--;
                            clearInterval(rid);
                            showScreen();
                        }
                        return false;
                    }
                }

                console.log("rid:" + rid);
                clearInterval(rid);
                if (index == 0) {
                    $(".child").children().remove();
                    showwelcome(item[0]);
                } else {
                    //分页
                    var paging = function () {
                        //分页显示完以后需要再次请求数据显示
                        if (item[0].length == 0) {
                            console.log("pagination:" + pagination);
                            clearInterval(pagination);
                            showScreen();
                            return false;
                        }

                        if (view.substr(0, 1) == "1") {
                            var start = "";
                            if (item[0] != null && item[0].length > 0) {
                                start = item[0][0].RegionName;
                            }
                            var isPaging = false;
                            for (var i = 0; i < item[0].length; i++) {
                                if (start != item[0][i].RegionName) {
                                    pagesize = i;
                                    isPaging = true;
                                    break;
                                }
                            }
                            if (!isPaging) {
                                pagesize = item[0].length;
                            }
                        }

                        var it = item[0].splice(0, pagesize);
                        if (it.length > 0) {
                            $("#dataReport table tr:not(:first)").remove();
                            createcontent(view, it);
                        }
                    };
                    paging();

                    //分页线程
                    pagination = setInterval(paging, speed);
                }
            };
            callback();

            //切屏线程,存在数据才切屏
            if (arr.length > 0) {
                rid = setInterval(callback, speed);
            }
        }
    });
}

function showwelcome(a) {
    $(".child").css({ "background-color": "gray" });

    var div, row, col;
    div = $("<div></div>").addClass("list-group").css({ "margin-top": "10%", "width": "70%", "text-align": "center" });
    row = $("<div></div>").addClass("row");
    try {
        col = $("<div></div>").addClass("col-lg-12").css({ "font-family": "Arial", "color": "black" }).html(a.Content);
    } catch (e) {

    }
    row.append(col);
    div.append(row);
    $(".child").append(div);
}

/**
* 创建表头
* @class NodeList
* @constructor
* @param a  视图类型 "11":配送中心入厂，"12":配送中心库存，"21":洗涤中心运行情况，"22":洗涤中心库存
*/
function createheader(a) {
    var div;
    div = $("<div id='dataReport'></div>").addClass("table-responsive").addClass("dataTables_wrapper");
    $(".child").append(div);

    var table, tr;
    if (a == "11") {       //仓库入厂
        table = $("<table></table>").addClass("table").addClass("table-bordered").addClass("table-hover");
        tr = $("<tr></tr>");
        $("<th style='text-align:left;'>类型</th>").appendTo(tr);
        $("<th style='text-align:right;'>数量</th>").appendTo(tr);
        $("<th style='text-align:left;'>类型</th>").appendTo(tr);
        $("<th style='text-align:right;padding-right:36px;'>数量</th>").appendTo(tr);
        $(table).append(tr);
    } else if (a == "12") {        //仓库库存
        table = $("<table></table>").addClass("table").addClass("table-bordered").addClass("table-hover");
        tr = $("<tr></tr>");
        $("<th style='text-align:left;'>品牌</th>").appendTo(tr);
        $("<th style='text-align:left;'>类型</th>").appendTo(tr);
        $("<th style='text-align:right;'>数量</th>").appendTo(tr);
        $("<th style='text-align:left;'>品牌</th>").appendTo(tr);
        $("<th style='text-align:left;'>类型</th>").appendTo(tr);
        $("<th style='text-align:right;padding-right:36px;'>数量</th>").appendTo(tr);
        $(table).append(tr);
    } else if (a == "21") {
        table = $("<table></table>").addClass("table").addClass("table-bordered").addClass("table-hover");
        tr = $("<tr></tr>");
        $("<th style='text-align:left;'>类型</th>").appendTo(tr);
        $("<th style='text-align:right;'>待洗数量</th>").appendTo(tr);
        $("<th style='text-align:right;'>洗涤中</th>").appendTo(tr);
        $("<th style='text-align:right;padding-right:36px;'>已洗涤</th>").appendTo(tr);
        $(table).append(tr);
    } else if (a == "22") {
        table = $("<table></table>").addClass("table").addClass("table-bordered").addClass("table-hover");
        tr = $("<tr></tr>");
        $("<th style='text-align:left;'>品牌</th>").appendTo(tr);
        $("<th style='text-align:left;'>类型</th>").appendTo(tr);
        $("<th style='text-align:right;'>数量</th>").appendTo(tr);
        $("<th style='text-align:left;'>品牌</th>").appendTo(tr);
        $("<th style='text-align:left;'>类型</th>").appendTo(tr);
        $("<th style='text-align:right;padding-right:36px;'>数量</th>").appendTo(tr);
        $(table).append(tr);
    }

    $(div).append(table);
}

/**
* 创建内容
* @class NodeList
* @constructor
* @param a  视图类型 "11":配送中心入厂，"12":配送中心库存，"21":洗涤中心运行情况，"22":洗涤中心库存
* @param b  数据源
*/
function createcontent(a, b) {
    if (b == null || b == undefined || b == "") {
        return;
    }

    var it, it1, tr;

    if (a == "11") {
        tr = $("<tr></tr>");
        $("<td colspan='4' style='text-align:left;padding-right:5px;'>" + b[0].RegionName + "</td>").appendTo(tr);
        $("#dataReport table").append(tr);
        for (var j = 0; j < b.length; j += 2) {
            it = b[j];
            tr = $("<tr></tr>");
            $("<td style='text-align:left;padding-right:5px;'>" + it.ClassName + "</td>").appendTo(tr);
            $("<td style='text-align:right;padding-left:0px;padding-right:32px;'>" + it.Quantity + "</td>").appendTo(tr);
            if (b.length > j + 1) {
                it1 = b[j + 1];

                $("<td style='text-align:left;padding-left:5px'>" + it1.ClassName + "</td>").appendTo(tr);
                $("<td style='text-align:right;padding-left:5px;padding-right:36px;'>" + (it1.Quantity == 0 ? "" : it1.Quantity) + "</td>").appendTo(tr);
            }
            $("#dataReport table").append(tr);
        }
    } else if (a == "12") {
        for (var j = 0; j < b.length; j++) {
            it = b[j];
            tr = $("<tr></tr>");
            $("<td style='text-align:left;padding-right:5px;'>" + it.BrandName1 + "</td>").appendTo(tr);
            $("<td style='text-align:left;padding-left:5px;'>" + it.ClassName1 + "</td>").appendTo(tr);
            $("<td style='text-align:right;padding-left:0px;padding-right:5px;'>" + it.Quantity1 + "</td>").appendTo(tr);
            $("<td style='text-align:left;padding-right:5px;'>" + it.BrandName2 + "</td>").appendTo(tr);
            $("<td style='text-align:left;padding-left:5px'>" + it.ClassName2 + "</td>").appendTo(tr);
            $("<td style='text-align:right;padding-left:0px;padding-right:36px;'>" + (it.Quantity2 == 0 ? "" : it.Quantity2) + "</td>").appendTo(tr);
            $("#dataReport table").append(tr);
        }
    } else if (a == "21") {
        for (var j = 0; j < b.length; j++) {
            it = b[j];
            tr = $("<tr></tr>");
            $("<td style='text-align:left;padding-right:5px;'>" + it.ClassName + "</td>").appendTo(tr);
            $("<td style='text-align:right;padding-right:5px;'>" + it.Quantity + "</td>").appendTo(tr);
            $("<td style='text-align:right;padding-left:0px;padding-right:5px;'>" + it.Washing + "</td>").appendTo(tr);
            $("<td style='text-align:right;padding-right:36px;'>" + it.Washed + "</td>").appendTo(tr);
            //$("<td style='text-align:left;padding-left:5px'>" + it.Handler + "</td>").appendTo(tr);
            $("#dataReport table").append(tr);
        }
    }
    else if (a == "22") {
        for (var j = 0; j < b.length; j++) {
            it = b[j];
            tr = $("<tr></tr>");
            $("<td style='text-align:left;padding-right:5px;'>" + it.BrandName1 + "</td>").appendTo(tr);
            $("<td style='text-align:left;padding-left:5px;'>" + it.ClassName1 + "</td>").appendTo(tr);
            $("<td style='text-align:right;padding-left:0px;padding-right:5px;'>" + it.Quantity1 + "</td>").appendTo(tr);
            $("<td style='text-align:left;padding-right:5px;'>" + it.BrandName2 + "</td>").appendTo(tr);
            $("<td style='text-align:left;padding-left:5px'>" + it.ClassName2 + "</td>").appendTo(tr);
            $("<td style='text-align:right;padding-left:0px;padding-right:36px;'>" + (it.Quantity2 == 0 ? "" : it.Quantity2) + "</td>").appendTo(tr);
            $("#dataReport table").append(tr);
        }
    }

    $("#dataReport").append(b);
}

function showMarquee() {
    var url1 = localStorage.getItem("NoticeURL");
    $.ajax({
        type: "post",
        data: {},
        url: url1 + "?t=" + new Date().getTime(),
        cache: false,
        success: function (result) {
            var marquee = [];
            if (result.length > 0) {
                if (result[0] == "")
                { } else {
                    for (var i = 0; i < result.length; i++) {
                        if (result[i].Position == warehouseID) {
                            if (result[i].IsFull == true) {
                            } else {
                                marquee.push(result[i]);
                            }
                        }
                    }
                }
            }

            $("#marquee").empty();
            var link;
            for (var i = 0; i < marquee.length; i++) {
                var link;
                if (i >= 1) {
                    link = $("<font color='white' style='margin-left:1em;'><a href='#'></a></font>").html("<font color='yellow'>#</font>" + marquee[i].Content);
                } else {
                    link = $("<font color='white'><a href='#'></a></font>").html("<font color='yellow'>#</font>" + marquee[i].Content);
                }
                $("#marquee").append(link);
            }
        }
    });
}
