
/** 弹出层 **/
layui.use('layer', function () {
    var $ = layui.jquery, layer = layui.layer;
    var table = layui.table;

    $("#person-center").on("click", function () {
        layer.open({
            type: 2,
            title: ["个人中心", "font-size:30px;color:#333;text-align:center;padding-top:50px"],
            area: ['500px', '480px'],
            shadeClose: true, //点击遮罩关闭
            content: '../Personal/Index',
            btn: ['确定']
        });
       
    })

    $("#questionChoose").on("click", function () {
        
        layer.open({
            type: 1,
            title: ["确认发布吗？", "font-size:24px;color:#333;text-align:left;padding-top:50px"],
            area: ['300px', '180px'],
            shadeClose: true, //点击遮罩关闭
            content: $("#questionSend"),
            end: function () {
                $("#questionSend").hide();
            }
        });
       
    })
    

})


/** 项目模块 **/

//开始时间
layui.use('laydate', function () {
    var laydate = layui.laydate;
    var value = $("#StartDate").val();
    
    laydate.render({
        elem: '#StartDate',
        format: 'yyyy/MM/dd',
       
        showBottom: false,
        
    });
});


//结束时间
layui.use('laydate', function () {
    var laydate = layui.laydate;  
    laydate.render({
        elem: '#EndDate',
        format: 'yyyy/MM/dd',
        showBottom: false,
        
    });
});

//时间
layui.use('laydate', function () {
    var laydate = layui.laydate;
    laydate.render({
        elem: '#Time',
        type: 'datetime',
    });
});

//出生时间
layui.use('laydate', function () {
    var laydate = layui.laydate;
    laydate.render({
        elem: '#Birthday',
        
    });
});

//雇佣时间
layui.use('laydate', function () {
    var laydate = layui.laydate;
    laydate.render({
        elem: '#HireDate',
        
    });
});
//开课时间
layui.use('laydate', function () {
    var laydate = layui.laydate;
    laydate.render({
        elem: '#courseStart',
        
        type: 'datetime',
        format: 'yyyy-MM-dd HH:mm:ss',
    });
});
//结课时间
layui.use('laydate', function () {
    var laydate = layui.laydate;
    laydate.render({
        elem: '#courseEnd',
        
        type: 'datetime',
        format: 'yyyy-MM-dd HH:mm:ss',
    });
});


//课程学员联动
$(function () {
    var info = $(".info");

    var id = $(".projectId");
    var length = id.length;

    
    

    for (let i = 0; i < length; i++) {

        $(info[i]).on("click", function () {
            $("#result").text("");
            $.ajax({
                type: "GET",
                url: '../Students/GetStuByProj/' + $(id[i]).text(),
                dataType: "json",
                success: function (data) {

                    var ListStudent = "<ul>";
                    $.each(data["data"], function (i, n) {
                        
                        if (n["Gender"] == 1) {
                            n["Gender"] = "男"
                        } else {
                            n["Gender"] = "女"
                        }
                        ListStudent += "<li>" + "<span class='noShow'>" + n["id"] + "</span>" + "<span class='numBg'>" + (i + 1) + "</span>" + "&nbsp" + "<span class='textSty'>" + n["StudentName"] + "(" + n["Gender"] + ")" + "&nbsp" + "&nbsp" + "&nbsp" + n["Mobile"] + "<img class='deleteStudent' onclick = 'deleteStu(\""+n["id"]+"\" )' src='../Content/images/deleteIcon.png'/>" + "</span>" + "</li>";
                    });
                    ListStudent += "</ul>";
                    $("#result").append(ListStudent);
                }
            })
            return false;
        })



    };
    
    
})


//表格鼠标悬停显示所有内容

$(function () {
    var td = $(".show-all");


    for (let i = 0; i < td.length; i++) {
        $(td[i]).hover(function () {
            var detail = $(this).text();
            td[i].title = detail
        })
    }
})


