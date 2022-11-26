$(function() {
    $('.button-add-cars').click(function(event) {
        $('.add-cars-form').addClass('active1');
    });

    $('.exit-cars-form').click(function(event) {
        $('.add-cars-form').removeClass('active1');
    });
    $('.add-accept-cars-form').click(function(event) {
        $('.add-cars-form').removeClass('active1');
        alert('Đã thêm xe');
    });

    $('.btn-change-car').click(function(event) {
        $('.change-car-form').addClass('active-change-form-car');
    });

    $('.exit-car-form').click(function(event) {
        $('.change-car-form').removeClass('active-change-form-car');
    });
    $('.save-change-car-form').click(function(event) {
        $('.change-car-form').removeClass('active-change-form-car');
        alert('Sửa thông tin xe thành công ');
    });


    $('.button-add-partner').click(function(event) {
        $('.add-partner-form').addClass('active2');
    });

    $('.exit-partner-form').click(function(event) {
        $('.add-partner-form').removeClass('active2');
    });

    $('.add-accept-partner-form').click(function(event) {
        $('.add-partner-form').removeClass('active2');
        alert('Đã thêm đối tác');
    });


    $('.open-menu').click(function (event) {

        $('.menu').addClass('show-menu');
        $('.open-menu').removeClass("button-menu")
        $('.close-menu').addClass('button-menu');
    });
    $('.close-menu').click(function (event) {
        $('.menu').addClass('hide-menu').one('webkitAnimationEnd', function (event) {
            $('.menu').removeClass('show-menu');
            $('.menu').removeClass('hide-menu');
        });;;
        $('.open-menu').addClass("button-menu");
        $('.close-menu').removeClass('button-menu');
    });

    $('.btn-change-partner').click(function(event) {
        $('.change-partner-form').addClass('active-change-form-partner');
    });
    $('.exit-partner-form').click(function(event) {
        $('.change-partner-form').removeClass('active-change-form-partner');
    });

    $('.save-change-partner-form').click(function(event) {
        $('.change-partner-form').removeClass('active-change-form-partner');
        alert('Sửa thông tin đối tác thành công');
    });


    $('.btn-change-plane-drive').click(function(event) {
        $('.modal-bill').addClass('active-modal-bill');
    });
    $('#close-modal-bill-transport').click(function(event) {
        $('.modal-bill').removeClass('active-modal-bill');
    });

    $('.btn-save-bill-transport').click(function(event) {
        $('.modal-bill').removeClass('active-modal-bill');
        alert('Sửa kế hoạch chạy xe thành công');
    });


});

// JS date
$(function() {
    $('input[name="daterange"]').daterangepicker({
      opens: 'left',
        language: 'vi-VN',
        locale: {
            format: 'DD/MM/YYYY',
            separator: " đến ",
            daysOfWeek: ["CN", "T2", "T3", "T4", "T5", "T6", "T7"],
            monthNames: ["Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6", "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12"],
            firstDay: 1,
            cancelLabel: "Hủy",
            applyLabel: "Chọn"
        }
    }, function(start, end, label) {
        console.log("A new date selection was made: " + start.format('YYYY-MM-DD') + ' to ' + end.format('YYYY-MM-DD'));
    });

    $('input[name="Time"]').daterangepicker({
        opens: 'left',
        language: 'vi-VN',
        locale: {
            format: 'DD/MM/YYYY',
            separator: " đến ",
            daysOfWeek: ["CN", "T2", "T3", "T4", "T5", "T6", "T7"],
            monthNames: ["Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6", "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12"],
            firstDay: 1,
            cancelLabel: "Hủy",
            applyLabel: "Chọn"
        }
    }, function (start, end, label) {
        console.log("A new date selection was made: " + start.format('YYYY/MM/DD') + ' to ' + end.format('YYYY-MM-DD'));
    });

});

jQuery(function ($) {
    $.datepicker.regional["vi-VN"] =
    {
        closeText: "Chọn",
        prevText: "Trước",
        nextText: "Sau",
        currentText: "Hôm nay",
        monthNames: ["Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6", "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12"],
        monthNamesShort: ["1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12"],
        dayNames: ["Chủ nhật", "Thứ hai", "Thứ ba", "Thứ tư", "Thứ năm", "Thứ sáu", "Thứ bảy"],
        dayNamesShort: ["CN", "Hai", "Ba", "Tư", "Năm", "Sáu", "Bảy"],
        dayNamesMin: ["CN", "T2", "T3", "T4", "T5", "T6", "T7"],
        weekHeader: "Tuần",
        dateFormat: "dd/mm/yy",
        firstDay: 1,
        isRTL: false,
        showMonthAfterYear: false,
        yearSuffix: ""
    };
    $.datepicker.setDefaults($.datepicker.regional["vi-VN"]);
});

