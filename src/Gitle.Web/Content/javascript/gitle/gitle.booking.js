$(function () {
  var activeRow = undefined;
  $('.booking-edit').on('click', function (e) {
    e.preventDefault();
    var row = $(this).parents('.booking-row');
    if (activeRow != undefined) {
      activeRow.find('.booking-row-edit').remove();
      activeRow.find('.booking-row-content').show();
    }
    activeRow = row;
    $.ajax({
      method: 'GET',
      data: {
        id: row.data('id')
      },
      url: '/booking/edit',
      success: function (data) {
        row.find('.booking-row-content').hide();
        row.prepend(data);
        addEditListeners(row);
        bookingRowInit(row);

      }
    });
  });

  var addEditListeners = function (row) {
    row.find('.booking-edit-cancel').on('click', function (e) {
      e.preventDefault();
      row.find('.booking-row-edit').remove();
      row.find('.booking-row-content').show();
      activeRow = undefined;
    });
  };

  var datepickerOptions = {
    format: 'dd-mm-yyyy',
    weekStart: 1
  };

  var setUnbillable = function (row) {
    row.find('.booking-row-null-toggle').addClass('null');
    row.find('.booking_Unbillable').val(1);

  }

  var setBillable = function (row) {
    row.find('.booking-row-null-toggle').removeClass('null');
    row.find('.booking_Unbillable').val(0);
  }

  var bookingRowInit = function (row) {

    row.find('.project-chooser').data('suggestion', undefined);

    row.find('.project-chooser').autocomplete({
      serviceUrl: '/project/autocomplete',
      autoSelectFirst: true,
      noCache: true,
      minChars: 0,
      onSelect: function (suggestion) {
        var projectChooser = row.find('.project-chooser');
        if (projectChooser.data('suggestion') != undefined && projectChooser.data('suggestion') == suggestion.data) return false;
        projectChooser.data('suggestion', suggestion.data).val(suggestion.value);
        row.find('.booking_Project_Id').val(suggestion.data);
        row.find('.issue-chooser').val('').autocomplete('setOptions', { params: { projectId: suggestion.data } });
        row.find('.booking_Issue_Id').val('');
        if (suggestion.extraValue == "ticketRequired") {
          row.find('.issue-chooser').prop("required", true);
          $(document).foundation('abide', 'reflow');
        } else {
          row.find('.booking_Comment').prop("required", true);
          $(document).foundation('abide', 'reflow');
        }
        if (suggestion.extraValue2 == "unbillable") {
          setUnbillable(row);
        } else {
          setBillable(row);
        }
      }
    }).on('focus', function () {
      $(this).autocomplete().onValueChange();
    });;

    row.find('.issue-chooser').autocomplete({
      serviceUrl: '/issue/autocomplete',
      params: { projectId: row.find('.booking_Project_Id').val() },
      autoSelectFirst: true,
      noCache: true,
      minChars: 1,
      width: row.find('.project-chooser').width(),
      onSelect: function (suggestion) {
        row.find('.booking_Issue_Id').val(suggestion.data);
        row.find('.issue-chooser').val(suggestion.value);
        row.find('.booking_Comment').prop("required", false);
        $(document).foundation('abide', 'reflow');

      }
    }).blur(function() {
      if ($(this).val() == '') {
        row.find('.booking_Issue_Id').val('');
      }
    });


    row.find('.booking-row-null-toggle').click(function (e) {
      e.preventDefault();
      if ($(this).hasClass('null')) {
        setBillable(row);
      } else {
        setUnbillable(row);
      }
    });

    

    row.find('.date').fdatepicker(datepickerOptions);

    row.find('.booking-parser').bookingParser();

  };

  bookingRowInit($('.booking-row-new'));

  $('.date').fdatepicker(datepickerOptions).on('show', function () {
    $('[data-dayshift]').removeClass('active');
  });

  $('.reportdate').fdatepicker({
    format: 'yyyy-mm-dd',
    weekStart: 1
  }).on('changeDate, change', function (ev) {
    var query = $('#query').val();
    var target = $(ev.target);
    var prefix = target.data('filter-prefix');
    var rx = new RegExp(prefix + ":[a-zA-Z0-9-_,.]+");
    query = query.replace(rx, '');
    query = query + ' ' + prefix + ':' + target.val();
    $('#query').val(query.replace(/ +(?= )/g, ''));
    $('#query-form').submit();
  });



  var dayShiftFormat = 'dd MMM';
  if (new FoundationHelper().getCurrentSizeClass() === 'large') {
    dayShiftFormat = 'ddd dd MMM';
  }

  $('[data-dayshift]').each(function () {
    var shift = $(this).data('dayshift');
    var label = '';
    var date = Date.today().add(shift).days();
    if (date.toString('dd-MM-yyyy') === $('#booking_Date').val()) {
      $(this).addClass('active');
    }
    switch (shift) {
      case 0:
        label = 'Vandaag';
        break;
      case -1:
        label = 'Gisteren';
        break;
      default:
        label = date.toString(dayShiftFormat);
    }
    $(this).text(label);
  }).click(function (e) {
    e.preventDefault();
    var shift = $(this).data('dayshift');
    var date = Date.today().add(shift).days();
    $('#booking_Date').val(date.toString('dd-MM-yyyy'));
    $('[data-dayshift]').removeClass('active');
    $(this).addClass('active');
  });
});