$(function() {
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
  $('textarea[data-suggestions-project]').textAreaSuggestion();
  hightlightHash();

  /* 
   * Auxilium.upload
   */
  $('.uploadarea').upload();
  $('.uploadlist').uploadList();

  /*
   * Auxilium.nestedtable
   */
  $('table.nested').nestedTable({
    afterAdd: function (row) {
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
  var focusElements = $('form .focus[value=""]');
  if (focusElements.length > 0)
    focusElements.trigger('focus');
  else {
    if ($(document.activeElement).not('body').length <= 0)
      setTimeout(function () { $('#search').trigger('focus'); }, 100);
  }
    

});