// Js checklist
$(function() {
    $('.checked_all').on('change', function() {
        $('.checkbox').prop('checked', $(this).prop("checked"));
    });

    $('.checkbox').change(function() {
        if ($('.checkbox:checked').length == $('.checkbox').length) {
            $('.checked_all').prop('checked', true);
        } else {
            $('.checked_all').prop('checked', false);
        }
    });
});


// Js date picker

$(function() {
    $('input[name="date"]').daterangepicker({
        singleDatePicker: true,
        showDropdowns: true,
        minYear: 1991,
        maxYear: parseInt(moment().format('YYYY'), 10)
    }, function(start, end, label) {
        var years = moment().diff(start, 'years');
    });
});


// onload
$(function() {
    window.onload = function() {
        setActiveMenuItem();
    };
});

$(document).ready(function () {
    var windowLocation = window.location.pathname.toLowerCase();
    $(".accordion a[href='" + windowLocation + "']").closest('li').addClass("active");
    var element = $("#accordionMenuLeft li");
    for (i = 0; i < element.length; i++) {
        var link = $(element[i]).find('a').attr('href');
        if (link != undefined) {
            var linkLowercase = link.toLowerCase();
            if (windowLocation.indexOf(linkLowercase) != -1 && linkLowercase != '' && windowLocation.toLowerCase().includes(linkLowercase)) {
                $(element[i]).addClass("active");
                $(element[i]).parent().closest("li").addClass("active");
            };
        }
    }

});

//function setActiveMenuItem() {
//    var links = $('a.menu-item');

//    var activeLink;
//    var url = location.pathname;
//    for (let index = 0; index < links.length; index++) {
//        const element = links[index];

//        if (url.toLowerCase().includes($(element).attr('href').toLowerCase())) {
//            activeLink = element;
//        }
//    }
//    $(activeLink).closest('div').addClass('active-box-task');
//};

// js handle
$(document).ready(function() {
    $('.add-cars-form').draggable({
        handle: ".header-add-cars-form"
    });
});

$(document).ready(function() {
    $('.modal-bill-transport').draggable({
        handle: ".number-bill-transport"
    });
});

$(document).ready(function() {
    $('.add-partner-form').draggable({
        handle: ".header-add-partner-form"
    });
});

$(document).ready(function() {
    $('.change-partner-form').draggable({
        handle: ".header-change-partner-form"
    });
});

$(document).ready(function() {
    $('.change-car-form').draggable({
        handle: ".header-change-car-form"
    });
});

$(document).ready(function startTime() {
    // Lấy Object ngày hiện tại
    var today = new Date();
    var current_day = today.getDay();
    var day_name = '';

    switch (current_day) {
        case 0:
            day_name = "Chủ nhật";
            break;
        case 1:
            day_name = "Thứ 2";
            break;
        case 2:
            day_name = "Thứ 3";
            break;
        case 3:
            day_name = "Thứ 4";
            break;
        case 4:
            day_name = "Thứ 5";
            break;
        case 5:
            day_name = "Thứ 6";
            break;
        case 6:
            day_name = "Thứ 7";
    }



    // Giờ, phút, giây hiện tại
    var d = today.getDate();
    var month = today.getMonth() + 1;
    var y = today.getFullYear();
    var h = today.getHours();
    var m = today.getMinutes();
    var s = today.getSeconds();

    // Chuyển đổi sang dạng 01, 02, 03
    d = checkTime(d);
    month = checkTime(month);
    m = checkTime(m);
    s = checkTime(s);


    var ampm = "AM";
    if (h > 12) {
        h -= 12;
        ampm = "PM";
    }
    // Ghi ra trình duyệt

    document.getElementById('ngayVN').innerHTML = day_name + ", " + d + "/" + month + "/" + y;
    document.getElementById('gioVN').innerHTML = h + ":" + m + " " + ampm;

    // Dùng hàm setTimeout để thiết lập gọi lại 0.5 giây / lần
    var t = setTimeout(function() {
        startTime();
    }, 500);


    // Hàm này có tác dụng chuyển những số bé hơn 10 thành dạng 01, 02, 03, ...
    function checkTime(i) {
        if (i < 10) {
            i = "0" + i;
        }
        return i;
    };
});

// Start js login
const inputs = document.querySelectorAll(".input");


function addcl() {
    let parent = this.parentNode.parentNode;
    parent.classList.add("focus");
}

function remcl() {
    let parent = this.parentNode.parentNode;
    if (this.value == "") {
        parent.classList.remove("focus");
    }
}


inputs.forEach(input => {
    input.addEventListener("focus", addcl);
    input.addEventListener("blur", remcl);
});
// End js login

function heightWindow() {
    var heightWindow = elmnt.offsetHeight;
    if (heightWindow > 600) {
        $('.modal-bill-transport').addClass('');
    }
}