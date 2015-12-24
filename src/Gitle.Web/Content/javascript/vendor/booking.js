$(function () {
  $('.booking-buttons').hide();

  var activeRow = undefined;

  $('.booking-row').hover(function () {
    $(this).addClass('booking-highlight');
    $(this).find('.booking-buttons').show();
  }, function () {
    $(this).removeClass('booking-highlight');
    $(this).find('.booking-buttons').hide();
  });

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
        row.find('.booking-row-edit').hide();
        row.find('.booking-row-edit').show();
        addEditListeners();
        bookingRowInit(row.find('.booking-row-edit'));

      }
    });
  });

  var addEditListeners = function () {
    $('.booking-edit-cancel').on('click', function (e) {
      e.preventDefault();
      var row = $(this).parents('.booking-row');
      row.find('.booking-row-edit').remove();
      row.find('.booking-row-content').show();
      activeRow = undefined;
    });

    $('.booking-edit-save').on('click', function (e) {
      e.preventDefault();
    });
  };

  var datepickerOptions = {
    format: 'dd-mm-yyyy',
    weekStart: 1
  };

  var bookingRowInit = function (row) {

    row.find('.project-chooser').data('suggestion', undefined);

    row.find('.project-chooser').autocomplete({
      serviceUrl: '/project/autocomplete',
      autoSelectFirst: true,
      onSelect: function (suggestion) {
        if (row.find('.project-chooser').data('suggestion') != undefined && row.find('.project-chooser').data('suggestion') == suggestion.data) return false;
        row.find('.project-chooser').data('suggestion', suggestion.data);
        row.find('.booking_Project_Id').val(suggestion.data);
        row.find('.project-chooser').val(suggestion.value);
        row.find('.issue-chooser').val('');
        row.find('.booking_Issue_Id').val('');
        row.find('.issue-chooser').autocomplete('setOptions', { params: { projectId: row.find('.booking_Project_Id').val() } });
      }
    });

    row.find('.issue-chooser').autocomplete({
      serviceUrl: '/issue/autocomplete',
      width: row.find('.project-chooser').width(),
      noCache: true,
      params: { projectId: row.find('.booking_Project_Id').val() },
      autoSelectFirst: true,
      onSelect: function (suggestion) {
        row.find('.booking_Issue_Id').val(suggestion.data);
        row.find('.issue-chooser').val(suggestion.value);
      }
    });

    var bookingInput = row.find('.booking_Minutes');
    $('.booking-parser').change(function (e) {
      var value = $(this).val().replace(',', '.');

      var timeRegex = /^([0-9]+):([0-9]{2})$/i;
      var decimalRegex = /^([0-9]+)\.([0-9]+)$/i;
      var numberRegex = /^[0-9]+$/i;

      var visualOutput = value;
      var hoursOutput = value;

      if (numberRegex.test(value)) {
        var number = parseInt(value);
        if (number > 9) {
          var fullhours = parseInt(number / 60);
          var fullminutes = number - (fullhours * 60);
          visualOutput = fullhours + ':' + (fullminutes < 10 ? "0" + fullminutes : fullminutes);
          hoursOutput = number;
        } else {
          visualOutput = number + ':00';
          hoursOutput = number * 60;
        }
      }
      if (timeRegex.test(value)) {
        var parts = value.split(':');
        var hours = parseFloat(parts[0]);
        var minutes = parseFloat(parts[1]);
        hoursOutput = (hours * 60 + minutes);
      }
      if (decimalRegex.test(value)) {
        var hours = parseFloat(value);
        hoursOutput = (hours * 60);
        var fullhours = parseInt(value);
        var fullminutes = (hours - fullhours) * 60;
        visualOutput = fullhours + ':' + (fullminutes < 10 ? "0" + fullminutes : fullminutes);
      }

      $(this).val(visualOutput);
      bookingInput.val(hoursOutput);
    });

    row.find('.date').fdatepicker(datepickerOptions);

  };

  bookingRowInit($('.booking-row-new'));

  $('.date').fdatepicker(datepickerOptions).on('show', function () {
    $('[data-dayshift]').removeClass('active');
  });

  $('[data-dayshift]').each(function () {
    var shift = $(this).data('dayshift');
    var label = '';
    switch (shift) {
      case 0:
        label = 'Vandaag';
        break;
      case -1:
        label = 'Gisteren';
        break;
      default:
        label = Date.today().add(shift).days().toString('ddd dd MMM');
    }
    $(this).text(label);
  }).click(function (e) {
    e.preventDefault();
    var today = Date.today();
    var shift = $(this).data('dayshift');
    var date = today.add(shift).days();
    $('#booking_Date').val(date.toString('dd-MM-yyyy'));
    $('[data-dayshift]').removeClass('active');
    $(this).addClass('active');
  }).filter('[data-dayshift=0]').click();



});