function showHideNavDiv() {
    if ($(window).width() < 751) {
        $('.navTree').hide();
        $('.col-md-1').hide();
        $('.col-md-1').addClass('navTree-one');
    }
    else {
        $('.navTree').show();
        $('.col-md-1').show();
        $('.col-md-1').removeClass('navTree-one');
    }
}


$(function () {
    showHideNavDiv();
});

$(document).ready(function () {
    //отображени дерева задач при ширене окна < 751px
    var isNavTreeShow = false;
    $(window).on('resize', function () {        
        showHideNavDiv()
    });    
    $('[class="navbar-toggle tasks"]').on('click', function (e) {
        if (isNavTreeShow) {
            $('.navTree').hide();
            $('.col-md-1').hide();
            $('.col-md-9').show();
            isNavTreeShow = !isNavTreeShow;
        }
        else {
            $('.navTree').show();
            $('.col-md-1').show();
            $('.col-md-9').hide();
            isNavTreeShow = !isNavTreeShow;
        }
    });
    var currentID = -1;
    var lastID = -1;
    //отображение выбранной вкладки
    $('.nav  a,.col-navTree>li>a').on('click', function (e) {
        var currentAttrValue = $(this).attr('href');
        var tabSelector = $('.tabs ' + currentAttrValue);
        var tabEditSelector = $('.tabs #EditLead');
                
        tabSelector.show().siblings().hide();
        $(this).parent('li').addClass('active').siblings().removeClass('active');
        location.hash = currentAttrValue;
        //отображение функционального блока
        if (currentAttrValue == "#EditLead" || 
            currentAttrValue == "#AddLead" ||
            currentAttrValue == "#AddSubLead" ||
            currentAttrValue == "#DeleteLead") {
            $('.navTreeStartFunc').removeClass('hidden');
            $('.navTreeFunc').addClass('hidden');
            lastID = currentID;
            currentID = -1;
        }
        else {
            $('.navTreeStartFunc').addClass('hidden');
            $('.navTreeFunc').removeClass('hidden');
            lastID = currentID;
            currentID = Number(currentAttrValue.replace(/\D+/g, ""));
            //Получение данных для редактирования
            tabEditSelector.find($('.title')).val(tabSelector.find($('.title')).find($('.field')).text());
            tabEditSelector.find($('.executors')).val(tabSelector.find($('.executors')).find($('.field')).text());
            tabEditSelector.find($('.status')).val(tabSelector.find($('.status')).find($('.field')).text());
            tabEditSelector.find($('.descript')).text(tabSelector.find($('.descript')).find($('.field')).text());
            tabEditSelector.find($('.id')).val(currentID.toString());
            //Получение данных для удаления
            $('.tabs #DeleteLead').find($('.id')).val(currentID.toString());
        }        
        e.preventDefault();
    });
    //добавление задачи/подзадачи
    $('#ajaxAddLead, #ajaxAddSubLead').submit(function (e) {
        e.preventDefault();
        var currentAttrValue = $(this).attr('id');
        console.log(currentAttrValue);
        if (currentAttrValue == 'ajaxAddSubLead')
            $('.descendant').attr('value', lastID);
        else
            $('.descendant').attr('value', -1);
        var formData = $(this).serialize();
        var formMessages = $('.AddLead-messages');
        $.ajax({
            type: 'POST',
            url: $(this).attr('action'),
            data: formData
        })
		.done(function (response) {
		    $(formMessages).removeClass('error');
		    $(formMessages).addClass('success');
		    $(formMessages).text('Задача добавлена');
		    window.location.href = "/";
		})
		.fail(function (data) {
		    $(formMessages).removeClass('success');
		    $(formMessages).addClass('error');
		    if (data.responseText !== '') {
		        $(formMessages).text(data.responseText);
		    } else {
		        $(formMessages).text('Ошибка, задача не добавлена');
		    }
		});
    });
    //редактирование задачи
    $('#ajaxEditLead').submit(function (e) {
        e.preventDefault();
        var formData = $(this).serialize();
        var formMessages = $('#EditLead-messages');
        $.ajax({
            type: 'POST',
            url: $(this).attr('action'),
            data: formData
        })
		.done(function (response) {
		    $(formMessages).removeClass('error');
		    $(formMessages).addClass('success');
		    $(formMessages).text('Задача изменена');
		    window.location.href = "/";
		})
		.fail(function (data) {
		    $(formMessages).removeClass('success');
		    $(formMessages).addClass('error');
		    if (data.responseText !== '') {
		        $(formMessages).text(data.responseText);
		    } else {
		        $(formMessages).text('Ошибка, задача не изменена');
		    }
		});
    });
    //Удаление задачи
    $('#ajaxDeleteSubLead').submit(function (e) {
        e.preventDefault();
        var formData = $(this).serialize();
        var formMessages = $('#DeleteLead-messages');
        $.ajax({
            type: 'POST',
            url: $(this).attr('action'),
            data: formData
        })
		.done(function (response) {
		    $(formMessages).removeClass('error');
		    $(formMessages).addClass('success');
		    $(formMessages).text('Задача удалена');
		    window.location.href = "/";
		})
		.fail(function (data) {
		    $(formMessages).removeClass('success');
		    $(formMessages).addClass('error');
		    if (data.responseText !== '') {
		        $(formMessages).text(data.responseText);
		    } else {
		        $(formMessages).text('Ошибка, задача не удалена');
		    }
		});
    });
});