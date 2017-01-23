//Sliding Effect Control
head.js("Content/js/skin-select/jquery.cookie.js");
head.js("Content/js/skin-select/skin-select.js");

//---------------------������Ϣ��ѯ--------------------------------- 
head.js("Content/js/newsticker/jquery.newsTicker.js", function () {
    var nt_title = $('#nt-title').newsTicker({
        row_height: 18,
        max_rows: 1,
        duration: 5000,
        pauseOnHover: 0
    });

    $.ajax({
        url: "http://wthrcdn.etouch.cn/weather_mini",
        data: { "citykey": 101020100 },
        dataType: 'jsonp',
        crossDomain: true,
        success: function (result) {
            if (result.data.forecast.length > 0) {
                var weater = "";
                for (var i = 0; i < result.data.forecast.length; i++) {
                    var forecast = result.data.forecast[i];
                    weater +=
                        "<li>&nbsp;�Ϻ�&nbsp;&nbsp;" + forecast.date +
                        "&nbsp;&nbsp;<b>" + forecast.type +
                        "</b>&nbsp;&nbsp;" + forecast.high +
                        "&nbsp;&nbsp;" + forecast.low +
                        "&nbsp;&nbsp;" + forecast.fengxiang + "&nbsp;&nbsp;</li>";
                }
                $("#nt-title").html(weater);
            }
        },
        error: function () {
            alert("��ѯ��������");
        }
    });
});
//---------------------�����˵�--------------------------------- 
head.js("Content/js/custom/scriptbreaker-multiple-accordion-1.js", function () {

    $(".topnav").accordionze({
        accordionze: true,
        speed: 500,
        closedSign: '<img src="Content/img/plus.png">',
        openedSign: '<img src="Content/img/minus.png">'
    });

});

//---------------------�����˵��򿪹ر�--------------------------------- 
head.js("Content/js/slidebars/slidebars.min.js", "Content/js/slidebars/jquery.easing.min.js", function () {

    $(document).ready(function() {
        var mySlidebars = new $.slidebars();

        $('.toggle-left').on('click', function() {
            mySlidebars.toggle('right');
        });
    });
});

//---------------------��������--------------------------------- 
head.js("Content/js/search/jquery.quicksearch.js", function () {
    $('input.id_search').quicksearch('#menu-showhide li, .menu-left-nest li');
});

//---------------------���������򿪹ر�---------------------------
head.js("Content/js/tip/jquery.tooltipster.js", function () {

    $('.tooltip-tip-x').tooltipster({
        position: 'right'

    });

    $('.tooltip-tip').tooltipster({
        position: 'right',
        animation: 'slide',
        theme: '.tooltipster-shadow',
        delay: 1,
        offsetX: '-12px',
        onlyOne: true

    });
    $('.tooltip-tip2').tooltipster({
        position: 'right',
        animation: 'slide',
        offsetX: '-12px',
        theme: '.tooltipster-shadow',
        onlyOne: true

    });
    $('.tooltip-top').tooltipster({
        position: 'top'
    });
    $('.tooltip-right').tooltipster({
        position: 'right'
    });
    $('.tooltip-left').tooltipster({
        position: 'left'
    });
    $('.tooltip-bottom').tooltipster({
        position: 'bottom'
    });
    $('.tooltip-reload').tooltipster({
        position: 'right',
        theme: '.tooltipster-white',
        animation: 'fade'
    });
    $('.tooltip-fullscreen').tooltipster({
        position: 'left',
        theme: '.tooltipster-white',
        animation: 'fade'
    });
    //For icon tooltip



});

//---------------------��ǰʱ����Ϣ---------------------------
head.js("Content/js/clock/jquery.clock.js", function () {

    //clock
    $('#digital-clock').clock({
        offset: '+8',//����������������ʱ����������
        type: 'digital'
    });

});
//---------------------ifram�߶�---------------------------
function ResetSize() {
    $("#iframMain").height($(window).height() - 135);
    $("#iframMain").css("max-height", $(window).height() - 135);
}

