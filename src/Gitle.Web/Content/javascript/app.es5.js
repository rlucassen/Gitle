"use strict";

$(function () {
  $(document.body).addClass("js-enabled");
  $(window).resize(function () {
    if ($(window).height() < $(document).height()) {
      $(document.body).addClass("scroll");
    } else {
      $(document.body).removeClass("scroll");
    }
  });

  /*
   * Vendor initialization
   */
  $('.colorpicker').colorPicker();
  $(".chosen-select").chosen({ no_results_text: "Oops, nothing found!", width: '100%' });
  $('.tablesorter').tablesorter({ sortList: [[0, 0]] });

  /*
   * Auxilium.functions
   */
  $('input[data-remember]').cookieRemember();
  $('[data-slugify]').slugify();
  $(document).ctrlS();
  $("table.row-clickable").tableRowClickable();
  $('.marked').markdownify();
  $('[data-confirm]').confirm();

  /* 
   * Auxilium.upload
   */
  $('.uploadarea').upload();
  $('.uploadlist').uploadList();

  /*
   * Auxilium.nestedtable
   */
  $('table.nested').nestedTable({
    afterAdd: function afterAdd(row) {
      row.find('.colorpicker-open').prop('rel', row.find('.colorpicker').prop('name'));
      row.find('.colorpicker').colorPicker();
    }
  });

  /*
   * Gitle.functions
   */
  $('.booking-parser').bookingParser();
  $('#search').gitleSearch();
  $('[data-live-comments]').liveComments();
  $('[data-insert-startdate][data-insert-enddate]').startEndDatePreset();
  $('[data-numberfield]').initProjectTypeNumbers();

  /*
   * Joyride start button
   */
  $('#initiate-joyride').click(function () {
    $(document).foundation('joyride', 'start');
  });

  // Focus on .focus elements
  $('form .focus[value=""]').focus();

  // Back to top button
  $("#back-to-top-button").hide();

  $(function () {
    $(window).scroll(function () {
      if ($(this).scrollTop() > 150) {
        $('#back-to-top-button').fadeIn(200);
      } else {
        $('#back-to-top-button').fadeOut(200);
      }
    });

    $('#back-to-top-button a').click(function (e) {
      e.preventDefault();
      $('body,html').animate({
        scrollTop: 0
      }, 500);
      $("#jump-to-ticket").focus();
      return false;
    });
  });

  // Jump to ticket
  $("#jump-to-ticket").on('keyup', function (e) {
    if (e.keyCode == 13) {
      var target = $('div[data-issueid=' + $("#jump-to-ticket").val() + ']');
      if (target != null) {
        target.removeClass("blink");
        $('html, body').animate({
          scrollTop: target.offset().top
        }, 500);
        target.addClass("blink");
        $("#jump-to-ticket").val("");
      }
    }
  });
});

